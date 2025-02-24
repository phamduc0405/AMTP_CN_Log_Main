using ATCPIP;
using ExcelDataReader.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace ACO2_App._0.INIT
{
    public class T5Helper
    {
        #region User define
        public enum Command
        {
            None,
            Reset,
            TxReset,
            ZoneA,
            ReadyZoneA,
            ZoneC,
            ReadyZoneC,
            MtpWrite,
            CellLoading,
            CheckTmdInfo,
            TmdMd5,
            RegPin,
            Ca310Check,
            CorCheck,
            Ca310CalWrite,
            CheckHostVer,
            LineCheck,
            ContactCurrent,
            MtpResultData,
            SystemError,
            Current,
            Start,
            Run,
            Origin,
            Next,
            Back,
            TurnOn,
            TurnOff,
            CylOnMIT,
            CylOffMIT,
            MtpWritePrescale,
            MtpVerify,
            IdCheck,
            Compensation,
            BcErrorCheck,
            WhiteCurrentCheck,
            TESTER_INIT,
            MCA_OFF,
            C_CURRENT,
            PopupReWork,
            PopupCellInfo,
            BG7000_ON,
            PopupNGJig,
            TSP_Check,
            TSP_START,
            CONTACT_CURRENT_CHECK_AFTER_MTP,
        }

        public enum Prefix
        {
            None,
            Down,
            Ack,
            NAck,
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SendHeader
        {
            public byte Stx;
            public byte Type;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] Length;

            public void Init()
            {
                Stx = 0x02;
                Type = 0x50;
                Length = new byte[4];
            }

            public void SetLength(int length)
            {
                Length = Encoding.ASCII.GetBytes(length.ToString("X4"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SendTerminator
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] Crc;

            public byte Etx;

            /// <summary>
            /// 
            /// </summary>
            public void Init()
            {
                Crc = Encoding.ASCII.GetBytes(@"0000");
                Etx = 0x03;
            }
        }


        /// <summary>
        /// Send format : STX(1byte) + 0x50(1byte) + "length(4byte)" + DATA + CRC(4byte:0000) + ETX(1byte)
        /// </summary>
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class SendPacket
        {
            /// <summary>
            /// 
            /// </summary>
            public SendHeader Header;

            public byte[] Data;

            public SendTerminator Terminator;

            /// <summary>
            /// Constructor
            /// </summary>
            public void Init()
            {
                Header.Init();
                Terminator.Init();
            }
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ReceiveHeader
        {
            public byte Stx;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]  // OLD 2
            public byte[] Type;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] Length;

            public void Init()
            {
                Stx = 0x02;
                Type = new byte[2]; // OLD =2 REASON : MTPMANMUAL USE 2 OK 
                Length = new byte[4];
            }
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ReceiveTerminator
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] Crc;
            public byte Etx;

            public void Init()
            {
                Crc = new byte[4];
                Etx = 0x03;
            }
        }

        /// <summary>
        /// Receive format : STX(1byte) + 0x1050(2bytes) + "length(4byte)" + DATA + CRC(4byte:0000) + ETX(1byte)
        /// </summary>
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class ReceivePacket
        {
            public ReceiveHeader Header;
            public byte[] Data;
            public ReceiveTerminator Terminator;

            public void Init()
            {
                Header.Init();
                Terminator.Init();

                Data = null;
            }
        }

        #endregion

        #region Constant
        public const int SendCmdSize = 2;
        public const int LengthSize = 4;
        public const int CrcSize = 4;
        public const int StxSize = 1;
        public const int EtxSize = 1;
        public const int LeastSize = SendCmdSize + LengthSize + CrcSize + StxSize + EtxSize;
        public const int LineCheckSize = 26;
        #endregion

        public static ReceivePacket Parse(byte[] message)
        {
            var packet = ByteToReceivePacket(message);
            return packet;
        }

        public static void Send2T5(TCPConnect conn, byte[] datas, Equipment eqp,int Channel = 0)
        {
            conn.Send(datas);
            string str = $"[SEND]: {Encoding.ASCII.GetString(datas)}";
            
            eqp.LogT5EventHandle(str,(Channel).ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="channel"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static byte[] MakePacket(Command command, int channel, params string[] datas)
        {
            if (channel < 0 || channel >= 24)
                return null;

            var packet = new SendPacket();
            packet.Init();
            packet.Data = MakeData(channel, command, datas);
            packet.Header.SetLength(packet.Data.Length);
            return SendPacketToByte(packet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        public static byte[] MakePacket(Command command, int channel)
        {
            if (channel < -1 || channel >= 24)
                return null;

            var packet = new SendPacket();
            packet.Init();
            packet.Data = MakeData(channel, command, null);
            packet.Header.SetLength(packet.Data.Length);
            return SendPacketToByte(packet);
        }

        public static int GetSendChannel(byte[] data)
        {
            var packet = ByteToSendPacket(data);

            var split = Encoding.ASCII.GetString(packet.Data).Split(',');
            var channel = 0;

            if (split[0] == "Ch")
            {
                return channel = Convert.ToInt16(split[1]) - 1;
            }
            else
            {
                return channel = Convert.ToInt16(split[2]) - 1;
            }
        }

        public static bool CheckHeartBit(string message)
        {
            return message.ToUpper().Contains("LINECHECK");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="command"></param>
        /// <param name="datas"></param>
        /// <returns></returns>
        private static byte[] MakeData(int channel, Command command, params string[] datas)
        {
            var commandString = string.Empty;

            switch (command)
            {
                case Command.Reset: commandString = "RESET"; break;
                case Command.TxReset: commandString = "TX_RESET"; break;
                case Command.ZoneA: commandString = "ZONE_A"; break;
                case Command.ReadyZoneA: commandString = "A0_READY"; break;
                case Command.ZoneC: commandString = "ZONE_C"; break;
                case Command.ReadyZoneC: commandString = "C0_READY"; break;
                case Command.MtpWrite: commandString = "MTP_WRITE"; break;
                case Command.CellLoading: commandString = "CELL_LOADING"; break;
                case Command.CheckTmdInfo: commandString = "TMD_INFO"; break;
                case Command.TmdMd5: commandString = "TMD_MD5"; break;
                case Command.RegPin: commandString = "REG_PIN"; break;
                case Command.Ca310Check: commandString = "CA310_CHECK"; break;
                case Command.CorCheck: commandString = "COR_CHECK"; break;
                case Command.Ca310CalWrite: commandString = "ITEM, CA310_CAL_WRITE,1,"; break;
                case Command.CheckHostVer: commandString = "HOST_VER"; break;
                case Command.LineCheck: commandString = "LineCheck"; break;
                case Command.ContactCurrent: commandString = "CONTACT_CURRENT"; break;
                case Command.MtpResultData: commandString = "MTP_RESULT_DATA"; break;
                case Command.SystemError: commandString = "SYSTEM_ERROR"; break;
                case Command.Current: commandString = "CURRENT"; break;
                case Command.Start: commandString = "START"; break;
                case Command.Run: commandString = "RUN"; break;
                case Command.Origin: commandString = "ORIGIN_START"; break;
                case Command.Next: commandString = "NEXT"; break;
                case Command.Back: commandString = "BACK"; break;
                case Command.TurnOn: commandString = "TURN_ON"; break;
                case Command.TurnOff: commandString = "TURN_OFF"; break;
                case Command.CylOnMIT: commandString = "CYL_ON_MIT"; break;
                case Command.CylOffMIT: commandString = "CYL_OFF_MIT"; break;
                case Command.MtpWritePrescale: commandString = "MTP_WRITE_PRESCALE"; break;
                case Command.MtpVerify: commandString = "MTP_VERIFY"; break;
                case Command.IdCheck: commandString = "ID_CHECK"; break;
                case Command.Compensation: commandString = "COMPENSATION"; break;
                case Command.BcErrorCheck: commandString = "BC_ERROR_CHECK"; break;
                case Command.WhiteCurrentCheck: commandString = "WHITE_CURRENT_CHECK"; break;
                case Command.TESTER_INIT: commandString = "TESTER_INIT"; break;
                case Command.PopupReWork: commandString = "POPUPREWORK"; break;
                case Command.PopupCellInfo: commandString = "POPUPCELLINFO"; break;
                case Command.BG7000_ON: commandString = "BG7000_ON"; break;
                case Command.PopupNGJig: commandString = "POPUPNGJIG"; break;
                case Command.TSP_Check: commandString = "TSP_CHECK"; break;
                case Command.CONTACT_CURRENT_CHECK_AFTER_MTP: commandString = "CONTACT_CURRENT_CHECK_AFTER_MTP"; break;


            }

            var builder = new StringBuilder();
            builder.Append(string.Format("Ch,{0},{1}", channel + 1, commandString));
            if (datas != null)
            {
                foreach (var data in datas)
                {
                    builder.Append(string.Format(",{0}", data));
                }
            }
            Console.WriteLine($"{Encoding.ASCII.GetBytes(builder.ToString())}");
            return Encoding.ASCII.GetBytes(builder.ToString());
        }

        public static Command GetCommand(string content)
        {
            var command = Command.None;
            switch (content)
            {
                case "RESET": command = Command.Reset; break;
                case "TX_RESET": command = Command.TxReset; break;
                case "ZONE_A": command = Command.ZoneA; break;
                case "A0_READY": command = Command.ReadyZoneA; break;
                case "ZONE_C": command = Command.ZoneC; break;
                case "C0_READY": command = Command.ReadyZoneC; break;
                case "MTP_WRITE": command = Command.MtpWrite; break;
                case "CELL_LOADING": command = Command.CellLoading; break;
                case "TMD_INFO": command = Command.CheckTmdInfo; break;
                case "TMD_MD5": command = Command.TmdMd5; break;
                case "REG_PIN": command = Command.RegPin; break;
                case "CA310_CHECK": command = Command.Ca310Check; break;
                case "COR_CHECK": command = Command.CorCheck; break;
                case "CA310_CAL_WRITE": command = Command.Ca310CalWrite; break;
                case "HOST_VER": command = Command.CheckHostVer; break;
                case "LineCheck": command = Command.LineCheck; break;
                case "CONTACT_CURRENT": command = Command.ContactCurrent; break;
                case "MTP_RESULT_DATA": command = Command.MtpResultData; break;
                case "SYSTEM_ERROR": command = Command.SystemError; break;
                case "CURRENT": command = Command.Current; break;
                case "START": command = Command.Start; break;
                case "RUN": command = Command.Run; break;
                case "ORIGIN_START": command = Command.Origin; break;
                case "NEXT": command = Command.Next; break;
                case "BACK": command = Command.Back; break;
                case "TURN_ON": command = Command.TurnOn; break;
                case "TURN_OFF": command = Command.TurnOff; break;
                case "CYL_ON_MIT": command = Command.CylOnMIT; break;
                case "CYL_OFF_MIT": command = Command.CylOffMIT; break;
                case "MTP_WRITE_PRESCALE": command = Command.MtpWritePrescale; break;
                case "MTP_VERIFY": command = Command.MtpVerify; break;
                case "ID_CHECK": command = Command.IdCheck; break;
                case "BC_ERROR_CHECK": command = Command.BcErrorCheck; break;
                case "COMPENSATION": command = Command.Compensation; break;
                case "WHITE_CURRENT": command = Command.WhiteCurrentCheck; break;
                case "TESTER_INIT": command = Command.TESTER_INIT; break;
                case "TSP_Check": command = Command.TSP_Check; break;
            }
       
            return command;
        }

        public static Prefix GetPrefix(string content)
        {
            var prefix = Prefix.None; ;
            switch (content.ToUpper())
            {
                case "DOWN": prefix = Prefix.Down; break;
                case "ACK": prefix = Prefix.Ack; break;
                case "NACK": prefix = Prefix.NAck; break;
            }

            return prefix;
        }

        private static byte[] SendPacketToByte(SendPacket packet)
        {
            try
            {
                int headerSize = Marshal.SizeOf(packet.Header);
                int terminatorSize = Marshal.SizeOf(packet.Terminator);
                int bodySize = packet.Data.Length;
                int totalSize = headerSize + terminatorSize + bodySize;

                var target = new byte[totalSize];

                var headerPointer = Marshal.AllocHGlobal(headerSize + 1);
                var terminatorPointer = Marshal.AllocHGlobal(terminatorSize + 1);

                Marshal.StructureToPtr(packet.Header, headerPointer, false);
                Marshal.StructureToPtr(packet.Terminator, terminatorPointer, false);


                Marshal.Copy(headerPointer, target, 0, headerSize);
                Marshal.Copy(terminatorPointer, target, headerSize + bodySize, terminatorSize);
                Buffer.BlockCopy(packet.Data, 0, target, headerSize, bodySize);

                Marshal.FreeHGlobal(headerPointer);
                Marshal.FreeHGlobal(terminatorPointer);

                return target;
            }
            catch (Exception exception)
            {
                var debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, exception.Message);

                LogTxt.Add(LogTxt.Type.Exception, "T5 SendPacketToByte: " + debug);
            }
            return null;
        }

        private static ReceivePacket ByteToReceivePacket(byte[] data)
        {
            try
            {
                var packet = new ReceivePacket();
                packet.Init();

                var headerSize = Marshal.SizeOf(packet.Header);
                var terminatorSize = Marshal.SizeOf(packet.Terminator);
                var totalSize = data.Length;
                var bodySize = totalSize - headerSize - terminatorSize;

                var headerPointer = Marshal.AllocHGlobal(headerSize+1);
                var terminatorPointer = Marshal.AllocHGlobal(terminatorSize + 1);

                Marshal.Copy(data, 0, headerPointer, headerSize);
                Marshal.Copy(data, headerSize + bodySize, terminatorPointer, terminatorSize);


                packet.Header = (ReceiveHeader)Marshal.PtrToStructure(headerPointer, packet.Header.GetType());
                packet.Terminator = (ReceiveTerminator)Marshal.PtrToStructure(terminatorPointer, packet.Terminator.GetType());

                packet.Data = new byte[bodySize];
                Buffer.BlockCopy(data, headerSize, packet.Data, 0, bodySize);

                Marshal.FreeHGlobal(headerPointer);
                Marshal.FreeHGlobal(terminatorPointer);

                return packet;

            }
            catch (Exception exception)
            {
                var debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, exception.Message);
                LogTxt.Add(LogTxt.Type.Exception, "T5 ByteToReceivePacket: " + debug);
            }
            return null;
        }

        private static SendPacket ByteToSendPacket(byte[] data)
        {
            try
            {
                var packet = new SendPacket();
                packet.Init();

                var headerSize = Marshal.SizeOf(packet.Header);
                var terminatorSize = Marshal.SizeOf(packet.Terminator);
                var totalSize = data.Length;
                var bodySize = totalSize - headerSize - terminatorSize;

                var headerPointer = Marshal.AllocHGlobal(headerSize + 1);
                var terminatorPointer = Marshal.AllocHGlobal(terminatorSize + 1);

                Marshal.Copy(data, 0, headerPointer, headerSize);
                Marshal.Copy(data, headerSize + bodySize, terminatorPointer, terminatorSize);


                packet.Header = (SendHeader)Marshal.PtrToStructure(headerPointer, packet.Header.GetType());
                packet.Terminator = (SendTerminator)Marshal.PtrToStructure(terminatorPointer, packet.Terminator.GetType());

                packet.Data = new byte[bodySize];
                Buffer.BlockCopy(data, headerSize, packet.Data, 0, bodySize);

                Marshal.FreeHGlobal(headerPointer);
                Marshal.FreeHGlobal(terminatorPointer);

                return packet;

            }
            catch (Exception exception)
            {
                var debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, exception.Message);
                LogTxt.Add(LogTxt.Type.Exception, "T5 ByteToSendPacket: " + debug);
            }
            return null;
        }
    }
}
