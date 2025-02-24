using ACO2_App._0.INIT;
using ACO2_App._0.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ACO2.INIT;
using ATCPIP;
using static ACO2_App._0.INIT.T5Helper;

namespace ACO2_App._0
{
    public class Equipment
    {
        #region Field
        #region Classes
        private Controller _controller;
        private EquipmentConfig _eqpConfig;
        private List<Channel> _channels;
        private TcpIpHelper _tcpT5Mess;
        private TCPConnect _cons;

        #endregion
        #region Common
        private int _indexEquip;
        private Thread _update;
        #endregion
        #endregion
        #region Property
        #region Classes
        public EquipmentConfig EqpConfig
        {
            get { return _eqpConfig; }
            set { _eqpConfig = value; }
        }
        public List<Channel> Channels
        {
            get { return _channels; }
            set { _channels = value; }
        }
        public TCPConnect Cons
        {
            get { return _cons; }
            set { _cons = value; }
        }
        public TcpIpHelper TcpT5Mess
        {
            get { return _tcpT5Mess; }
            set { _tcpT5Mess = value; }
        }
        #endregion
        #region Common
        #endregion
        #endregion
        //T:Event
        #region Event
        public delegate void LogT5EventDelegate(string message, string ch);
        public event LogT5EventDelegate LogT5Event;
        public delegate void T5InforEventDelegate(Channel t5Infor);
        public event T5InforEventDelegate T5InforEvent;
        public delegate void PopUpMesEventDelegate(Channel productData, string log);
        public event PopUpMesEventDelegate PopUpMesEvent;
        public delegate void ConnectEventDelegate(bool isConnect);
        public event ConnectEventDelegate ConnectEvent;
       
