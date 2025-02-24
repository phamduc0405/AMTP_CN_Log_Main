using APlc;
using ACO2_App._0.INIT;
using ACO2_App._0.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AExcel;

namespace ACO2_App._0.Plc
{
    public class PLCHelper
    {
        #region Enum

        #endregion
        #region Field
        private List<BitModel> _bit;
        private List<WordModel> _words;
        private List<PlcMemmory> _plcMemms;
        private APlc.PlcComm _plc;
        private Thread _update;
        #endregion
        #region Property
        public List<BitModel> Bits
        {
            get { return _bit; }
            set { _bit = value; }
        }
        public List<WordModel> Words
        {
            get { return _words; }
            set { _words = value; }
        }
        public List<PlcMemmory> PlcMemms
        {
            get { return _plcMemms; }
            set { _plcMemms = value; }
        }
        public string EqpId { get; set; } = null;
        #endregion
        #region Event
        public delegate void BitChangedEventDelegate(string Method, object data);
        public event BitChangedEventDelegate BitChangedEvent;
        public delegate void WordChangedEventDelegate(string Method, object data);
        public event WordChangedEventDelegate WordChangedEvent;
        #endregion
        #region Constructor

        public PLCHelper()
        {

        }
        #endregion

        #region Public Method
        public async Task LoadExcel(string ExcelPath)
        {

            _bit = await AExcel.ExcelHelper.ReadExcel<BitModel>(ExcelPath, "Handshake Bit");
            _words = await AExcel.ExcelHelper.ReadExcel<WordModel>(ExcelPath, "PC->PLC Word");
            var wplc = await AExcel.ExcelHelper.ReadExcel<WordModel>(ExcelPath, "PLC->PC Word");

            _words.AddRange(wplc);
            _plcMemms = await AExcel.ExcelHelper.ReadExcel<PlcMemmory>(ExcelPath, "MemoryConfig");
            foreach (var bit in _bit)
            {
                if (bit.LinkWord.Trim() != "x")
                {
                    if (_words.Any(x => x.Item == bit.LinkWord))
                    {
                        bit.Words = _words.Where(x => x.Item == bit.LinkWord).ToList();
                    }
                }
            }
        }
        public void Start(APlc.PlcComm plc, string eqp)
        {
            _plc = plc;
            EqpId = eqp;
            foreach (var b in _bit)
            {
                b.PLCs = _plc;
            }
            foreach (var w in Words)
            {
                w.PLCs = _plc;
            }

            _plc.BitChangedEvent -= PlcComm_BitChangedEvent;
            _plc.BitChangedEvent += PlcComm_BitChangedEvent;
            //_plc.WordChangedEvent -= PlcComm_WordChangedEvent;
            //_plc.WordChangedEvent += PlcComm_WordChangedEvent;
        }


        private void PlcComm_WordChangedEvent(APlc.MelsecIF.WordStatus status)
        {
            //try
            //{
            //    //Task.Run(() =>
            //    //{
            //    //    foreach (var w in _words)
            //    //    {
            //    //        if (status.Address <= (w.Address + w.Length - 1) && status.Address >= w.Address)
            //    //        {
            //    //            w.WordChangeFromPlc();
            //    //        }
            //    //    }
            //    //});
            //}
            //catch (Exception ex)
            //{
            //    var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.Message);
            //    LogTxt.Add(LogTxt.Type.Exception, debug);
            //}

        }

        public BitModel GetBitName(APlc.MelsecIF.BitStatus bitstatus)
        {
            BitModel bit = new BitModel(_plc);
            if (_bit.Any(x => x.PLCAddress == bitstatus.Address))
            {
                bit = _bit.FirstOrDefault(x => x.PLCAddress == bitstatus.Address);
            }
            return bit;
        }
        public void Close()
        {

            _update?.Abort();

        }
        #endregion

