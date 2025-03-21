using APlc;
using ACO2_App._0.INIT;
using ACO2_App._0.Model;
using ACO2_App._0.Plc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static APlc.MelsecIF;
using ACO2.Data;
using ACO2.INIT;
using System.Diagnostics;
using OfficeOpenXml.ConditionalFormatting;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Shapes;
using Mitsu3E;
using static ACO2_App._0.INIT.T5Helper;
using System.Windows.Forms;
using CsvHelper;
using System.Globalization;
using Path = System.IO.Path;
using System.Runtime.Remoting.Channels;
using System.Windows.Threading;
using MTP.Model;
using AINI;
using ABOOKFEEDER.INIT;

namespace ACO2_App._0
{
    public class Controller
    {
        #region Enum
        public enum Zone
        {
            Zone1,
            Zone2,
        }
        public enum Tool
        {
            Tool1,
            Tool2,
        }
        #endregion
        #region Field
        #region Classes
        private List<Equipment> _equipment;
        private ControllerConfig _controllerConfig;
        private APlc.PlcComm _plc;
        private PLCHelper _plcH;
        private ListCellDatas _listCellDatas;
        private DataLog _dataLog;
        private MachineStatus _machineStatus;
        private List<CurrentData> _currDatas;
        private List<DefectCode> _defectCodes;
        #endregion
        #region Common
        private bool _isTrigging;
        private Thread _aliveBit;
        private bool _isPlcConnected;
        private Dictionary<string, Func<Task>> _handlers;

        #endregion
        #endregion
        #region Property
        #region Classes
        public List<Equipment> Equipment
        { get { return _equipment; } set { _equipment = value; } }
        public ControllerConfig ControllerConfig
        {
            get { return _controllerConfig; }
            set { _controllerConfig = value; }
        }
        public APlc.PlcComm Plc
        {
            get { return _plc; }
            set { _plc = value; }
        }
        public PLCHelper PlcH
        {
            get { return _plcH; }
            set { _plcH = value; }
        }
        public ListCellDatas ListCellDatas
        {
            get { return _listCellDatas; }
            set { _listCellDatas = value; }
        }
        public MachineStatus MachineStatus
        {
            get { return _machineStatus; }
            set { _machineStatus = value; }
        }
        public List<CurrentData> CurrsDatas { get { return _currDatas; } set { _currDatas = value; } }
        public List<DefectCode> DefectCodes
        {
            get { return _defectCodes; }
            set { _defectCodes = value; }
        }
        #endregion
        #region Common
        public bool IsPlcConnected
        {
            get { return _isPlcConnected; }
        }
        #endregion
        #endregion
        #region Event
        public event Action<CellData> CellLogDataChanged;
        public event Action HeaderStatusChanged;
        public delegate void CurrDataEventDelegate(List<CurrentData> currData);
        public event CurrDataEventDelegate CurrDataEvent;
        public delegate void PlcConnectChangeEventDelegate(bool isConnected);
        public event PlcConnectChangeEventDelegate PlcConnectChangeEvent;
        public delegate Task<bool> MessageDisplayEventDelegate(bool isYesNo, string message);
        public event MessageDisplayEventDelegate MessageDisplayEvent;
        #endregion
        #region Constuctor
        public Controller()
        {
            _controllerConfig = new ControllerConfig();
            _listCellDatas = new ListCellDatas();
            ReadControllerConfig();
            ReadCellDataBackup();
            _equipment = new List<Equipment>();
            foreach (var eqpc in _controllerConfig.EqpConfigs)
            {
                Equipment e = new Equipment(eqpc.EQPIndex, eqpc);
                _equipment.Add(e);
            }
            if (_controllerConfig != null)
            {
                _dataLog = new DataLog(_controllerConfig.EQPID, _controllerConfig.DelLog);
            }
            else _dataLog = new DataLog(_controllerConfig.EQPID);
            _dataLog.Start();
            _machineStatus = new MachineStatus();
            _defectCodes = new List<DefectCode>();
            _defectCodes = ReadDefectConfig();
        }
        public void test()
        {
           _machineStatus.ChannelStatus.FirstOrDefault(x => x.ZoneNo == "1" && x.Channnel == "CH03").Status="SKIP";
            _machineStatus.ChannelStatus.FirstOrDefault(x => x.ZoneNo == "2" && x.Channnel == "CH05").Status = "SKIP";

        }
        public void InitialChannelStatus()
        {
            var zone1 = GetManyWordValueInAreaFromPLC("STATUS_ZONE1");
            foreach (var channel in zone1)
            {
                    _machineStatus.ChannelStatus.Add(new ChannelStatus
                    {
                        ZoneNo = "1",
                        Channnel = channel.Comment.Replace("CHANNEL", ""),
                        Status = ""
                    });
            }
            var zone2 = GetManyWordValueInAreaFromPLC("STATUS_ZONE2");
            foreach (var channel in zone2)
            {
                _machineStatus.ChannelStatus.Add(new ChannelStatus
                {
                    ZoneNo = "2",
                    Channnel = channel.Comment.Replace("CHANNEL", ""),
                    Status = ""
                });
            }
        }
        public async void Initial()
        {
            foreach (var e in _equipment)
            {
                e.Initial();
                Thread.Sleep(1000);
                e.Start();
            }
            InitialPlc();
            InitialChannelStatus();
        }
        public void InitialPlc()
        {
            if (_controllerConfig.PLCConfig != null)
            {

                _plc = new PlcComm();
                _plc.ConfigComm(_controllerConfig.PLCConfig);
                _plc.Start();
                _plcH = new PLCHelper();
                _plcH = _controllerConfig.PLCHelper;
                _plcH.Start(_plc, _controllerConfig.EQPID);
                _aliveBit = new Thread(Alive)
                {
                    IsBackground = true
                };
                _aliveBit.Start();
                InitialHandler();
                _plcH.BitChangedEvent += (name, bit) =>
                {
                    PLCBitChange((BitModel)bit);
                };

            }

        }
        public void InitialGetDataProduct()
        {
            _currDatas = new List<CurrentData>();

            List<CellData> tempData = LoadDataByDateRange(DateTime.Now, DateTime.Now);

            if (tempData == null || tempData.Count == 0)
            {
                LogTxt.Add(LogTxt.Type.Status, "[DATA] CANNOT GET DATA PRODUCT FROM EXCEL.");
                return;
            }

            var groupedData = tempData.GroupBy(c => new { c.ZoneNo, c.Channel.ChannelNo })
                                      .Select(g => new CurrentData
                                      {
                                          Time = DateTime.Now,
                                          Zone = g.Key.ZoneNo,
                                          ChannelName = g.Key.ChannelNo.Replace("CHANNEL",""),
                                          Good = g.Count(c => c.Channel.MTPWriteResult == "GOOD"),
                                          NGContact = g.Count(c => !string.IsNullOrEmpty(c.Channel.ContactResult) && c.Channel.ContactResult != "GOOD"),
                                          NGIns = g.Count(c => !string.IsNullOrEmpty(c.Channel.MTPWriteResult) && c.Channel.MTPWriteResult != "GOOD")
                                      }).ToList();

            _currDatas.AddRange(groupedData);

            foreach (var data in _currDatas)
            {
                LogTxt.Add(LogTxt.Type.Status, $"[ZONE {data.Zone} - CHANNEL {data.ChannelName}] Good: {data.Good}, NGContact: {data.NGContact}, NGIns: {data.NGIns}");
            }
        }