        #endregion
        #region Constructor
        public Equipment(int eqpindex, EquipmentConfig eqpConfig)
        {
            _eqpConfig = eqpConfig;
            _indexEquip = eqpindex;
            _channels = new List<Channel>();
            _channels = _eqpConfig.Channels;
            _update = new Thread(Update)
            {
                Priority = ThreadPriority.AboveNormal
            };
            _cons = new TCPConnect(_eqpConfig.EQPID);
            
        }
        //T:Start
        public void Start()
        {
            _update.Start();
            _tcpT5Mess = new TcpIpHelper(EqpConfig.PCSignalIPAddress, EqpConfig.PCSignalPort.ToString(), EqpConfig.PCSignalIsActive, "T5_Message");
            _tcpT5Mess.ReciveByteEvent += (data, id, tcpip) =>
            {
                T5MessageSeparate(data, id);
            };
            _tcpT5Mess.Connect.OnConnectEvent -= Connect_OnConnectEvent;
            _tcpT5Mess.Connect.OnConnectEvent += Connect_OnConnectEvent;
            _cons = _tcpT5Mess.Connect;
        }
        //T:Initial
        public async void Initial()
        {
            if (_controller == null)
            {
                _controller = MainWindow.Controller;
            }
            if (_eqpConfig == null)
            {
                _eqpConfig = new EquipmentConfig();

            }

        }
        #endregion
        #region Destructor
        //T:Close
        public void Close()
        {
            _update?.Abort();
            _tcpT5Mess?.Close();

        }
        #endregion
        #region Private Method
        #region PCSignal Mes
        public void T5SendMessage(T5Helper.Command cmd, int ch)
        {
            if (_tcpT5Mess.IsConnected)
            {
                T5Helper.Send2T5(_tcpT5Mess.Connect, T5Helper.MakePacket(cmd, ch - 1), this, ch);
            }
        }
        //T:T5 Message Separate
        private void T5MessageSeparate(byte[] message, string eqpid = "")
        {
            List<int> startIndices = new List<int>();
            for (int i = 0; i < message.Length; i++)
            {
                if (message[i] == 0x02)
                {
                    startIndices.Add(i);
                }
            }
            foreach (int startIndex in startIndices)
            {
                for (int i = startIndex; i < message.Length; i++)
                {
                    if (message[i] == 3)
                    {
                        int endIndex = i;
                        int length = endIndex - startIndex + 1;
                        byte[] segment = new byte[length];
                        Array.Copy(message, startIndex, segment, 0, length);
                        ProcessSignal(segment, eqpid);
                        break;
                    }
                }
            }
        }
        //T:T5 Message Process
        private void ProcessSignal(byte[] message, string eqpid = "")
        {
            var packet = T5Helper.Parse(message);
            var length = Convert.ToInt16(Encoding.ASCII.GetString(packet.Header.Length), 16);
            if (length == packet.Data.Length)
            {
                string str = Encoding.ASCII.GetString(packet.Data);
                if (!_eqpConfig.IsLineCheck && T5Helper.CheckHeartBit(str)) return;

                try
                {
                    var strLog = $"[PCSIGNAL][{_indexEquip}]: {str}";


                    var split = str.Split(',');
                    if (split.Length < 3) { return; }
                    var channel = 0;
                    var command = T5Helper.Command.None;
                    var prefix = T5Helper.Prefix.None;

                    if (split[0] == "Ch")
                    {
                        channel = Convert.ToInt16(split[1]);
                        command = T5Helper.GetCommand(split[2]);
                    }
                    else
                    {
                        prefix = T5Helper.GetPrefix(split[0]);
                        channel = Convert.ToInt16(split[2]);
                        command = T5Helper.GetCommand(split[3]);
                    }

                    LogT5EventHandle(strLog, channel.ToString());
                    Channel channelSignal = new Channel();

                    if (channel < 10)
                    {
                        channelSignal.ChannelNo = $"CH0{channel.ToString()}";
                    }
                    else
                    {
                        channelSignal.ChannelNo = $"CH{channel.ToString()}";
                    }
                    PopUpMesEventHandle(channelSignal, str);
                    switch (command)
                    {
                        case T5Helper.Command.None:
                            break;
                        case T5Helper.Command.Reset:
                            break;
                        case T5Helper.Command.TxReset:
                            break;
                        case T5Helper.Command.ZoneA:
                            break;
                        case T5Helper.Command.ReadyZoneA:
                            break;
                        case T5Helper.Command.ZoneC:
                            break;
                        case T5Helper.Command.ReadyZoneC:
                            break;
                        case T5Helper.Command.MtpWrite:
                            if (prefix == Prefix.Ack)
                            {
                                Channel product = _channels[channel - 1];
                                product.MTPStartTime = DateTime.Now;
                                
                            }
                            if (prefix == Prefix.None)
                            {
                                Channel product = _channels[channel - 1];

                                if (split[3] == "GOOD")
                                {
                                    CheckMessage(command, channel, "GOOD");
                                
                                }
                                else
                                {
                                    if (split.Count() > 4)
                                    {
                                        CheckMessage(command, channel, split[4]);
                                    }
                                }
                            }
                            break;
                        case T5Helper.Command.CellLoading:
                            if (prefix == Prefix.Ack)
                            {
                                Channel product = _channels[channel - 1];
                                product.ContactStartTime = DateTime.Now;
                                product.InsStartTime = DateTime.Now;
                            }
                            if (prefix == Prefix.None)
                            {
                                if (split[3] == "GOOD")
                                {
                                    Channel product = _channels[channel - 1];
                                    UpdateDataContactLog(product, split[3], true);
                                    if (string.IsNullOrEmpty(product.CellID)) return;
                                    Thread.Sleep(600);
                                    Task.Run(() =>
                                    {
                                        Task.Delay(10);
                                        if (product.ContactResult == "GOOD")
                                        {
                                            UpdateDataContactLog(product, split[3], true);
                                        }
                                        else
                                        {
                                            product.ContactResult = "NG";
                                            UpdateDataContactLog(product, split[3], true);
                                        }
                                    });
                                }
                                else
                                {
                                    Channel product = _channels[channel - 1];
                                    if (split.Count() > 4)
                                    {
                                        if (split[4] != "0")
                                        {
                                            UpdateDataContactLog(product, split[4], true);
                                        }
                                    }
                                }
                            }
                           
                            break;
                        case T5Helper.Command.CheckTmdInfo:
                            if (prefix == Prefix.None)
                            {
                                Channel product0 = _channels[channel - 1];
                            }
                            break;
                        case T5Helper.Command.TmdMd5:
                            if (prefix == Prefix.None)
                            {
                                Channel product0 = _channels[channel - 1];
                            }
                            break;
                        case T5Helper.Command.RegPin:
                            break;
                        case T5Helper.Command.Ca310Check:
                            break;
                        case T5Helper.Command.CorCheck:
                            break;
                        case T5Helper.Command.Ca310CalWrite:
                            break;
                        case T5Helper.Command.CheckHostVer:
                            if (prefix == Prefix.None)
                            {
                                Channel product0 = _channels[channel - 1];
                                product0.TxHostVer = split[3];
                                T5InforEventHandle(product0);
                            }
                            break;
                        case T5Helper.Command.LineCheck:
                            break;
                        case T5Helper.Command.ContactCurrent:
                            if (prefix == Prefix.None)
                            {
                                Channel productCtCurr1 = _channels[channel - 1];
                                if (productCtCurr1.MTPWriteResult != "GOOD")
                                {
                                    if (split.Count() > 4 && split[3] != "GOOD")
                                    {
                                        CheckMessage(T5Helper.Command.CellLoading, channel, split[2]);
                                    }
                                }
                            }
                            break;
                        case T5Helper.Command.MtpResultData:
                            break;
                        case T5Helper.Command.SystemError:
                            if (prefix == Prefix.None)
                            {
                                Channel productCtCurr1 = _channels[channel - 1];
                                UpdateDataContactLog(productCtCurr1, split[3], false);
                            }
                            break;
                        case T5Helper.Command.Current:
                            break;
                        case T5Helper.Command.Start:
                            if (split[3] == "NEXT")
                            {
                            }
                            break;
                        case T5Helper.Command.Run:
                            break;
                        case T5Helper.Command.Origin:
                            break;
                        case T5Helper.Command.Next:
                            break;
                        case T5Helper.Command.Back:
                            break;
                        case T5Helper.Command.TurnOn:
                            break;
                        case T5Helper.Command.TurnOff:
                            break;
                        case T5Helper.Command.CylOnMIT:
                            break;
                        case T5Helper.Command.CylOffMIT:
                            break;
                        case T5Helper.Command.MtpWritePrescale:
                            break;
                        case T5Helper.Command.MtpVerify:
                            break;
                        case T5Helper.Command.IdCheck:
                            break;
                        case T5Helper.Command.Compensation:
                            if (prefix == Prefix.None)
                            {
                                if (split.Count() > 4)
                                {
                                    Channel productCtCurr = _channels[channel - 1];
                                    CheckMessage(T5Helper.Command.CellLoading, channel, split[4]);
                                }
                            }
                            break;
                        case T5Helper.Command.BcErrorCheck:
                            if (prefix == Prefix.None)
                            {
                                if (split.Count() > 4)
                                {
                                    CheckMessage(T5Helper.Command.CellLoading, channel, split[4]);
                                }
                            }
                            break;
                        case T5Helper.Command.WhiteCurrentCheck:
                            break;
                        case T5Helper.Command.TESTER_INIT:
                            break;
                        case T5Helper.Command.MCA_OFF:
                            if (!split[3].Contains("GOOD"))
                            {
                                CheckMessage(T5Helper.Command.MtpWrite, channel, "MCA_CHECK_NG");
                            }
                            break;
                        case T5Helper.Command.C_CURRENT:
                            if (prefix == Prefix.None)
                            {
                                if (split.Count() > 4)
                                {
                                    CheckMessage(T5Helper.Command.MtpWrite, channel, split[4]);
                                }
                            }
                            break;
                        case T5Helper.Command.TSP_START:
                            if (prefix == Prefix.None)
                            {
                                if (split.Count() > 4)
                                {
                                    CheckMessage(T5Helper.Command.CellLoading, channel, split[4]);
                                }
                                else
                                {
                                    if (split[3] == "NG")
                                    {
                                        CheckMessage(T5Helper.Command.CellLoading, channel, $"{command}_NG");
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    if (command == T5Helper.Command.None)
                    {
                        Channel product = _channels[channel - 1];
                        if (prefix == Prefix.None)
                        {
                            if (split.Count() > 4)
                            {
                                if (product.ContactResult == "GOOD")
                                {
                                    CheckMessage(T5Helper.Command.MtpWrite, channel, split[4]);
                                }
                                if (string.IsNullOrEmpty(product.ContactResult))
                                {
                                    CheckMessage(T5Helper.Command.CellLoading, channel, split[4]);
                                }
                            }
                            else
                            {
                                if (split[3] == "NG")
                                {

                                    if (product.ContactResult == "GOOD")
                                    {
                                        CheckMessage(T5Helper.Command.MtpWrite, channel, $"{command}_NG");
                                    }
                                    if (string.IsNullOrEmpty(product.ContactResult))
                                    {
                                        CheckMessage(T5Helper.Command.CellLoading, channel, $"{command}_NG");
                                    }
                                }
                            }
                        }
                    }
                    string msg = Encoding.ASCII.GetString(message);

                }
                catch (Exception e)
                {
                    string debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, e.Message);
                    LogTxt.Add(LogTxt.Type.Exception, "Controller ProcessSignal:" + debug);
                    LogTxt.Add(LogTxt.Type.FlowRun, "Controller ProcessSignal:" + debug);
                }
            }
            else
            {

            }
        }
        //T:T5 Message Handler Command
        public void CheckMessage(T5Helper.Command command, int channel, string result)
        {
            if (channel > 0) channel--;
            Channel productData = _channels[channel];

            if (command == T5Helper.Command.CellLoading)
            {
                UpdateDataContactLog(productData, result);
            }
            if (command == T5Helper.Command.MtpWrite)
            {
                UpdateDataInsLog(productData, result);
                productData.MTPEndTime = DateTime.Now;
                productData.MTPTackTime = (productData.MTPEndTime - productData.MTPStartTime).TotalSeconds;
            }
        }
        public string GetDefectCode(string DefectName)
        {
            if (DefectName.ToUpper().Trim() == "GOOD") return "";
            var result = _controller.DefectCodes.Any(x => x.DefectName.ToUpper() == DefectName.ToUpper());
            if (result)
            {
                return _controller.DefectCodes.First(x => x.DefectName.ToUpper() == DefectName.ToUpper()).Msg;
            }
            return "SF67";
        }
        //T:T5 Message Contact Handler
        public void UpdateDataContactLog(Channel productData, string result, bool isCellLoading = false)
        {
           
            if (result == "GOOD" && isCellLoading)
            {
                productData.DefectCode = "";
                productData.ContactResult = result;
                productData.ContactEndTime = DateTime.Now;
                productData.ContactTackTime = (productData.ContactEndTime - productData.ContactStartTime).TotalSeconds;
                productData.ResultInsEventHandle(productData.ChannelNo, productData.ContactResult, "", productData.DefectCode);
                return;
            }
            if (result != "GOOD" && string.IsNullOrEmpty(productData.DefectCode))
            {
                productData.ContactResult = "NG";
                productData.DefectCode = GetDefectCode(result);
                productData.ContactEndTime = DateTime.Now;
                productData.ContactTackTime = (productData.ContactEndTime - productData.ContactStartTime).TotalSeconds;
                productData.ResultInsEventHandle(productData.ChannelNo, productData.ContactResult, "", productData.DefectCode);

            }
            else if (!string.IsNullOrEmpty(productData.DefectCode))
            {
                return;
            }
            productData.ContactResult = result;
        }
        //T:T5 Message MTP Write Handler
        public void UpdateDataInsLog(Channel productData, string result)
        {
            if (result != "GOOD" && string.IsNullOrEmpty(productData.DefectCode))
            {
                productData.DefectCode = GetDefectCode(result);
            }
            productData.MTPWriteResult = result;
            productData.InsEndTime = DateTime.Now;
            productData.InsTackTime = (productData.InsEndTime - productData.InsStartTime).TotalSeconds;
            productData.ResultInsEventHandle(productData.ChannelNo, productData.ContactResult, productData.MTPWriteResult, productData.DefectCode);

        }
        #endregion
        #endregion
        //T:Public Void =======
        #region Public Void
        #endregion
        //T:Event Handle
        #region EventHandle
        private void Connect_OnConnectEvent(bool IsConnected)
        {
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                ConnectEventHandle(IsConnected);
            });
        }
        public void LogT5EventHandle(string message, string ch)
        {
            LogTxt.Add(LogTxt.Type.PCSignalMess, message, _eqpConfig.EqpName, $"CH{ch}");
            Console.WriteLine($"{message} --- CH{ch}");
            var handle = LogT5Event;
            if (handle != null)
            {
                handle(message, ch);
            }
        }
        private void PopUpMesEventHandle(Channel product, string log)
        {
            var handle = PopUpMesEvent;
            if (handle != null)
            {
                handle(product, log);
            }
        }
        private void T5InforEventHandle(Channel t5Infor)
        {
            var handle = T5InforEvent;
            if (handle != null)
            {
                handle(t5Infor);
            }
        }
        private void ConnectEventHandle(bool isConnect)
        {
            string st = isConnect ? "Connected" : "Disconnected";
            LogTxt.Add(LogTxt.Type.Status, $"[PCSIGNAL][{_eqpConfig.EqpName}] {st} with PC!");

            var handle = ConnectEvent;
            if (handle != null)
            {
                handle(isConnect);
            }
        }
        #endregion
        //nam
        //duc
        #region Update
        //T:Update
        public async void Update()
        {
            while (true)
            {

                await Task.Delay(100);
            }
        }
    
        #endregion
    }
}

