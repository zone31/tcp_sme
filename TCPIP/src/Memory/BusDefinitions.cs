using SME;

namespace TCPIP
{
    public partial class Memory
    {
        public interface InternetPacketBus : IBus
        {
            long frame_number { get; set; } // Increments so we can distinguish between new packages
            byte ip_protocol { get; set; }
            ulong ip_dst_addr_0 { get; set; }
            ulong ip_dst_addr_1 { get; set; }
            ulong ip_src_addr_0 { get; set; }
            ulong ip_src_addr_1 { get; set; }
            uint ip_id { get; set; }
            uint fragment_offset { get; set; }
            int data_length { get; set; } // the size we are writing currently
            byte data { get; set; } // The data needed
        }


        public interface PacketInBus : IBus
        {
            [InitialValue(0)]
            uint bytes_left { get; set; }

            [InitialValue(long.MaxValue)]
            long frame_number { get; set; }

            [InitialValue(long.MaxValue)]
            long ip_id { get; set; }

            [InitialValue(0x00)]
            ushort type { get; set; }

            [InitialValue(0x00)]
            byte data { get; set; }
        }
    }
}