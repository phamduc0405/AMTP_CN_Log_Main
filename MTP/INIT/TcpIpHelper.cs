using AComm.AP;
using HPSocket.Sdk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACO2.INIT
{
    public class TcpIpHelper
    {

        #region Field
        private bool _isConnected = false;
        private string _ip;
        private string _port;
        private bool _isActive;
        private string _id;
        private ATCPIP.TCPConnect _connect;
        #endregion
        #region Property
        public string IP
        {
            get { return _ip; }
            set { _ip = value; }
        }
        public string Port
        {
            get { return _port; }
            set { _port = value; }
        }
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        public string Id
        {
            get { return _id; }
            private set { _id = value; }
        }
        public ATCPIP.TCPConnect Connect
        {
            get { return _connect; }

        }
        public bool IsConnected
        {
            get { return _isConnected; }

        }
        #endregion

        #region Event
        public delegate void ReciveEventDelegate(string value, string id, TcpIpHelper tcp);
        public event ReciveEventDelegate ReciveEvent;

        public delegate void ReciveByteEventDelegate(byte[] value, string id, TcpIpHelper tcp);
        public event ReciveByteEventDelegate ReciveByteEvent;
        #endregion
        #region Constructor
        public TcpIpHelper(string ip, string port, bool isActive, string id = "")
        {
            _id = id;
            ushort port_id = 0;
            if (ushort.TryParse(port, out port_id)) { }
            if (_connect == null)
            {
                _connect = new ATCPIP.TCPConnect(_id);
                _connect.ReceiveEvent -= _connect_ReceiveEvent;
                _connect.ReceiveEvent += _connect_ReceiveEvent;
                _connect.OnConnectEvent -= _connect_OnConnectEvent;
                _connect.OnConnectEvent += _connect_OnConnectEvent;
                //  _connect.ConnectMode = ATCPIP.ConnectMode.Passive;
                _connect.Start(ip, port_id, isActive);
            }
        }

        private void _connect_OnConnectEvent(bool IsConnected)
        {
            _isConnected = IsConnected;
        }

        private void _connect_ReceiveEvent(byte[] obj)
        {
            Task.Run(() =>
            {
                string str = Encoding.UTF8.GetString(obj); ;

                ReciveEventHandle(str, _id);
                ReciveByteEventHandle(obj, _id);
            });
        }

        public void Close()
        {
            if (_connect != null)
            {
                _connect.Close();
            }
        }
        #endregion
        #region Private Void

        #endregion
        #region Public Void

        #endregion
        #region EventHandle
        private void ReciveEventHandle(string value, string id)
        {
            var handle = ReciveEvent;
            if (handle != null)
            {
                handle(value, id, this);
            }
        }
        private void ReciveByteEventHandle(Byte[] value, string id)
        {
            var handle = ReciveByteEvent;
            if (handle != null)
            {
                handle(value, id, this);
            }
        }
        #endregion
        #region Update

        #endregion

    }
}
