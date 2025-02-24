using ACO2_App._0.INIT;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACO2.PlcComm
{
    internal class SerialHelper
    {
        #region Event
        /// <summary>
        /// array[0]:Station; Array[1-n]: Data
        /// </summary>

        public event Action<int,string> DataReceived;
        #endregion
        #region File
        private int id;
        private Thread ThreadTracking;
        private SerialPort _serialPort;
        private int _Maxstation;
        private int _Scantime;
        #endregion

        #region Contruction
        //TODO: INIT COMPORT
        /// <summary>
        /// Scantime default: 500ms
        /// </summary>
        /// <param name="MaxStation"></param>
        /// <param name="Scantime"></param>
        public SerialHelper(int MaxStation ,int id = 0, int Scantime = 500)
        {
            _Maxstation = MaxStation;
            _Scantime = Scantime <= 200 ? 200 : Scantime;
            this.id = id;
        }
        #region Init Comport
        public void Init(string pComPort)
        {
            InitCom(pComPort, 9600, 8, Parity.None, StopBits.One, Handshake.None);
        }
        public void Init(string pComPort, int BaudRate)
        {
            InitCom(pComPort, BaudRate, 8, Parity.None, StopBits.One, Handshake.None);
        }
        public void Init(string pComPort, int BaudRate, int pDatabit)
        {
            InitCom(pComPort, BaudRate, pDatabit, Parity.None, StopBits.One, Handshake.None);
        }
        public void Init(string pComPort, int BaudRate, int pDatabit, Parity pParity)
        {

            InitCom(pComPort, BaudRate, pDatabit, pParity, StopBits.One, Handshake.None);
        }
        public void Init(string pComPort, int BaudRate, int pDatabit, Parity pParity, StopBits pStopbit, Handshake pHandshake)
        {

            InitCom(pComPort, BaudRate, pDatabit, pParity, StopBits.One, pHandshake);
        }
        public void Init(string pComPort, int BaudRate, int pDatabit, Parity pParity, StopBits pStopbit)
        {
            InitCom(pComPort, BaudRate, pDatabit, pParity, pStopbit, Handshake.None);
        }
        private void InitCom(string pComPort, int BaudRate, int pDatabit, Parity pParity, StopBits pStopbit, Handshake pHandshake)
        {
            _serialPort = new SerialPort(pComPort, BaudRate, pParity, pDatabit, pStopbit);
            _serialPort.Handshake = pHandshake;
            _serialPort.WriteTimeout = 500;
            _serialPort.DataReceived -= new SerialDataReceivedEventHandler(onDataReceived);
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(onDataReceived);
        }
        #endregion
        public bool Start()
        {
            try
            {

                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                }

                return true;
            }
            catch (Exception exception)
            {
                var debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, exception.Message);
                LogTxt.Add(LogTxt.Type.Exception, "SerialHelper LV32 Start:" + debug);
            }
            return false;
        }

        private void onDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(500);
                string data = _serialPort.ReadExisting();
                DataReceived(id,data);
            }
            catch (Exception exception)
            {
                var debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, exception.Message);
                LogTxt.Add(LogTxt.Type.Exception, "SerialHelper LV32 onDataReceived:" + debug);
            }
        }



        #endregion
        #region Event Handlle

        #endregion
    }
}