        #region Private Method
        private void PlcComm_BitChangedEvent(APlc.MelsecIF.BitStatus status)
        {

            BitModel bit = new BitModel(_plc);

            if (_bit.Any(x => x.PLCAddress == status.Address))
            {
                bit = _bit.FirstOrDefault(x => x.PLCAddress == status.Address);
                bit.BitChangeByPlc();
                if (bit.Type == "Event")
                {
                    if (!status.IsOn)
                    {
                        bit.SetPCValue = false;
                        return;
                    }
                    MakeLogBit(false, bit, status.IsOn);
                    BitChangedEventHandle(bit.Item, bit);
                }
                if (!status.IsOn) return;
                if (bit.Type == "Command")
                {
                    string name = bit.Item.Trim() + "CONFIRM";
                    BitChangedEventHandle(name, bit);
                    MakeLogBit(true, bit, status.IsOn);
                    bit.SetPCValue = false;
                }
                if (bit.Item.Contains("REQUESTO"))
                {
                    string name = "CRSTCONFIRM";
                    BitChangedEventHandle(name, bit);
                }
                else return;
            }
        }

        private void MakeLogBit(bool isSend, BitModel bit, bool value)
        {
            StringBuilder strlog = new StringBuilder();
            string str = string.Format("[{0}] [Bit]: Name:{1} Address:{2} Value:{3}", isSend ? "SEND" : "RECV", bit.Item, bit.PLCHexAdd, value.ToString());
            strlog.Append(str);
            List<WordModel> words = Words.Where(x => x.IsPlc == false).ToList();
            foreach (var item in words)
            {
                strlog.Append("\n\t");
                str = string.Format("Address:{0} Value:{1}", item.Start, item.GetValue);
                strlog.Append(str);
            }
            LogTxt.Add(LogTxt.Type.PLCMess, strlog.ToString(), EqpId);
        }
        #endregion
        #region EventHandle
        private void BitChangedEventHandle(string FuncName, object data)
        {
            var handle = BitChangedEvent;
            if (handle != null)
            {
                handle(FuncName, data);
            }
        }
        private void WordChangedEventHandle(string FuncName, object data)
        {
            var handle = WordChangedEvent;
            if (handle != null)
            {
                handle(FuncName, data);
            }
        }

        public PLCHelper Copy()
        {
            // Tạo đối tượng PLCHelper mới
            PLCHelper copy = new PLCHelper();

            // Sao chép các thuộc tính
            copy.EqpId = this.EqpId;

            // Sao chép sâu các danh sách bằng cách tạo danh sách mới và sao chép từng phần tử
            if (this.Bits != null)
            {
                copy.Bits = new List<BitModel>();
                foreach (var bit in this.Bits)
                {
                    // Cần implement một phương thức Clone() hoặc Copy() cho BitModel để tạo bản sao sâu
                    copy.Bits.Add(bit.Copy());
                }
            }

            if (this.Words != null)
            {
                copy.Words = new List<WordModel>();
                foreach (var word in this.Words)
                {
                    // Cần implement một phương thức Clone() hoặc Copy() cho WordModel để tạo bản sao sâu
                    copy.Words.Add(word.Copy());
                }
            }

            if (this.PlcMemms != null)
            {
                copy.PlcMemms = new List<PlcMemmory>();
                foreach (var plcMem in this.PlcMemms)
                {
                    // Cần implement một phương thức Clone() hoặc Copy() cho PlcMemmory để tạo bản sao sâu
                    copy.PlcMemms.Add(plcMem.Copy());
                }
            }

            // Sao chép các đối tượng khác nếu cần thiết
            // Đối với _plc và _update, bạn cần quyết định sao chép như thế nào tùy vào logic ứng dụng
            // copy._plc = this._plc; // Nếu chỉ muốn sao chép tham chiếu
            // copy._update = this._update; // Nếu chỉ muốn sao chép tham chiếu

            return copy;
        }
        #endregion
    }
}
