using SME;

namespace TCPIP
{
    public partial class DataOut
    {
        public interface ReadBus : IBus
        {
            [InitialValue(0x00)]
            int socket { get; set; }
            [InitialValue(0xFF)]
            byte data { get; set; }
        }
        public interface WriteBus : IBus
        {
            [InitialValue(0x00)]
            long frame_number { get; set; }
            int socket { get; set; }
             [InitialValue(0xFF)]
            byte data { get; set; }
        }
    }

    public partial class DataIn
    {
        public interface WriteBus : IBus
        {
            int socket { get; set; }
            uint sequence { get; set; }
            uint highest_sequence_ready {get; set; }
            byte data { get; set; }
            bool invalidate { get; set; }
            int data_length { get; set; }
            long frame_number { get; set; }
        }
        public interface ReadBus : IBus
        {
            [InitialValue(0x00)]
            int socket { get; set; }
            [InitialValue(0x00)]
            byte data { get; set; }
        }
    }


    public partial class PacketOut
    {
        public interface ReadBus : IBus
        {
            [InitialValue(0x00)]
            long frame_number { get; set; }

            [InitialValue(0x00)]
            byte protocol { get; set; }

            [InitialValue(0x00)]
            ulong ip_dst_addr_0 { get; set; }

            [InitialValue(0x00)]
            ulong ip_dst_addr_1 { get; set; }

            [InitialValue(0x00)]
            ulong ip_src_addr_0 { get; set; }

            [InitialValue(0x00)]
            ulong ip_src_addr_1 { get; set; }

            [InitialValue(0x00)]
            uint ip_id { get; set; }

            [InitialValue(0x00)]
            uint fragment_offset { get; set; }

            [InitialValue(0x00)]
            int data_length { get; set; } // the size we are writing currently

            [InitialValue(0x00)]
            byte data { get; set; } // The data needed

            [InitialValue(-1)]
            int addr { get; set; } // The data address(-1 if order is not important)
        }
        public interface WriteBus  : IBus
        {
            [InitialValue(0x00)]
            long frame_number { get; set; } // Increments so we can distinguish between new packages

            [InitialValue(0x00)]
            byte protocol { get; set; }

            [InitialValue(0x00)]
            ulong ip_dst_addr_0 { get; set; }

            [InitialValue(0x00)]
            ulong ip_dst_addr_1 { get; set; }

            [InitialValue(0x00)]
            ulong ip_src_addr_0 { get; set; }

            [InitialValue(0x00)]
            ulong ip_src_addr_1 { get; set; }

            [InitialValue(0x00)]
            uint ip_id { get; set; }

            [InitialValue(0x00)]
            uint fragment_offset { get; set; }

            [InitialValue(0x00)]
            int data_length { get; set; } // the size we are writing currently

            [InitialValue(0x00)]
            byte data { get; set; } // The data needed

            [InitialValue(-1)]
            int addr { get; set; } // The data address(-1 if order is not important)
        }
    }

    public partial class PacketIn
    {
        public interface WriteBus  : IBus
        {
            [InitialValue(0x00)]
            long frame_number { get; set; } // Increments so we can distinguish between new packages

            [InitialValue(0x00)]
            byte protocol { get; set; }

            [InitialValue(0x00)]
            ulong ip_dst_addr_0 { get; set; }

            [InitialValue(0x00)]
            ulong ip_dst_addr_1 { get; set; }

            [InitialValue(0x00)]
            ulong ip_src_addr_0 { get; set; }

            [InitialValue(0x00)]
            ulong ip_src_addr_1 { get; set; }

            [InitialValue(0x00)]
            uint ip_id { get; set; }

            [InitialValue(0x00)]
            uint fragment_offset { get; set; }

            [InitialValue(0x00)]
            int data_length { get; set; } // the size we are writing currently

            [InitialValue(0x00)]
            byte data { get; set; } // The data needed

            [InitialValue(-1)]
            int addr { get; set; } // The data address(-1 if order is not important)
        }
        public interface ReadBus  : IBus
        {
            [InitialValue(0x00)]
            long frame_number { get; set; } // Increments so we can distinguish between new packages

            [InitialValue(0x00)]
            byte protocol { get; set; }

            [InitialValue(0x00)]
            ulong ip_dst_addr_0 { get; set; }

            [InitialValue(0x00)]
            ulong ip_dst_addr_1 { get; set; }

            [InitialValue(0x00)]
            ulong ip_src_addr_0 { get; set; }

            [InitialValue(0x00)]
            ulong ip_src_addr_1 { get; set; }

            [InitialValue(0x00)]
            uint ip_id { get; set; }

            [InitialValue(0x00)]
            uint fragment_offset { get; set; }

            [InitialValue(0x00)]
            int data_length { get; set; } // the size we are writing currently

            [InitialValue(0x00)]
            byte data { get; set; } // The data needed

            [InitialValue(-1)]
            int addr { get; set; } // The data address(-1 if order is not important)
        }

    }
     public partial class FrameOut
    {
        public interface WriteBus  : IBus
        {
            [InitialValue(0x00)]
            long frame_number { get; set; } // Increments so we can distinguish between new packages

            [InitialValue(0x00)]
            ushort ethertype { get; set; }

            [InitialValue(0x00)]
            byte data { get; set; } // The data needed

            [InitialValue(0)]
            uint addr { get; set; } // The data address(-1 if order is not important)
        }
    }
}
