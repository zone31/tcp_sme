using System;

namespace TCPIP
{
    class IPv4
    {
        /*
         0                   1                   2                   3
         0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |Version|  IHL  |Type of Service|          Total Length         |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |         Identification        |Flags|     Fragment Offset     |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |  Time to Live |    Protocol   |        Header Checksum        |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                         Source Address                        |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                      Destination Address                      |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                    Options                    |    Padding    |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

         */

        public const uint HEADER_SIZE = 0x14; // Not counting the optionals


        public const byte VERSION = 0x04;

        public const uint VERSION_OFFSET = 0x00;
        public const uint IHL_OFFSET = 0x00;

        public const uint DIFFERENTIATED_SERVICES_OFFSET = 0x01;

        public const uint TOTAL_LENGTH_OFFSET_0 = 0x02;
        public const uint TOTAL_LENGTH_OFFSET_1 = 0x03;

        public const uint ID_OFFSET_0 = 0x04;
        public const uint ID_OFFSET_1 = 0x05;

        public const uint FLAGS_OFFSET = 0x06;

        public const uint TTL_OFFSET = 0x08;

        public const uint PROTOCOL_OFFSET = 0x09;

        public const uint CHECKSUM_OFFSET_0 = 0x0A;
        public const uint CHECKSUM_OFFSET_1 = 0x0B;

        public const uint SRC_ADDRESS_OFFSET_0 = 0x0C;
        public const uint SRC_ADDRESS_OFFSET_1 = 0x0D;
        public const uint SRC_ADDRESS_OFFSET_2 = 0x0E;
        public const uint SRC_ADDRESS_OFFSET_3 = 0x0F;

        public const uint DST_ADDRESS_OFFSET_0 = 0x10;
        public const uint DST_ADDRESS_OFFSET_1 = 0x11;
        public const uint DST_ADDRESS_OFFSET_2 = 0x12;
        public const uint DST_ADDRESS_OFFSET_3 = 0x13;

        public const ulong ADDRESS_MASK_0 = 0xFF;
        public const ulong ADDRESS_MASK_1 = 0xFF00;
        public const ulong ADDRESS_MASK_2 = 0xFF0000;
        public const ulong ADDRESS_MASK_3 = 0xFF000000;


        [Flags]
        public enum Flags : byte
        {
            RESERVED = 0x04,
            DF = 0x02,
            MF = 0x01,
        }

        public const uint FRAGMENT_OFFSET_OFFSET_0 = 0x06;
        public const uint FRAGMENT_OFFSET_OFFSET_1 = 0x06;
        public const uint FRAGMENT_OFFSET_MASK = 0x1FFF;



        public enum Protocol : byte
        {
            HOPOPT = 0,
            ICMP = 1,
            IGMP = 2,
            GGP = 3,
            IPv4 = 4,
            ST = 5,
            TCP = 6,
            CBT = 7,
            EGP = 8,
            IGP = 9,
            BBN_RCC_MON = 10,
            NVP_II = 11,
            PUP = 12,
            ARGUS = 13,
            EMCON = 14,
            XNET = 15,
            CHAOS = 16,
            UDP = 17,
            MUX = 18,
            DCN_MEAS = 19,
            HMP = 20,
            PRM = 21,
            XNS_IDP = 22,
            TRUNK_1 = 23,
            TRUNK_2 = 24,
            LEAF_1 = 25,
            LEAF_2 = 26,
            RDP = 27,
            IRTP = 28,
            ISO_TP4 = 29,
            NETBLT = 30,
            MFE_NSP = 31,
            MERIT_INP = 32,
            DCCP = 33,
            IDPR = 35,
            XTP = 36,
            DDP = 37,
            IDPR_CMTP = 38,
            IL = 40,
            IPv6 = 41,
            SDRP = 42,
            IPv6_Route = 43,
            IPv6_Frag = 44,
            IDRP = 45,
            RSVP = 46,
            GRE = 47,
            DSR = 48,
            BNA = 49,
            ESP = 50,
            AH = 51,
            I_NLSP = 52,
            NARP = 54,
            MOBILE = 55,
            TLSP = 56,
            SKIP = 57,
            IPv6_ICMP = 58,
            IPv6_NoNxt = 59,
            IPv6_Opts = 60,
            CFTP = 62,
            SAT_EXPAK = 64,
            KRYPTOLAN = 65,
            RVD = 66,
            IPPC = 67,
            SAT_MON = 69,
            VISA = 70,
            IPCV = 71,
            CPNX = 72,
            CPHB = 73,
            WSN = 74,
            PVP = 75,
            BR_SAT_MON = 76,
            SUN_ND = 77,
            WB_MON = 78,
            WB_EXPAK = 79,
            ISO_IP = 80,
            VMTP = 81,
            SECURE_VMTP = 82,
            VINES = 83,
            TTP = 84,
            IPTM = 84,
            NSFNET_IGP = 85,
            DGP = 86,
            TCF = 87,
            EIGRP = 88,
            OSPFIGP = 89,
            Sprite_RPC = 90,
            LARP = 91,
            MTP = 92,
            IPIP = 94,
            SCC_SP = 96,
            ETHERIP = 97,
            GMTP = 100,
            IFMP = 101,
            PNNI = 102,
            PIM = 103,
            ARIS = 104,
            SCPS = 105,
            QNX = 106,
            IPComp = 108,
            SNP = 109,
            Compaq_Peer = 110,
            IPX_in_IP = 111,
            VRRP = 112,
            L2TP = 115,
            DDX = 116,
            IATP = 117,
            STP = 118,
            SRP = 119,
            UTI = 120,
            SMP = 121,
            PTP = 123,
            FIRE = 125,
            CRTP = 126,
            CRUDP = 127,
            SSCOPMCE = 128,
            IPLT = 129,
            SPS = 130,
            PIPE = 131,
            SCTP = 132,
            FC = 133,
            RSVP_E2E_IGNORE = 134,
            UDPLite = 136,
            MPLS_in_IP = 137,
            manet = 138,
            HIP = 139,
            Shim6 = 140,
            WESP = 141,
            ROHC = 142,
            Reserved = 255,
        }


    }
}