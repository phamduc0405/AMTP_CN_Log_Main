
using ACO2_App._0.INIT;
using ACO2_App._0.Plc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACO2_App._0.Model
{
    public class ControllerConfig
    {
        public string EQPID { get; set; } = string.Empty;
        public string PathLog {  get; set; }= Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).ToString()).ToString();
        public int DelLog { get; set; } = 60;
        public string AdminPass { get; set; } = string.Empty;
        public string OperatorPass { get; set; } = string.Empty;
        public string EngineerPass { get; set; } = string.Empty;

        public APlc.PLCConfig PLCConfig { get; set; } = new APlc.PLCConfig();
        public PLCHelper PLCHelper { get; set; } = new PLCHelper();
        public List<EquipmentConfig> EqpConfigs { get; set; } = new List<EquipmentConfig>();

    }
    public class EquipmentConfig
    {
        public int EQPIndex { get; set; } = 0;
        public string EQPID { get; set; } = string.Empty;
        public string EqpName { get; set; } = string.Empty;
        public string PCSignalIPAddress { get; set; } = "127.0.0.1";
        public ushort PCSignalPort { get; set; } = 4900;
        public bool PCSignalIsActive { get; set; } = false;
        public bool IsLineCheck { get; set; } = false;

        public List<Channel> Channels { get; set; } = new List<Channel> ();
    }
    public class ListCellDatas
    {
        public List<CellData> CellDatas { get; set; } = new List<CellData> ();
      
    }

    public class CellData
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public string MachineName { get; set; } = string.Empty; // Name Machine
        public string EQPID { get; set; } = string.Empty; // Name EQP
        public string Unit { get; set; } = string.Empty; // Name Unit
        public string Stage { get; set; } = string.Empty; // Name Stage
        public string CellID { get; set; } = string.Empty; // Code of Panel
        public string TrackIn { get; set; } = string.Empty;// Result TrackIn
        public string TrackOut { get; set; } = string.Empty; // Result TrackOut
        public DateTime MCStartTime { get; set; } = DateTime.Now; // When RobotLoader pickup panel to TrackIn
        public DateTime MCEndTime { get; set; } = DateTime.Now; // When Robot drop panel to TrackOut
        public double MCTackTime { get; set; } = 0; // Total time from MCStarttime to MCEndTime
        public string InsRobot1ToolNo { get; set; } = string.Empty; // Number Tool Robot Using for panel
        public string InsRobot2ToolNo { get; set; } = string.Empty; // Number Tool Robot Using for panel
        public string ZoneNo { get; set; } = string.Empty; // Zone Number panel now
        public DateTime UnitStartTime { get; set; } = DateTime.Now; // When Robot drop panel to JIG
        public DateTime UnitEndTime { get; set; } = DateTime.Now; // When Robot pickup panel from JIG
        public double UnitTackTime { get; set; } = 0; // Total time from UnitStartTime to UnitEndTime

        public DateTime RBDropStartTime { get; set; } = DateTime.Now; // When Robot start drop panel to JIG
        public DateTime RBDropEndTime { get; set; } = DateTime.Now; // When Robot end drop panel done
        public double RBDropTackTime { get; set; } = 0; // Total time from RBDropStartTime to RBDropEndTime

        public DateTime SpareStartTime { get; set; } = DateTime.Now; 
        public DateTime SpareEndTime { get; set; } = DateTime.Now; 
        public double SpareTackTime { get; set; } = 0;

        public DateTime ServoFWStartTime { get; set; } = DateTime.Now; // When Servo Start FW
        public DateTime ServoFWEndTime { get; set; } = DateTime.Now; // When Servo End FW
        public double ServoFWTaktTime { get; set; } = 0;// Total time from ServoFWStartTime to ServoFWEndTime

        public DateTime CylDWStartTime { get; set; } = DateTime.Now; //When Cyl Start DW
        public DateTime CylDWEndTime { get; set; } = DateTime.Now;//When Cyl End DW
        public double CylDWTaktTime { get; set; } = 0;// Total time from CylDWStartTime to CylDWEndTime

        public DateTime CylUpStartTime { get; set; } = DateTime.Now; //When Cyl Start Up
        public DateTime CylUpEndTime { get; set; } = DateTime.Now;//When Cyl End Up
        public double CylUpTaktTime { get; set; } = 0;// Total time from CylUpStartTime to CylUpEndTime

        public DateTime ServoBWStartTime { get; set; } = DateTime.Now; // When Servo Start BW
        public DateTime ServoBWEndTime { get; set; } = DateTime.Now; // When Servo End BW
        public double ServoBWTaktTime { get; set; } = 0;// Total time from ServoBWStartTime to ServoBWEndTime

        public DateTime RBPickStartTime { get; set; } = DateTime.Now; // When Robot start pick panel to JIG
        public DateTime RBPickEndTime { get; set; } = DateTime.Now; // When Robot end pick panel done
        public double RBPickTackTime { get; set; } = 0; // Total time from RBPickStartTime to RBPickEndTime

        public string ABRule { get; set; } = string.Empty; // AB Rule
        public string Retry { get; set; } = string.Empty; // Retry
        public string Rechecked { get; set; } = string.Empty; // Rechecked
        public string Temperater {  get; set; } = string.Empty; // Temperater
        public Channel Channel { get; set; } = new Channel ();

        public void Clear()
        {
            this.CellID = "";
            this.TrackIn = "";
            this.TrackOut = "";
            this.Unit = "";
            this.Stage = "";
            this.MCStartTime = DateTime.MinValue;
            this.MCEndTime = DateTime.MinValue;
            this.MCTackTime = 0;
            this.InsRobot1ToolNo = "";
            this.InsRobot2ToolNo = "";
            this.ABRule = "";
            this.Retry = "";
            this.Rechecked = "";
            this.Temperater = "";
            this.ZoneNo = "";
            this.UnitStartTime = DateTime.MinValue;
            this.UnitEndTime = DateTime.MinValue;
            this.UnitTackTime = 0;
            this.RBDropStartTime = DateTime.MinValue;
            this.RBDropEndTime = DateTime.MinValue;
            this.RBDropTackTime = 0;
            this.SpareStartTime = DateTime.MinValue;
            this.SpareEndTime = DateTime.MinValue;
            this.SpareTackTime = 0;
            this.ServoFWStartTime = DateTime.MinValue;
            this.ServoFWEndTime = DateTime.MinValue;
            this.ServoFWTaktTime = 0;
            this.CylDWStartTime = DateTime.MinValue;
            this.CylDWEndTime = DateTime.MinValue;
            this.CylDWTaktTime = 0;
            this.CylUpStartTime = DateTime.MinValue;
            this.CylUpEndTime = DateTime.MinValue;
            this.CylUpTaktTime = 0;
            this.ServoBWStartTime = DateTime.MinValue;
            this.ServoBWEndTime = DateTime.MinValue;
            this.ServoBWTaktTime = 0;
            this.RBPickStartTime = DateTime.MinValue;
            this.RBPickEndTime = DateTime.MinValue;
            this.RBPickTackTime = 0;
            this.Channel.Clear();

        }

    }
    public class Channel
    {
        public string CellID { get; set; } = string.Empty; // Code of Panel
        public string LastResult { get; set; } = string.Empty; // Result  of panel (Receive from PCSignal)

        public string TxHostVer { get; set; } = string.Empty; // Version of TxHost (Receive from PCSignal)
        public string ChannelNo { get; set; } = string.Empty; // Number channel panel using (Receive from PLC)
        public string X600 { get; set; } = string.Empty; // Version X600 channel using (Receive from PCSignal or ConfigPC)
        public string T5MACVer { get; set; } = string.Empty; // MAC Address channel using (Receive from PCSignal or ConfigPC)
        public string TMDFile { get; set; } = string.Empty; // TMD File channel using (Receive from PCSignal or ConfigPC)
        public string PGUi { get; set; } = string.Empty; // PG UI channel using (Receive from PCSignal or ConfigPC)
        public string T5MacChannel { get; set; } = string.Empty; //T5 MAC CHANNEL channel using (Receive from PCSignal or ConfigPC)

        public DateTime InsStartTime { get; set; } = DateTime.Now; // When panel start check CONTACT
        public DateTime InsEndTime { get; set; } = DateTime.Now; // When panel MTPWrite Done
        public double InsTackTime { get; set; } = 0; // Total time from InsStartTime to InsEndTime
        public DateTime ContactStartTime { get; set; } = DateTime.Now; // When panel start check CONTACT
        public DateTime ContactEndTime { get; set; } = DateTime.Now; // When panel CONTACT Done
        public double ContactTackTime { get; set; } = 0; // Total time from ContactStartTime to ContactEndTime
        public string ContactResult { get; set; } = string.Empty; // Result Contact of panel (Receive from PCSignal)
        public DateTime MTPStartTime { get; set; } = DateTime.Now; // When panel start check MTPWrite
        public DateTime MTPEndTime { get; set; } = DateTime.Now; // When panel MTPWrite Done
        public double MTPTackTime { get; set; } = 0; // Total time from MTPStartTime to MTPEndTime
        public string MTPWriteResult { get; set; } = string.Empty; // Result MTPWrite of panel (Receive from PCSignal)
        public string DefectCode { get; set; } = string.Empty; // Defect Code of panel (Receive from PCSignal)
        public string Ibat { get; set; } = string.Empty; //Ibat of panel (Receive from PCSignal)
        public string IVss { get; set; } = string.Empty; // IVss of panel (Receive from PCSignal)
        public string IDd { get; set; } = string.Empty; // IDd of panel (Receive from PCSignal)
        public string ICi { get; set; } = string.Empty; // ICi of panel (Receive from PCSignal)
        public string IBat2 { get; set; } = string.Empty; // IBat2 of panel (Receive from PCSignal)
        public string IDd2 { get; set; } = string.Empty; // IDd2 of panel (Receive from PCSignal)

        public void Clear()
        {
            this.CellID = "";
            this.TxHostVer = "";
            this.ChannelNo = "";
            this.X600 = "";
            this.T5MACVer = "";
            this.TMDFile = "";
            this.PGUi = "";
            this.T5MacChannel = "";
            this.InsStartTime = DateTime.MinValue;
            this.InsEndTime = DateTime.MinValue;
            this.InsTackTime = 0;
            this.ContactStartTime = DateTime.MinValue;
            this.ContactEndTime = DateTime.MinValue;
            this.ContactTackTime = 0;
            this.ContactResult = "";
            this.MTPStartTime = DateTime.MinValue;
            this.MTPEndTime = DateTime.MinValue;
            this.MTPTackTime = 0;
            this.MTPWriteResult = "";
            this.DefectCode = "";
            this.Ibat = "";
            this.IVss = "";
            this.IDd = "";
            this.ICi = "";
            this.IBat2 = "";
            this.IDd2 = "";
        }
        public delegate void ResultInsEventDelegate(string channelNo,string ContactResult,string MTPWriteResult,string DefectCode);
        public event ResultInsEventDelegate ResultInsEvent;
        public void ResultInsEventHandle(string channelNo, string ContactResult, string MTPWriteResult, string DefectCode)
        {
            var handle = ResultInsEvent;
            if (handle != null)
            {
                handle(channelNo, ContactResult, MTPWriteResult, DefectCode);
            }
        }
    }
    public class MachineStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _status = string.Empty;
        private string _availabilityState = string.Empty;
        private string _interlockState = string.Empty;
        private string _moveState = string.Empty;
        private string _runState = string.Empty;
        public string Status // AUTO/MANUAL/ERROR
        {
            get => _status;
            set => SetProperty(ref _status, value, nameof(Status));
        }

        public string AvailabilityState // UP/DOWN
        {
            get => _availabilityState;
            set => SetProperty(ref _availabilityState, value, nameof(AvailabilityState));
        }

        public string InterlockState // ON/OFF
        {
            get => _interlockState;
            set => SetProperty(ref _interlockState, value, nameof(InterlockState));
        }

        public string MoveState // RUNNING/PAUSE
        {
            get => _moveState;
            set => SetProperty(ref _moveState, value, nameof(MoveState));
        }

        public string RunState // RUN/IDLE
        {
            get => _runState;
            set => SetProperty(ref _runState, value, nameof(RunState));
        }
        public List<ChannelStatus> ChannelStatus { get; set; } = new List<ChannelStatus>();
        private void SetProperty(ref string field, string newValue, string propertyName)
        {
            if (field != newValue)
            {
                string str = $"[MACHINE]{propertyName} changed from '{field}' to '{newValue}'";
                LogTxt.Add(LogTxt.Type.Status, str);
                LogTxt.Add(LogTxt.Type.FlowRun, str);
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public class ChannelStatus
    {
        public event PropertyChangedEventHandler PropertyChannelChanged;
        private string _status = string.Empty;

        public string ZoneNo { get; set; } = string.Empty;
        public string Channnel { get; set; } = string.Empty;
        public string Status // AUTO / MANUAL / SKIP
        {
            get => _status;
            set => SetProperty(ref _status, value, nameof(Status));
        }
        private void SetProperty(ref string field, string newValue, string propertyName)
        {
            if (field != newValue)
            {
                string str = $"[CHANNEL][{ZoneNo}][{Channnel}]{propertyName} changed from '{field}' to '{newValue}'";
                LogTxt.Add(LogTxt.Type.Status, str);
                LogTxt.Add(LogTxt.Type.FlowRun, str);
                field = newValue;
                PropertyChannelChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public class CurrentData
    {
        public DateTime Time { get; set; }
        public string Zone { get; set; } = "";
        public string ChannelName { get; set; } = "";
        public int Good { get; set; } = 0;
        public int NGContact { get; set; } = 0;
        public int NGIns { get; set; } = 0;
        public int Total { get { return Good + NGContact + NGIns; } }
     

        public void Clear()
        {
            this.Good = 0;
            this.NGContact = 0;
            this.NGIns = 0;

        }
    }
}
