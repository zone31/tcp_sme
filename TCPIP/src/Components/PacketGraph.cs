using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TCPIP
{
    // The packet graph file uses specific files that indicate in what order
    // a packet needs to be received or sent.
    //
    // It is seen as an acrylic graph, where the first node is an "event" (send, receive etc).
    // To propegate to the next node, we need to fulfill the node, and go to the next.
    // If there are a split , these tasks are done independently of each other, in no specific order
    //

    // events:
    //  * receive:
    //      A packet that we should receive from the stack.
    //  * send:
    //      A packet that we should send into the stack.
    //  * datain:
    //      Data from the stack to the user application
    //  * dataout:
    //      Data from the user application that goes into the stack
    //  * command:
    //      Defines a command as sent by the application to the stack
    //  * wait:
    //      wait N clockcycles before the node is ready.
    public class PacketGraph  {
        [Flags]
        public enum PacketInfo{
            Send = 1 << 0, // this packet has to be sent.
            Receive = 1 << 1, // This packet has to be received.
            DataIn = 1 << 2,
            DataOut = 1 << 3,
            Command = 1 << 4,
            Wait = 1 << 5,
            Initial = 1 << 6, // Is this an beginning packet?
            End = 1 << 7, // Is this an end packet?
            Valid = 1 << 8, // Has this packet been sent, received or its action done??
            Active = 1 << 9, //  The packet is currently being worked on
        }

        // class defining the nodes in an directed acyclic graph
        // containing the information of the packets to send and receive.
        // The nodes contain the file we send or validate up against.
        // It will only send or receive correctly when all "dependsOn"
        // is marked as good.
        public class Packet
        {
            public int id;
            public PacketInfo info;
            public string dataPath;
            public byte[] data;
            public ushort type;
            public SortedSet<int> dependsOn;
            public SortedSet<int> requiredBy;
        }

        private Dictionary<int,Packet> packetList = new Dictionary<int,Packet>();
        private SortedSet<int> initPackets = new SortedSet<int>();
        private SortedSet<int> exitPackets = new SortedSet<int>();
        private SortedSet<int> packetPointers; // List of packet pointers waiting
        private const int bufferSize = 100000;
        private byte[] receiveBuffer = new byte[bufferSize];
        private int receiveBufferSize = 0;


        public PacketGraph(string dir){
            string[] filePaths = Directory.GetFiles(dir);
            var simPackets = from c in filePaths
                             select GenerateSimPacket(c);

            // Find the end points in the graph
            var dependsOn = new HashSet<int>();
            var allNodes = new HashSet<int>();
            foreach(Packet simPacket in simPackets){
                allNodes.Add(simPacket.id);
                dependsOn.UnionWith(new HashSet<int>(simPacket.dependsOn));
                packetList.Add(simPacket.id,simPacket);
            }

            // Append the end packets
            exitPackets = new SortedSet<int>(allNodes.Except(dependsOn));
            foreach(int x in exitPackets)
            {
                packetList[x].info |= PacketInfo.End;
            }
            // Calculate which packets are required
            foreach(var x in packetList.OrderBy(a => a.Key))
            {
                var requiredBy = new HashSet<int>();
                foreach(var y in packetList.OrderBy(a => a.Key))
                {
                    if(y.Value.dependsOn.Contains(x.Key)){
                        requiredBy.Add(y.Key);
                    }
                }
                packetList[x.Key].requiredBy = new SortedSet<int>(requiredBy);
            }

            // Print the current packet information
            int maxDepends = 0;
            foreach(var x in packetList.OrderBy(a => a.Key))
            {
                // Get the max dependency for each node
                if(x.Value.dependsOn.Count > maxDepends){
                    maxDepends = x.Value.dependsOn.Count;
                }
            }

            //Add The current packet pointers
            packetPointers =  new SortedSet<int>(initPackets);

            // Test if we need to guess in the design
            if(maxDepends > 1 || initPackets.Count > 1 || exitPackets.Count > 1){
                Logging.log.Warn("The graph may have to guess which node it is receiving data to");
            }
        }

        private Packet GenerateSimPacket(string filePath){
            Logging.log.Trace("Parsing " + filePath);
            var fileName =  Path.GetFileName(filePath);
            var reg = new Regex(@"^(\d*)_?(.*)\-(\w*)\.bin$");
            var match = reg.Match(fileName);
            PacketInfo info = 0;
            var id = Int32.Parse(match.Groups[1].Value);
            var dependsString = match.Groups[2].Value.Split("_");
            var dependsOn = new SortedSet<int>();
            // test if we have found dependencies, it must be an initial packet
            if(dependsString[0].Equals(""))
            {
                initPackets.Add(id);
                info |= PacketInfo.Initial;
            }
            else
            {
                dependsOn = new SortedSet<int>(dependsString.Select(int.Parse));
            }

            var metainfo = match.Groups[3].Value;

            switch(metainfo)
            {
                case "send":
                    info |= PacketInfo.Send;
                    break;
                case "receive":
                    info |= PacketInfo.Receive;
                    break;
                case "datain":
                    info |= PacketInfo.DataIn;
                    break;
                case "dataout":
                    info |= PacketInfo.DataOut;
                    break;
                case "command":
                    info |= PacketInfo.Command;
                    break;
                case "wait":
                    info |= PacketInfo.Wait;
                    break;
                default:
                    Logging.log.Fatal("Packet metainfo not detected: " + fileName);
                    break;
            }
            // Create the packet
            Packet pack = new Packet();
            pack.id = id;
            pack.info = info;
            // if this packet can be sent or received, load the data
            if((pack.info & (PacketInfo.Send | PacketInfo.Receive)) > 0)
            {
                pack.data = File.ReadAllBytes(filePath);
                // Set the packet type based on the byte value
                pack.type = (ushort)(pack.data[EthernetIIFrame.ETHERTYPE_OFFSET_0] << 0x08);
                pack.type |= (ushort)(pack.data[EthernetIIFrame.ETHERTYPE_OFFSET_1]);
            }
            pack.dataPath = filePath;
            pack.dependsOn = dependsOn;
            return pack;
        }

        // Iterate over packet structure, and detect if it is a valid packet
        private bool isReady(int i, bool start){
            var packet =  packetList[i];
            bool accum = true;
            // If we have an packet as initial, it must be ready always at start
            if((packet.info & PacketInfo.Initial) > 0 && start){
                return true;
            }
            // If this is not the start node of the chain, test if it is valid
            if(start == false){
                accum = (packet.info & PacketInfo.Valid) > 0;
            }
            // If the accumulator is false the packet must not be valid, and we return false ealy
            if(accum == false){
                return false;
            }
            // Iterate over the dependencies, and call them when recursively
            foreach (var x in packet.dependsOn){
                accum &= isReady(x,false);
            }
            return accum;

        }
        // Overloaded isReady, so we do no have to set the root flag
        private bool isReady(int i)
        {
            return isReady(i,true);
        }





        ///////////////////////////////////////////////////
        // Public Functions
        public IEnumerable<(ushort type,byte data,uint bytes_left)> IterateOverPacketToSend(){
            // Get all packets that we have to send currently, and test if they are ready
            var toSend = packetPointers.Where(
                x => (packetList[x].info & PacketInfo.Send) > 0 &&
                     isReady(x)
            ).ToList();

            int pid = packetPointers.First();
            Logging.log.Trace("PacketID: " + pid + " iterator started");
            Packet p = packetList[pid];

            // Start after ethernet header
            List<byte> packetBytes = new List<Byte>(p.data.Skip((int)EthernetIIFrame.HEADER_SIZE));

            for (int i = 0; i < packetBytes.Count; i++)
            {
                yield return (p.type,packetBytes[i],(uint)(packetBytes.Count-i-1));
            }

            // Mark as valid
            packetList[pid].info |= PacketInfo.Valid;

            // Remove from queue
            packetPointers.Remove(pid);

            // We add what is up next, so it is valid packet pointers
            packetPointers.UnionWith(packetList[pid].requiredBy);
            Logging.log.Trace("PacketID: " + pid + " iterator ended");
            yield break;
        }

        public bool HasPackagesToSend(){
            bool ret = packetPointers.Where(x => (packetList[x].info & PacketInfo.Send) > 0 ).Count() > 0;
            return ret;
        }

        ///////////////////////////////////////////////////
        // Helper Functions
        public void Info(){
            foreach(var x in packetList.OrderBy(a => a.Key))
            {
                string depends = String.Join(",",x.Value.dependsOn);
                string required = String.Join(",",x.Value.requiredBy);
                Logging.log.Info($"PacketID: {x.Key,5} Depends: {depends,7} Required: {required,7} Flags:" + x.Value.info);
            }


            foreach(var x in packetList.OrderBy(a => a.Key))
            {
                Logging.log.Info("Is ready " + x.Key + " " + isReady(x.Key));
            }
        }
        // Dumps the current state as a graphwiz string
        public string GraphwizState(){
            string ret = "digraph G{\n";
            foreach(KeyValuePair<int,Packet> x in packetList.OrderBy(a => a.Key))
            {
                bool typeSet = false;
                // Set the graph types
                if((x.Value.info & PacketInfo.Initial) > 0 && !typeSet)
                {
                    ret += $"{x.Key} [shape=Mdiamond];\n";
                    typeSet = true;
                }
                if((x.Value.info & PacketInfo.End) > 0 && !typeSet)
                {
                    ret += $"{x.Key} [shape=Msquare];\n";
                    typeSet = true;
                }
                if((x.Value.info & PacketInfo.Send) > 0 && !typeSet)
                {
                    ret += $"{x.Key} [shape=triangle];\n";
                    typeSet = true;
                }
                if((x.Value.info & PacketInfo.Receive) > 0 && !typeSet)
                {
                    ret += $"{x.Key} [shape=invtriangle];\n";
                    typeSet = true;
                }
                if((x.Value.info & PacketInfo.DataIn) > 0 && !typeSet)
                {
                    ret += $"{x.Key} [shape=house];\n";
                    typeSet = true;
                }
                if((x.Value.info & PacketInfo.DataOut) > 0 && !typeSet)
                {
                    ret += $"{x.Key} [shape=unvhouse];\n";
                    typeSet = true;
                }


                // Define the paths
                foreach(int y in x.Value.requiredBy){

                    ret += $"   {x.Key} -> {y};\n";
                }

            }
            ret += "}\n";
            return ret;
        }
    }
}