        #endregion
        #region Destructor
        public void Dispose()
        {
            foreach (var eqp in _equipment)
            {
                eqp?.Close();
            }
            _aliveBit?.Abort();
            _plc.Close();
            _plcH?.Close();
            _dataLog?.Stop();
            SaveCellDataBackup();
        }
        #endregion
        #region Config
        //T: Read Controller Config
        private void ReadControllerConfig()
        {
            try
            {
                if (File.Exists(DefaultData.AppPath + @"\Setting\SystemConfig.setting"))
                {
                    string readText = File.ReadAllText(DefaultData.AppPath + @"\Setting\SystemConfig.setting");
                    _controllerConfig = XmlHelper<ControllerConfig>.DeserializeFromString(readText);
                    if (_controllerConfig == null)
                    {
                        _controllerConfig = new ControllerConfig();
                    }
                    DefaultData.LogPath = _controllerConfig.PathLog;
                }
                else
                {
                    _controllerConfig.PathLog = DefaultData.LogPath;
                    _controllerConfig.DelLog = 30;
                }
            }
            catch (Exception ex)
            {
                var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", MethodBase.GetCurrentMethod().DeclaringType.Name.ToString(), MethodBase.GetCurrentMethod().Name, ex.Message);
                LogTxt.Add(LogTxt.Type.Exception, debug);
            }
        }
        //T: Save Controller Config
        public void SaveControllerConfig()
        {
            string str = XmlHelper<ControllerConfig>.SerializeToString(_controllerConfig);
            try
            {
                Task.Run(() =>
                {
                    string path = DefaultData.AppPath + @"\Setting";
                    DefaultData.CheckFolder(path);
                    path += @"\SystemConfig.setting";
                    File.WriteAllText(path, str);
                });
            }
            catch (Exception ex)
            {
                var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.Message);
                LogTxt.Add(LogTxt.Type.Exception, debug);
            }
        }
        //T: Read Cell Data Backup
        private void ReadCellDataBackup()
        {
            try
            {
                if (File.Exists(DefaultData.AppPath + @"\Setting\DataCellBackup.data"))
                {
                    string readText = File.ReadAllText(DefaultData.AppPath + @"\Setting\DataCellBackup.data");
                    ListCellDatas = XmlHelper<ListCellDatas>.DeserializeFromString(readText);
                    if (_listCellDatas == null)
                    {
                        _listCellDatas = new ListCellDatas();
                    }
                }
                else
                {
                    _controllerConfig.PathLog = DefaultData.LogPath;
                    _controllerConfig.DelLog = 30;
                }
            }
            catch (Exception ex)
            {
                var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", MethodBase.GetCurrentMethod().DeclaringType.Name.ToString(), MethodBase.GetCurrentMethod().Name, ex.Message);
                LogTxt.Add(LogTxt.Type.Exception, debug);
            }
        }
        //T: Save Cell Data Backup
        public void SaveCellDataBackup()
        {
            string str = XmlHelper<ListCellDatas>.SerializeToString(_listCellDatas);
            try
            {
                Task.Run(() =>
                {
                    string path = DefaultData.AppPath + @"\Setting";
                    DefaultData.CheckFolder(path);
                    path += @"\DataCellBackup.data";
                    File.WriteAllText(path, str);
                });
            }
            catch (Exception ex)
            {
                var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.Message);
                LogTxt.Add(LogTxt.Type.Exception, debug);
            }
        }
        //T: Read Defect Code Config
        public List<DefectCode> ReadDefectConfig(string path = "")
        {
            List<DefectCode> defectcodes = new List<DefectCode>();
            string pathfile = path == "" ? DefaultData.AppPath + String.Format(@"\Setting\DefectCode.ini") : path;
            if (File.Exists(pathfile))
            {
                int totalcount = int.Parse(IniHelper.READ(pathfile, "Result Error Code", "Total Count"));
                for (int i = 0; i < totalcount; i++)
                {
                    try
                    {
                        string[] value = IniHelper.READ(pathfile, "Result Error Code", i.ToString()).Split(',');
                        if (value.Length > 5)
                        {

                            var info = new DefectCode()
                            {
                                Index = i.ToString(),
                                DefectName = value[0],
                                DefectGroup = value[1],
                                Msg = value[2],
                                PrintCode = value[3],
                                AbRule = value[4],
                                Tray = value[5],
                            };

                            defectcodes.Insert(i, info);


                        }
                    }
                    catch (Exception ex)
                    {
                        var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.Message);
                        LogTxt.Add(LogTxt.Type.Exception, debug);
                    }
                }

            }
            return defectcodes;

        }
        //T: Save DefectCode
        public async Task<bool> SaveDefectConfig(List<DefectCode> defectCodes, string path = "")
        {
            try
            {
                await Task.Run(() =>
                {
                    string pathfile = path == "" ? DefaultData.AppPath + @"\Setting\DefectCode.ini" : path;

                    // Write total count
                    IniHelper.WRITE(pathfile, "Result Error Code", "Total Count", defectCodes.Count.ToString());

                    // Write each defect code
                    for (int i = 0; i < defectCodes.Count; i++)
                    {
                        var defectCode = defectCodes[i];
                        string value = string.Join(",", defectCode.DefectName, defectCode.DefectGroup, defectCode.Msg, defectCode.PrintCode, defectCode.AbRule, defectCode.Tray);
                        IniHelper.WRITE(pathfile, "Result Error Code", i.ToString(), value);
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.Message);
                LogTxt.Add(LogTxt.Type.Exception, debug);
                return false;
            }
        }
        #endregion
        #region Private Method
        private async Task<bool> MessageDisplayEventHandle(bool isYesNo, string message)
        {
            var handle = MessageDisplayEvent;
            if (handle != null)
            {
                var result = await handle(isYesNo, message);
                return result == true;
            }
            // Return a default value if there are no subscribers
            return false;
        }
        #endregion
        #region Public Method
        #region Interface with PLC
        public bool GetSignalBitFromPLC(string Name, string Area = "")
        {
            bool signal = false;
            signal = _plcH.Bits.FirstOrDefault(x => x.Item == $"{Name}").GetPLCValue;
            return signal;
        }
        public bool GetSignalBitFromPC(string Name, string Area = "")
        {
            bool signal = false;
            signal = _plcH.Bits.FirstOrDefault(x => x.Item == $"{Name}").GetPCValue;
            return signal;
        }
        public string GetWordValueFromPLC(string Name, bool isPLCWord, string Area = "")
        {
            string value = "";
            value = _plcH.Words.FirstOrDefault(x => x.Comment == $"{Name}" && x.IsPlc == isPLCWord).GetValue;
            return value;
        }
        public List<WordModel> GetManyWordValueInAreaFromPLC(string area)
        {
            List<WordModel> words = new List<WordModel>();
            words= _plcH.Words.Where(x => x.IsPlc==true && x.Area == area).ToList();
            return words;
        }
        public bool SetSignalBitFromPC(string Name, bool value, string Area = "")
        {
            bool signal = false;
            _plcH.Bits.FirstOrDefault(x => x.Item == $"{Name}").SetPCValue = value;
            signal = GetSignalBitFromPC($"{Name}");
            if (signal == value) return true;
            else return false;
        }
        public bool SetWordValueFromPC(string Name, string value, string Area = "")
        {
            _plcH.Words.FirstOrDefault(x => x.Comment == $"{Name}").SetValue = value;
            if (GetWordValueFromPLC($"{Name}", false) == value) { return true; } else { return false; }
        }

        #endregion
        public async void PLCBitChange(BitModel bit)
        {
            await Task.Run(async () =>
            {
                switch (bit.Type.Trim())
                {
                    case "Command":
                        if (bit.GetPLCValue)
                        {
                            bit.SetPCValue = false;
                        }
                        break;

                    case "Event":
                        if (bit.GetPLCValue)
                        {
                            if (_handlers.TryGetValue(bit.Comment, out Func<Task> handler))
                            {
                                await handler();
                                bit.SetPCValue = true;
                            }
                            bit.SetPCValue = true;
                        }
                        else
                        {
                            bit.SetPCValue = false;
                        }
                        break;
                }
               
            });
        }
        #region Handle Signal PLC
        private void InitialHandler()
        {
            _handlers = new Dictionary<string, Func<Task>>(StringComparer.OrdinalIgnoreCase)
        {
            { Bit.TRACKIN_1, () => Task.Run(() => new TrackInSequenceHandler(Bit.TRACKIN_1)) },
            { Bit.TRACKIN_2, () => Task.Run(() => new TrackInSequenceHandler(Bit.TRACKIN_2)) },

            { Bit.ROBOT1_1_START_PUT, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT1_1_START_PUT)) },
            { Bit.ROBOT1_2_START_PUT, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT1_2_START_PUT)) },
            { Bit.ROBOT1_1_END_PUT, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT1_1_END_PUT)) },
            { Bit.ROBOT1_2_END_PUT, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT1_2_END_PUT)) },
            { Bit.ROBOT1_1_START_GET, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT1_1_START_GET)) },
            { Bit.ROBOT1_2_START_GET, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT1_2_START_GET)) },
            { Bit.ROBOT1_1_END_GET, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT1_1_END_GET)) },
            { Bit.ROBOT1_2_END_GET, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT1_2_END_GET)) },
            { Bit.ROBOT2_1_START_PUT, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT2_1_START_PUT)) },
            { Bit.ROBOT2_2_START_PUT, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT2_2_START_PUT)) },
            { Bit.ROBOT2_1_END_PUT, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT2_1_END_PUT)) },
            { Bit.ROBOT2_2_END_PUT, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT2_2_END_PUT)) },
            { Bit.ROBOT2_1_START_GET, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT2_1_START_GET)) },
            { Bit.ROBOT2_2_START_GET, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT2_2_START_GET)) },
            { Bit.ROBOT2_1_END_GET, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT2_1_END_GET)) },
            { Bit.ROBOT2_2_END_GET, () => Task.Run(() => new RobotSequenceHandler(Bit.ROBOT2_2_END_GET)) },


            { Bit.ROBOT1_1_TRACKOUT, () => Task.Run(() => new TrackOutSequenceHandler(Bit.ROBOT1_1_TRACKOUT)) },
            { Bit.ROBOT1_2_TRACKOUT, () => Task.Run(() => new TrackOutSequenceHandler(Bit.ROBOT1_2_TRACKOUT)) },
            { Bit.ROBOT2_1_TRACKOUT, () => Task.Run(() => new TrackOutSequenceHandler(Bit.ROBOT2_1_TRACKOUT)) },
            { Bit.ROBOT2_2_TRACKOUT, () => Task.Run(() => new TrackOutSequenceHandler(Bit.ROBOT2_2_TRACKOUT)) },

            { Bit.ZONE1_SERVO_1_FW_START, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE1_SERVO_1_FW_START)) },
            { Bit.ZONE1_SERVO_2_FW_START, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE1_SERVO_2_FW_START)) },
            { Bit.ZONE1_SERVO_1_FW_END, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE1_SERVO_1_FW_END)) },
            { Bit.ZONE1_SERVO_2_FW_END, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE1_SERVO_2_FW_END)) },

            { Bit.ZONE1_SERVO_1_BW_START, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE1_SERVO_1_BW_START)) },
            { Bit.ZONE1_SERVO_2_BW_START, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE1_SERVO_2_BW_START)) },
            { Bit.ZONE1_SERVO_1_BW_END, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE1_SERVO_1_BW_END)) },
            { Bit.ZONE1_SERVO_2_BW_END, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE1_SERVO_2_BW_END)) },

            { Bit.ZONE2_SERVO_1_FW_START, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE2_SERVO_1_FW_START)) },
            { Bit.ZONE2_SERVO_2_FW_START, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE2_SERVO_2_FW_START)) },
            { Bit.ZONE2_SERVO_1_FW_END, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE2_SERVO_1_FW_END)) },
            { Bit.ZONE2_SERVO_2_FW_END, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE2_SERVO_2_FW_END)) },

            { Bit.ZONE2_SERVO_1_BW_START, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE2_SERVO_1_BW_START)) },
            { Bit.ZONE2_SERVO_2_BW_START, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE2_SERVO_2_BW_START)) },
            { Bit.ZONE2_SERVO_1_BW_END, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE2_SERVO_1_BW_END)) },
            { Bit.ZONE2_SERVO_2_BW_END, () => Task.Run(() => new ServoSequenceHandler(Bit.ZONE2_SERVO_2_BW_END)) },

                 // ZONE 1
            { Bit.ZONE1_CYL_1_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_1_UP_START)) },
            { Bit.ZONE1_CYL_2_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_2_UP_START)) },
            { Bit.ZONE1_CYL_3_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_3_UP_START)) },
            { Bit.ZONE1_CYL_4_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_4_UP_START)) },
            { Bit.ZONE1_CYL_5_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_5_UP_START)) },
            { Bit.ZONE1_CYL_6_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_6_UP_START)) },
            { Bit.ZONE1_CYL_7_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_7_UP_START)) },
            { Bit.ZONE1_CYL_8_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_8_UP_START)) },
            { Bit.ZONE1_CYL_9_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_9_UP_START)) },
            { Bit.ZONE1_CYL_10_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_10_UP_START)) },
            { Bit.ZONE1_CYL_11_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_11_UP_START)) },
            { Bit.ZONE1_CYL_12_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_12_UP_START)) },

            { Bit.ZONE1_CYL_1_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_1_UP_END)) },
            { Bit.ZONE1_CYL_2_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_2_UP_END)) },
            { Bit.ZONE1_CYL_3_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_3_UP_END)) },
            { Bit.ZONE1_CYL_4_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_4_UP_END)) },
            { Bit.ZONE1_CYL_5_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_5_UP_END)) },
            { Bit.ZONE1_CYL_6_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_6_UP_END)) },
            { Bit.ZONE1_CYL_7_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_7_UP_END)) },
            { Bit.ZONE1_CYL_8_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_8_UP_END)) },
            { Bit.ZONE1_CYL_9_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_9_UP_END)) },
            { Bit.ZONE1_CYL_10_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_10_UP_END)) },
            { Bit.ZONE1_CYL_11_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_11_UP_END)) },
            { Bit.ZONE1_CYL_12_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_12_UP_END)) },

            { Bit.ZONE1_CYL_1_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_1_DOWN_START)) },
            { Bit.ZONE1_CYL_2_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_2_DOWN_START)) },
            { Bit.ZONE1_CYL_3_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_3_DOWN_START)) },
            { Bit.ZONE1_CYL_4_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_4_DOWN_START)) },
            { Bit.ZONE1_CYL_5_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_5_DOWN_START)) },
            { Bit.ZONE1_CYL_6_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_6_DOWN_START)) },
            { Bit.ZONE1_CYL_7_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_7_DOWN_START)) },
            { Bit.ZONE1_CYL_8_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_8_DOWN_START)) },
            { Bit.ZONE1_CYL_9_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_9_DOWN_START)) },
            { Bit.ZONE1_CYL_10_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_10_DOWN_START)) },
            { Bit.ZONE1_CYL_11_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_11_DOWN_START)) },
            { Bit.ZONE1_CYL_12_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_12_DOWN_START)) },

            { Bit.ZONE1_CYL_1_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_1_DOWN_END)) },
            { Bit.ZONE1_CYL_2_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_2_DOWN_END)) },
            { Bit.ZONE1_CYL_3_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_3_DOWN_END)) },
            { Bit.ZONE1_CYL_4_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_4_DOWN_END)) },
            { Bit.ZONE1_CYL_5_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_5_DOWN_END)) },
            { Bit.ZONE1_CYL_6_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_6_DOWN_END)) },
            { Bit.ZONE1_CYL_7_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_7_DOWN_END)) },
            { Bit.ZONE1_CYL_8_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_8_DOWN_END)) },
            { Bit.ZONE1_CYL_9_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_9_DOWN_END)) },
            { Bit.ZONE1_CYL_10_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_10_DOWN_END)) },
            { Bit.ZONE1_CYL_11_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_11_DOWN_END)) },
            { Bit.ZONE1_CYL_12_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE1_CYL_12_DOWN_END)) },

            // ZONE 2
            { Bit.ZONE2_CYL_1_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_1_UP_START)) },
            { Bit.ZONE2_CYL_2_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_2_UP_START)) },
            { Bit.ZONE2_CYL_3_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_3_UP_START)) },
            { Bit.ZONE2_CYL_4_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_4_UP_START)) },
            { Bit.ZONE2_CYL_5_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_5_UP_START)) },
            { Bit.ZONE2_CYL_6_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_6_UP_START)) },
            { Bit.ZONE2_CYL_7_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_7_UP_START)) },
            { Bit.ZONE2_CYL_8_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_8_UP_START)) },
            { Bit.ZONE2_CYL_9_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_9_UP_START)) },
            { Bit.ZONE2_CYL_10_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_10_UP_START)) },
            { Bit.ZONE2_CYL_11_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_11_UP_START)) },
            { Bit.ZONE2_CYL_12_UP_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_12_UP_START)) },

            { Bit.ZONE2_CYL_1_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_1_UP_END)) },
            { Bit.ZONE2_CYL_2_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_2_UP_END)) },
            { Bit.ZONE2_CYL_3_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_3_UP_END)) },
            { Bit.ZONE2_CYL_4_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_4_UP_END)) },
            { Bit.ZONE2_CYL_5_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_5_UP_END)) },
            { Bit.ZONE2_CYL_6_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_6_UP_END)) },
            { Bit.ZONE2_CYL_7_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_7_UP_END)) },
            { Bit.ZONE2_CYL_8_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_8_UP_END)) },
            { Bit.ZONE2_CYL_9_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_9_UP_END)) },
            { Bit.ZONE2_CYL_10_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_10_UP_END)) },
            { Bit.ZONE2_CYL_11_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_11_UP_END)) },
            { Bit.ZONE2_CYL_12_UP_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_12_UP_END)) },

            { Bit.ZONE2_CYL_1_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_1_DOWN_START)) },
            { Bit.ZONE2_CYL_2_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_2_DOWN_START)) },
            { Bit.ZONE2_CYL_3_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_3_DOWN_START)) },
            { Bit.ZONE2_CYL_4_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_4_DOWN_START)) },
            { Bit.ZONE2_CYL_5_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_5_DOWN_START)) },
            { Bit.ZONE2_CYL_6_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_6_DOWN_START)) },
            { Bit.ZONE2_CYL_7_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_7_DOWN_START)) },
            { Bit.ZONE2_CYL_8_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_8_DOWN_START)) },
            { Bit.ZONE2_CYL_9_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_9_DOWN_START)) },
            { Bit.ZONE2_CYL_10_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_10_DOWN_START)) },
            { Bit.ZONE2_CYL_11_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_11_DOWN_START)) },
            { Bit.ZONE2_CYL_12_DOWN_START, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_12_DOWN_START)) },

            { Bit.ZONE2_CYL_1_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_1_DOWN_END)) },
            { Bit.ZONE2_CYL_2_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_2_DOWN_END)) },
            { Bit.ZONE2_CYL_3_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_3_DOWN_END)) },
            { Bit.ZONE2_CYL_4_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_4_DOWN_END)) },
            { Bit.ZONE2_CYL_5_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_5_DOWN_END)) },
            { Bit.ZONE2_CYL_6_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_6_DOWN_END)) },
            { Bit.ZONE2_CYL_7_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_7_DOWN_END)) },
            { Bit.ZONE2_CYL_8_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_8_DOWN_END)) },
            { Bit.ZONE2_CYL_9_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_9_DOWN_END)) },
            { Bit.ZONE2_CYL_10_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_10_DOWN_END)) },
            { Bit.ZONE2_CYL_11_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_11_DOWN_END)) },
            { Bit.ZONE2_CYL_12_DOWN_END, () => Task.Run(() => new CylinderSequenceHandler(Bit.ZONE2_CYL_12_DOWN_END)) }

        };

        }

        #endregion
        #region Method Excute Data
        
        public CellData FindCellInListTemp(string cellid, bool isStepStartIns = false, bool isStepInsDone = false, string channel = "", bool isStepStartTrackOut = false)
        {
            if (isStepStartIns)
            {
                return _listCellDatas.CellDatas.FirstOrDefault(cell => cell.CellID == cellid && cell.TrackIn == "OK");
            }
            else if (isStepInsDone && !isStepStartIns)
            {
                return _listCellDatas.CellDatas.FirstOrDefault(cell => cell.CellID == cellid && cell.TrackIn == "OK" );
            }
            else if (isStepStartTrackOut)
            {
                return _listCellDatas.CellDatas.FirstOrDefault(cell => cell.CellID == cellid);
            }
            return null;
        }
        public async Task<(string cellId, string channel,bool isTimeOut)> WaitForPlcData(string cellIdKey, string data2key)
        {
            int maxTimeoutMs = 10000;
            StopWatch stopwatch = new StopWatch();
            stopwatch.Start();

            while (string.IsNullOrEmpty(GetWordValueFromPLC(cellIdKey, true)) &&
                   string.IsNullOrEmpty(GetWordValueFromPLC(data2key, true)))
            {
                if (stopwatch.CheckElapsedTime(maxTimeoutMs))
                {
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[PLC TIMEOUT] Timeout while waiting for {cellIdKey} and {data2key}.");
                    return (null, null, true); 
                }
                Thread.Sleep(5);
            }
            stopwatch.Stop();

            string cellId = GetWordValueFromPLC(cellIdKey, true);
            string data2 = GetWordValueFromPLC(data2key, true);

            return (cellId, data2,false);
        }
        private void GetStatusDataFromPLC()
        {
            switch(GetWordValueFromPLC("STATUS", true))
            {
                case "0": _machineStatus.Status = "AUTO"; break; 
                case "1": _machineStatus.Status = "MANUAL"; break;
                case "2": _machineStatus.Status = "ERROR"; break;
                default : _machineStatus.Status = "DEFAULT";  break;
            }
            switch (GetWordValueFromPLC("AVAILABILITYSTATE", true))
            {
                case "0": _machineStatus.Status = "UP"; break;
                case "1": _machineStatus.Status = "DOWN"; break;
                default: _machineStatus.Status = "DEFAULT"; break;
            }
            switch (GetWordValueFromPLC("INTERLOCKSTATE", true))
            {
                case "0": _machineStatus.Status = "ON"; break;
                case "1": _machineStatus.Status = "OFF"; break;
                default: _machineStatus.Status = "DEFAULT"; break;
            }
            switch (GetWordValueFromPLC("MOVESTATE", true))
            {
                case "0": _machineStatus.Status = "RUNNING"; break;
                case "1": _machineStatus.Status = "PAUSE"; break;
                default: _machineStatus.Status = "DEFAULT"; break;
            }
            switch (GetWordValueFromPLC("RUNSTATE", true))
            {
                case "0": _machineStatus.Status = "RUN"; break;
                case "1": _machineStatus.Status = "IDLE"; break;
                default: _machineStatus.Status = "DEFAULT"; break;
            }
            _machineStatus.ChannelStatus.Clear();
            
            var zone1 = GetManyWordValueInAreaFromPLC("STATUS_ZONE1");
            foreach(var channel in zone1)
            {
                string status = "";
                switch (channel.GetValue)
                {
                    case "0":status = "AUTO";break;
                    case "1":status = "MANUAL";break;
                    case "2":status = "SKIP";break;
                }
               var channelstatus = _machineStatus.ChannelStatus.FirstOrDefault(x=>x.ZoneNo=="1"&&x.Channnel== channel.Comment.Replace("CHANNEL", "CH"));
                if(channelstatus != null)
                {
                    channelstatus.Status = status;
                }
                else
                {
                    _machineStatus.ChannelStatus.Add(new ChannelStatus
                    {
                        ZoneNo = "1",
                        Channnel = channel.Comment.Replace("CHANNEL", "CH"),
                        Status = status
                    });
                }
            }
            var zone2 = GetManyWordValueInAreaFromPLC("STATUS_ZONE2");
            foreach (var channel in zone2)
            {
                string status = "";
                switch (channel.GetValue)
                {
                    case "0": status = "AUTO"; break;
                    case "1": status = "MANUAL"; break;
                    case "2": status = "SKIP"; break;
                }
                var channelstatus = _machineStatus.ChannelStatus.FirstOrDefault(x => x.ZoneNo == "2" && x.Channnel == channel.Comment.Replace("CHANNEL", "CH"));
                if (channelstatus != null)
                {
                    channelstatus.Status = status;
                }
                else
                {
                    _machineStatus.ChannelStatus.Add(new ChannelStatus
                    {
                        ZoneNo = "2",
                        Channnel = channel.Comment.Replace("CHANNEL", "CH"),
                        Status = status
                    });
                }
            }
        }
        #endregion
        #region Handler LOG
        public async Task SaveDataLog(CellData productData)
        {
            if (productData == null) return;

            #region Make header
            var header = new StringBuilder();

            // Start with general fields
            header.Append("TIME,");
            header.Append("CELLID,");
            header.Append("EQUIP,");
            header.Append("Zone_No,");
            header.Append("UNIT,");
            header.Append("STAGE,");
            header.Append("CHANNEL,");
            header.Append("Trace In,");
            header.Append("Trace Out,");
            header.Append("INS_ROBOT 1_TOOL,");
            header.Append("INS_ROBOT 2_TOOL,");

            // Contact and result fields
            header.Append("CONTACT_RESULT,");
            header.Append("LAST RESULT,");
            header.Append("DEFECTCODE,");
            header.Append("MTP_WRITE_RESULT,");
            header.Append("AB Rule,");
            header.Append("RETRY,");
            header.Append("RECHECKED,");

            // Temperature and device information
            header.Append("TEMPERATURE,");
            header.Append("MC_Ver,");
            header.Append("TxHost Ver,");
            header.Append("TMD FILE,");
            header.Append("PG_UI,");
            header.Append("T5 MAC CHANNEL,");

            // Measurement and control fields
            header.Append("X600,");
            header.Append("IBat,");
            header.Append("IVss,");
            header.Append("IDd,");
            header.Append("ICi,");
            header.Append("IBat2,");
            header.Append("IDd2,");

            // Time and Tact fields
            header.Append("MC_START_TIME,");
            header.Append("MC_END_TIME,");
            header.Append("MC_TACT TIME,");
            header.Append("UNIT TOTAL TACT START TIME,");
            header.Append("UNIT TOTAL TACT END TIME,");
            header.Append("UNIT TOTAL TACT,");
            header.Append("INS_START_TIME,");
            header.Append("INS_END_TIME,");
            header.Append("INS_TACT TIME,");

            // Robot and process time fields
            header.Append("ROBOT PUT DOWN TACT START TIME,");
            header.Append("ROBOT PUT DOWN TACT END TIME,");
            header.Append("ROBOT PUT DOWN TACT,");
            header.Append("CHECK CONTACT TACT START TIME,");
            header.Append("CHECK CONTACT TACT END TIME,");
            header.Append("CHECK CONTACT TACT,");
            header.Append("SPARE TACT TIME,");
            header.Append("SPARE TACT END TIME,");
            header.Append("SPARE TACT,");

            // Servo and Cylinder time fields
            header.Append("SERVO FW TACT START TIME,");
            header.Append("SERVO FW TACT END TIME,");
            header.Append("SERVO FW TACT,");
            header.Append("CYL DW TACT START TIME,");
            header.Append("CYL DW TACT END TIME,");
            header.Append("CYL DW TACT,");

            // MTP Write and Cylinder UP2
            header.Append("CHECK MTP_WRITE START TIME,");
            header.Append("CHECK MTP_WRITE TACT END TIME,");
            header.Append("CHECK MTP_WRITE TACT,");
            header.Append("CYL UP2 TACT START TIME,");
            header.Append("CYL UP2 TACT END TIME,");
            header.Append("CYL UP2 TACT,");

            // Servo BW and Robot pick up time
            header.Append("SERVO BW TACT START TIME,");
            header.Append("SERVO BW TACT END TIME,");
            header.Append("SERVO BW TACT,");
            header.Append("ROBOT PICK UP TACT START TIME,");
            header.Append("ROBOT PICK UP TACT END TIME,");
            header.Append("ROBOT PICK UP TACT,");

            #endregion
            #region Make content
            int indexeqp = int.Parse(productData.ZoneNo)-1;
            var content = new StringBuilder();
            productData.Time = DateTime.Now;
            productData.MachineName = _controllerConfig.EQPID;
            if(productData.Channel.ContactResult == "GOOD"&& productData.Channel.MTPWriteResult == "GOOD"&&string.IsNullOrEmpty(productData.Channel.DefectCode))
            {
                productData.Channel.LastResult = "GOOD";
            }
            else
            {
                productData.Channel.LastResult = productData.Channel.DefectCode;

            }
            productData.Temperater = GetWordValueFromPLC("TEMPERATER", true);
            //  productData.EQPID = _controllerConfig.EqpConfigs[indexeqp].EQPID;
            productData.CylDWEndTime = productData.Channel.MTPStartTime;
            productData.CylDWTaktTime = (productData.CylDWEndTime - productData.CylDWStartTime).TotalMilliseconds;
            productData.CylUpStartTime = productData.Channel.MTPEndTime;
            productData.CylUpTaktTime = (productData.CylUpEndTime - productData.CylUpStartTime).TotalMilliseconds;

            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.Time)); // TIME
            content.Append(string.Format("{0},", productData.CellID)); //CELLID
            content.Append(string.Format("{0},", productData.MachineName)); //MACHINE
            content.Append(string.Format("{0},", productData.ZoneNo)); //ZONE
            content.Append(string.Format("UNIT{0},", productData.Unit)); //UNIT
            content.Append(string.Format("STAGE{0},", productData.Stage)); //STAGE
            content.Append(string.Format("CHANNEL{0},", productData.Channel.ChannelNo)); //CHANNEL
            content.Append(string.Format("{0},", productData.TrackIn)); //TRACEIN
            content.Append(string.Format("{0},", productData.TrackOut)); //TRACEOUT

            content.Append(string.Format("{0},", productData.InsRobot1ToolNo)); //INS_ROBOT1_TOOL
            content.Append(string.Format("{0},", productData.InsRobot2ToolNo)); //INS_ROBOT2_TOOL

            content.Append(string.Format("{0},", productData.Channel.ContactResult)); //CONTACT_RESULT
            content.Append(string.Format("{0},", productData.Channel.LastResult)); //LAST_RESULT
            content.Append(string.Format("{0},", productData.Channel.DefectCode)); //DEFECTCODE
            content.Append(string.Format("{0},", productData.Channel.MTPWriteResult)); //MTP_WRITE_RESULT

            content.Append(string.Format("{0},", productData.ABRule)); //AB rule
            content.Append(string.Format("{0},", productData.Retry)); //RETRY
            content.Append(string.Format("{0},", productData.Rechecked)); //RECHECKED

            content.Append(string.Format("{0},", productData.Temperater)); //TEMPERATER

            content.Append(string.Format("{0},", productData.Channel.T5MACVer)); //MC_Ver
            content.Append(string.Format("{0},", productData.Channel.TxHostVer)); //TX_HOST_VER
            content.Append(string.Format("{0},", productData.Channel.TMDFile)); //TMD FILE
            content.Append(string.Format("{0},", productData.Channel.PGUi)); //PG_UI
            content.Append(string.Format("{0},", productData.Channel.T5MacChannel)); //PG_UI
            content.Append(string.Format("{0},", productData.Channel.X600)); //X600

            content.Append(string.Format("{0},", productData.Channel.Ibat)); //Ibat
            content.Append(string.Format("{0},", productData.Channel.IVss)); //IVss
            content.Append(string.Format("{0},", productData.Channel.IDd)); //IDd
            content.Append(string.Format("{0},", productData.Channel.ICi)); //ICi
            content.Append(string.Format("{0},", productData.Channel.IBat2)); //IBat2
            content.Append(string.Format("{0},", productData.Channel.IDd2)); //IDd2

            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.MCStartTime)); // MC_START_TIME
            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.MCEndTime)); // MC_END_TIME
            content.Append(string.Format("{0},", productData.MCTackTime)); //MC_TAKTTIME

            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.UnitStartTime)); // UNIT_START_TIME
            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.UnitEndTime)); // UNIT_END_TIME
            content.Append(string.Format("{0},", productData.UnitTackTime)); //UNIT_TAKTTIME

            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.Channel.InsStartTime)); // INS_START_TIME
            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.Channel.InsEndTime)); // INS_END_TIME
            content.Append(string.Format("{0},", productData.Channel.InsTackTime)); //INS_TAKTTIME

            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.RBDropStartTime)); // ROBOT PUT DOWN TACT START TIME
            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.RBDropEndTime)); // ROBOT PUT DOWN TACT END TIME
            content.Append(string.Format("{0},", productData.RBDropTackTime)); //ROBOT PUT DOWN TACT

            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.Channel.ContactStartTime)); // CONTACT_START_TIME
            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.Channel.ContactEndTime)); // CONTACT_END_TIME
            content.Append(string.Format("{0},", productData.Channel.ContactTackTime)); //CONTACT_TAKTTIME

            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.SpareStartTime)); // SPARE_START_TIME
            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.SpareEndTime)); // SPARE_END_TIME
            content.Append(string.Format("{0},", productData.SpareTackTime)); //SPARE_TAKTTIME

            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.ServoFWStartTime)); // SERVO_FW_START_TIME
            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.ServoFWEndTime)); // SERVO_FW_END_TIME
            content.Append(string.Format("{0},", productData.ServoFWTaktTime)); //SERVO_FW_TAKTTIME

            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.CylDWStartTime)); // CYL_DW_START_TIME
            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.CylDWEndTime)); // CYL_DW_END_TIME
            content.Append(string.Format("{0},", productData.CylDWTaktTime)); //CYL_DW_TAKTTIME

            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.Channel.MTPStartTime)); // MTPWRITE_START_TIME
            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.Channel.MTPEndTime)); // MTPWRITE_END_TIME
            content.Append(string.Format("{0},", productData.Channel.MTPTackTime)); //MTPWRITE_TAKTTIME

            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.CylUpStartTime)); // CYL_UP_START_TIME
            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.CylUpEndTime)); // CYL_UP_END_TIME
            content.Append(string.Format("{0},", productData.CylUpTaktTime)); //CYL_UP_TAKTTIME

            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.ServoBWStartTime)); // SERVO_BW_START_TIME
            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.ServoBWEndTime)); // SERVO_BW_END_TIME
            content.Append(string.Format("{0},", productData.ServoBWTaktTime)); //SERVO_BW_TAKTTIME

            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.RBPickStartTime)); // ROBOT PICK UP TACT START TIME
            content.Append(string.Format("{0:yyyy-MM-dd HH:mm:ss},", productData.RBPickEndTime)); //  ROBOT PICK UP TACT END TIME
            content.Append(string.Format("{0},", productData.RBPickTackTime)); //ROBOT PICK UP TACT
            #endregion

            var result = _dataLog.Add(header.ToString(), content.ToString());
            Thread.Sleep(1000);
            Task.WaitAll(result);
            UpdateCurrData(productData);
        }
        private void UpdateCurrData(CellData productData)
        {
            if (productData == null) return;

                string zone = productData.ZoneNo;
                string channel = productData.Channel.ChannelNo;

                // Tìm dữ liệu trong _currDatas theo Zone và Channel
                var existingData = _currDatas.FirstOrDefault(c => c.Zone == zone && c.ChannelName == channel);

                if (existingData != null)
                {
                    // Kiểm tra và cập nhật số liệu dựa trên productData
                    if (productData.Channel.MTPWriteResult == "GOOD")
                    {
                        existingData.Good++;
                    CurrDataEventHandle(_currDatas);
                }
                    else if (!string.IsNullOrEmpty(productData.Channel.MTPWriteResult) && productData.Channel.MTPWriteResult != "GOOD")
                    {
                        existingData.NGIns++;
                    CurrDataEventHandle(_currDatas);
                }

                    if (!string.IsNullOrEmpty(productData.Channel.ContactResult) && productData.Channel.ContactResult != "GOOD")
                    {
                        existingData.NGContact++;
                    CurrDataEventHandle(_currDatas);
                }
                }
                else
                {
                    // Nếu chưa có dữ liệu, thêm mới vào danh sách
                    if (!string.IsNullOrEmpty(productData.Channel.MTPWriteResult) || !string.IsNullOrEmpty(productData.Channel.ContactResult))
                    {
                        var newData = new CurrentData
                        {
                            Time = DateTime.Now,
                            Zone = zone,
                            ChannelName = channel,
                            Good = productData.Channel.MTPWriteResult == "GOOD" ? 1 : 0,
                            NGIns = (!string.IsNullOrEmpty(productData.Channel.MTPWriteResult) && productData.Channel.MTPWriteResult != "GOOD") ? 1 : 0,
                            NGContact = (!string.IsNullOrEmpty(productData.Channel.ContactResult) && productData.Channel.ContactResult != "GOOD") ? 1 : 0
                        };
                       
                        _currDatas.Add(newData);
                    CurrDataEventHandle(_currDatas);
                }
                }
        }

        public string CreateLogFollowCellData(CellData cellData)
        {
          return  cellData == null
       ? "[ERROR] cellData is null."
       : string.Join(" ; ", cellData.GetType().GetProperties()
             .Select(prop => $"{prop.Name}: {prop.GetValue(cellData) ?? "NULL"}"));
        }
        //T: LoadDataByDateRange
        public List<CellData> LoadDataByDateRange(DateTime startDate, DateTime endDate)
        {
            List<CellData> productDatas = new List<CellData>();
            string folderPath = "";
            try
            {
                // Set the folder containing year directories
                folderPath = $"{DefaultData.LogPath}\\Data"; 
                // Get the list of dates within the selected date range
                List<DateTime> dateRange = GetDateRange(startDate, endDate);

                foreach (DateTime date in dateRange)
                {
                    // Construct paths for year and month directories
                    string yearDirectory = Path.Combine(folderPath, date.Year.ToString());
                    string monthDirectory = Path.Combine(yearDirectory, date.ToString("MM"));
                    string filePath = "";
                     filePath = Path.Combine(monthDirectory, $"{_controllerConfig.EQPID}_{date:dd}.csv"); 


                    // Check and process the CSV file
                    if (File.Exists(filePath))
                    {
                        using (var reader = new StreamReader(filePath))
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            // Read the header
                            csv.Read();
                            csv.ReadHeader();
                            var headers = csv.HeaderRecord;

                            // Read records dynamically
                            while (csv.Read())
                            {
                                var productData = new CellData
                                {
                                    Time = DateTime.Parse(csv.GetField("TIME")),  // Time
                                    CellID = csv.GetField("CELLID"),  // CELLID
                                    MachineName = csv.GetField("EQUIP"),  // MACHINE
                                    ZoneNo = csv.GetField("Zone_No"),  // Zone_No
                                    Unit = csv.GetField("UNIT"),  // UNIT
                                    Stage = csv.GetField("STAGE"),  // STAGE
                                    TrackIn = csv.GetField("Trace In"),  // Trace In
                                    TrackOut = csv.GetField("Trace Out"),  // Trace Out

                                    InsRobot1ToolNo = csv.GetField("INS_ROBOT 1_TOOL"),  // INS_ROBOT 1_TOOL
                                    InsRobot2ToolNo = csv.GetField("INS_ROBOT 2_TOOL"),  // INS_ROBOT 2_TOOL

                                    Channel = new Channel
                                    {
                                        ChannelNo = csv.GetField("CHANNEL"),  // CHANNEL
                                        ContactResult = csv.GetField("CONTACT_RESULT"),  // CONTACT_RESULT
                                        LastResult = csv.GetField("LAST RESULT"),  // LAST RESULT
                                        DefectCode = csv.GetField("DEFECTCODE"),  // DEFECTCODE
                                        MTPWriteResult = csv.GetField("MTP_WRITE_RESULT"),  // MTP_WRITE_RESULT
                                        InsStartTime = DateTime.Parse(csv.GetField("INS_START_TIME")),  // INS_START_TIME
                                        InsEndTime = DateTime.Parse(csv.GetField("INS_END_TIME")),  // INS_END_TIME
                                        InsTackTime = double.Parse(csv.GetField("INS_TACT TIME")),  // INS_TACT TIME
                                        ContactStartTime = DateTime.Parse(csv.GetField("CHECK CONTACT TACT START TIME")),  // CHECK CONTACT TACT START TIME
                                        ContactEndTime = DateTime.Parse(csv.GetField("CHECK CONTACT TACT END TIME")),  // CHECK CONTACT TACT END TIME
                                        ContactTackTime = double.Parse(csv.GetField("CHECK CONTACT TACT")),  // CONTACT_TACT TIME
                                        MTPStartTime = DateTime.Parse(csv.GetField("CHECK MTP_WRITE START TIME")),  // MTPWRITE_START_TIME
                                        MTPEndTime = DateTime.Parse(csv.GetField("CHECK MTP_WRITE TACT END TIME")),  // MTPWRITE_END_TIME
                                        MTPTackTime = double.Parse(csv.GetField("CHECK MTP_WRITE TACT")) , // MTPWRITE_TAKTTIME

                                        T5MACVer = csv.GetField("MC_Ver"),  // MC_Ver
                                        TxHostVer = csv.GetField("TxHost Ver"),  // TxHost Ver
                                        TMDFile = csv.GetField("TMD FILE"),  // TMD FILE
                                        PGUi = csv.GetField("PG_UI"),  // PG_UI
                                        T5MacChannel = csv.GetField("T5 MAC CHANNEL"),  // T5 MAC CHANNEL

                                        X600 = csv.GetField("X600"),  // X600
                                        Ibat = csv.GetField("IBat"),  // IBat
                                        IVss = csv.GetField("IVss"),  // IVss
                                        IDd = csv.GetField("IDd"),  // IDd
                                        ICi = csv.GetField("ICi"),  // ICi
                                        IBat2 = csv.GetField("IBat2"),  // IBat2
                                        IDd2 = csv.GetField("IDd2")  // IDd2
                                    },

                                    ABRule = csv.GetField("AB Rule"),  // AB Rule
                                    Retry = csv.GetField("RETRY"),  // RETRY
                                    Rechecked = csv.GetField("RECHECKED"),  // RECHECKED
                                    Temperater = csv.GetField("TEMPERATURE"),  // TEMPERATER

                                    MCStartTime = DateTime.Parse(csv.GetField("MC_START_TIME")),  // MC_START_TIME
                                    MCEndTime = DateTime.Parse(csv.GetField("MC_END_TIME")),  // MC_END_TIME
                                    MCTackTime = double.Parse(csv.GetField("MC_TACT TIME")),  // MC_TACT TIME

                                    UnitStartTime = DateTime.Parse(csv.GetField("UNIT TOTAL TACT START TIME")),  // UNIT TOTAL TACT START TIME
                                    UnitEndTime = DateTime.Parse(csv.GetField("UNIT TOTAL TACT END TIME")),  // UNIT TOTAL TACT END TIME
                                    UnitTackTime = double.Parse(csv.GetField("UNIT TOTAL TACT")),  // UNIT TOTAL TACT

                                    RBDropStartTime = DateTime.Parse(csv.GetField("ROBOT PUT DOWN TACT START TIME")),  // ROBOT PUT DOWN TACT START TIME
                                    RBDropEndTime = DateTime.Parse(csv.GetField("ROBOT PUT DOWN TACT END TIME")),  // ROBOT PUT DOWN TACT END TIME
                                    RBDropTackTime = double.Parse(csv.GetField("ROBOT PUT DOWN TACT")),  // ROBOT PUT DOWN TACT

                                    SpareStartTime = DateTime.Parse(csv.GetField("SPARE TACT TIME")),  // SPARE TACT START TIME
                                    SpareEndTime = DateTime.Parse(csv.GetField("SPARE TACT END TIME")),  // SPARE TACT END TIME
                                    SpareTackTime = double.Parse(csv.GetField("SPARE TACT")),  // SPARE TACT

                                    ServoFWStartTime = DateTime.Parse(csv.GetField("SERVO FW TACT START TIME")),  // SERVO FW TACT START TIME
                                    ServoFWEndTime = DateTime.Parse(csv.GetField("SERVO FW TACT END TIME")),  // SERVO FW TACT END TIME
                                    ServoFWTaktTime = double.Parse(csv.GetField("SERVO FW TACT")),  // SERVO FW TACT

                                    CylDWStartTime = DateTime.Parse(csv.GetField("CYL DW TACT START TIME")),  // CYL DW TACT START TIME
                                    CylDWEndTime = DateTime.Parse(csv.GetField("CYL DW TACT END TIME")),  // CYL DW TACT END TIME
                                    CylDWTaktTime = double.Parse(csv.GetField("CYL DW TACT")),  // CYL DW TACT

                                    CylUpStartTime = DateTime.Parse(csv.GetField("CYL UP2 TACT START TIME")),  // CYL UP2 TACT START TIME
                                    CylUpEndTime = DateTime.Parse(csv.GetField("CYL UP2 TACT END TIME")),  // CYL UP2 TACT END TIME
                                    CylUpTaktTime = double.Parse(csv.GetField("CYL UP2 TACT")),  // CYL UP2 TACT

                                    ServoBWStartTime = DateTime.Parse(csv.GetField("SERVO BW TACT START TIME")),  // SERVO BW TACT START TIME
                                    ServoBWEndTime = DateTime.Parse(csv.GetField("SERVO BW TACT END TIME")),  // SERVO BW TACT END TIME
                                    ServoBWTaktTime = double.Parse(csv.GetField("SERVO BW TACT")),  // SERVO BW TACT

                                    RBPickStartTime = DateTime.Parse(csv.GetField("ROBOT PICK UP TACT START TIME")),  // ROBOT PICK UP TACT START TIME
                                    RBPickEndTime = DateTime.Parse(csv.GetField("ROBOT PICK UP TACT END TIME")),  // ROBOT PICK UP TACT END TIME
                                    RBPickTackTime = double.Parse(csv.GetField("ROBOT PICK UP TACT"))  // ROBOT PICK UP TACT

                           
                                };


                                productDatas.Add(productData);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.",
                                           this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.Message);
                LogTxt.Add(LogTxt.Type.Exception, debug);
            }
            return productDatas;
        }
        private List<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
        {
            var dateRange = new List<DateTime>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                dateRange.Add(date);
            }
            return dateRange;
        }

        public async Task TestLog()
        {

            var productData = new CellData
            {

                Time = DateTime.Now,
                CellID = "CELL001",
                MachineName = "EQP001",
                ZoneNo = "1",
                Unit = "1",
                Stage = "1",
                TrackIn = "GOOD",
                TrackOut = "GOOD",

                InsRobot1ToolNo = "1",
                InsRobot2ToolNo = "2",

                Channel = new Channel
                {
                    ChannelNo = "1",
                    ContactResult = "GOOD",
                    LastResult = "GOOD",
                    DefectCode = "",
                    MTPWriteResult = "GOOD",
                    InsStartTime = DateTime.Now.AddMinutes(-5),
                    InsEndTime = DateTime.Now.AddMinutes(-4),
                    InsTackTime = 60.0,
                    ContactStartTime = DateTime.Now.AddMinutes(-2),
                    ContactEndTime = DateTime.Now.AddMinutes(-1),
                    ContactTackTime = 60.0,
                    MTPStartTime = DateTime.Now.AddMinutes(-5),
                    MTPEndTime = DateTime.Now.AddMinutes(-3),
                    MTPTackTime = 120.0,
                    TxHostVer = "V1.0",
                    X600 = "X600-01",
                    T5MACVer = "MAC001",
                    TMDFile = "TMDFile1",
                    PGUi = "PG_UI_1",
                    T5MacChannel = "Channel01",
                    Ibat = "IbatValue1",
                    IVss = "IVssValue1",
                    IDd = "IDdValue1",
                    ICi = "ICiValue1",
                    IBat2 = "IBat2Value1",
                    IDd2 = "IDd2Value1"
                },

                ABRule = "G",
                Retry = "0",
                Rechecked = "false",
                Temperater = "22.5",

                MCStartTime = DateTime.Now.AddMinutes(-10),
                MCEndTime = DateTime.Now.AddMinutes(-5),
                MCTackTime = 300.0,

                UnitStartTime = DateTime.Now.AddMinutes(-10),
                UnitEndTime = DateTime.Now.AddMinutes(-5),
                UnitTackTime = 300.0,

                RBDropStartTime = DateTime.Now.AddMinutes(-10),
                RBDropEndTime = DateTime.Now.AddMinutes(-9),
                RBDropTackTime = 60.0,

                SpareStartTime = DateTime.Now.AddMinutes(-8),
                SpareEndTime = DateTime.Now.AddMinutes(-7),
                SpareTackTime = 60.0,

                ServoFWStartTime = DateTime.Now.AddMinutes(-6),
                ServoFWEndTime = DateTime.Now.AddMinutes(-5),
                ServoFWTaktTime = 60.0,

                CylDWStartTime = DateTime.Now.AddMinutes(-4),
                CylDWEndTime = DateTime.Now.AddMinutes(-3),
                CylDWTaktTime = 60.0,

                CylUpStartTime = DateTime.Now.AddMinutes(-3),
                CylUpEndTime = DateTime.Now.AddMinutes(-2),
                CylUpTaktTime = 60.0,

                ServoBWStartTime = DateTime.Now.AddMinutes(-2),
                ServoBWEndTime = DateTime.Now.AddMinutes(-1),
                ServoBWTaktTime = 60.0,

                RBPickStartTime = DateTime.Now.AddMinutes(-1),
                RBPickEndTime = DateTime.Now,
                RBPickTackTime = 60.0
            };


          await  SaveDataLog(productData);
        }

        #endregion
        public async Task<bool> DisplayMessage(bool isYesNo, string message)
        {
            var result = await MessageDisplayEventHandle(isYesNo, message);
            return result;
        }
      
        public void UserLevelChange(int userLogin)
        {
            MainWindow.UserLogin = userLogin;
            HeaderStatusChanged?.Invoke();
        }
        #endregion
        #region Update
        public void Alive()
        {
            bool plcAlive = false;
            int plcCount = 0;
            bool isOn = false;

            BitModel bitAlive = null;

            while (true)
            {
                if (_plc != null)
                {
                    if (_plc.IsOpen)
                    {
                        try
                        {
                            isOn = !isOn;
                            if (bitAlive == null)
                            {
                                bitAlive = _plcH.Bits.FirstOrDefault(x => x.Item.ToUpper() == "ALIVE");
                            }

                            if (bitAlive != null)
                            {
                                bitAlive.SetPCValue = isOn;

                                if (plcAlive == bitAlive.GetPLCValue)
                                {
                                    plcCount++;
                                    if (plcCount > 100)
                                    {
                                        if (_isPlcConnected)
                                        {
                                            LogTxt.Add(LogTxt.Type.Status, "[PLC] Disconnect with PLC!");
                                            PlcConnectChangeEventHandle(false);
                                            _isPlcConnected = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!_isPlcConnected)
                                    {
                                        LogTxt.Add(LogTxt.Type.Status, "[PLC] Reconnect with PLC.");
                                    }
                                    _isPlcConnected = true;
                                    plcCount = 0;
                                    plcAlive = bitAlive.GetPLCValue;
                                    PlcConnectChangeEventHandle(true);
                                }
                            }
                            else
                            {
                                LogTxt.Add(LogTxt.Type.Exception, "[PLC] Cannot find Bit ALIVE!");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogTxt.Add(LogTxt.Type.Exception, $"[PLC] {ex.Message}");
                        }
                    }
                    else
                    {
                        if (_isPlcConnected)
                        {
                            LogTxt.Add(LogTxt.Type.Status, "[PLC] Disconnect because _plc.IsOpen = false.");
                            PlcConnectChangeEventHandle(false);
                            _isPlcConnected = false;
                        }
                    }
                }
                else
                {
                    if (_isPlcConnected)
                    {
                        LogTxt.Add(LogTxt.Type.Status, "[PLC] Disconnect because _plc == null.");
                        PlcConnectChangeEventHandle(false);
                        _isPlcConnected = false;
                    }
                }
                Thread.Sleep(200);
            }
        }

        #endregion

        #region EventHandle
        private void PlcConnectChangeEventHandle(bool isConnected)
        {
            var handle = PlcConnectChangeEvent;
            if (handle != null)
            {
                handle(isConnected);
            }
        }

        public void CurrDataEventHandle(List<CurrentData> currDatas)
        {
            var handle = CurrDataEvent;
            if (handle != null)
            {
                handle(currDatas);
            }
        }

        #endregion
    }
}
