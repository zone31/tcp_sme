using System;

using SME;
using SME.Components;

namespace TCPIP
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            using (var sim = new Simulation())
            {

                //var mem = new TrueDualPortMemory<byte>(8192);
                //var simulator = new DatagramInputSimulator("data/udp_25/");
                // var simulator = new TUNSimulator();
                //                var network = new NetworkReader(simulator.frameBus);
                //var internet = new InternetIn(simulator.datagramBusIn);
                //simulator.datagramBusInControl = internet.datagramBusInControl;

                //var transport = new Transport(internet.segmentBusIn);
                //var dataInReader = new DataInReader(transport.dataInBus);


                // Graph simulator
                // int packet_out_mem_size = 8192;
                // var packet_out_mem = new TrueDualPortMemory<byte>(packet_out_mem_size);
                //var packet_out = new PacketOut(packet_out_mem,packet_out_mem_size);
                DataOutSimulator simulator = new DataOutSimulator();
                // var internetIn = new InternetIn(simulator.datagramBusIn,packet_out.bus_in_internet);
                // var internetOut = new InternetOut(simulator.datagramBusOut,
                //                                   packet_out.bus_out,
                //                                   packet_out.bus_out_control);
                var transport = new Transport();
                transport.dataOutProducerControlBus = simulator.bufferProducerControlBus;
                transport.dataOutReadBus = simulator.dataOutReadBus;

                simulator.consumerControlBus = transport.dataOutConsumerControlBus;
                
                var pp = new PrinterProcess();
                pp.bus = transport.packetOutWriteBus;
                pp.computeProducerControlBus = transport.packetOutProducerControlBus;

                transport.packetOutConsumerControlBus = pp.consumerControlBus;

                // Use fluent syntax to configure the simulator.
                // The order does not matter, but `Run()` must be
                // the last method called.

                // The top-level input and outputs are exposed
                // for interfacing with other VHDL code or board pins

                sim
                    .AddTopLevelOutputs(pp.bus)
                    .AddTopLevelInputs(simulator.dataOutReadBus)
                    .BuildCSVFile()
                    //.BuildVHDL()
                    .Run();

                // After `Run()` has been invoked the folder
                // `output/vhdl` contains a Makefile that can
                // be used for testing the generated design
            }
        }
    }

}
