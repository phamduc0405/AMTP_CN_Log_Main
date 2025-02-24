using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACO2.Model
{
    public class TCPIPModel
    {
        #region Field
        private string _ipAdd = "127.0.0.1";
        private string _port = "3000";
        private bool _isActive = false;
        private string _quantity = "1";
        private int _timeDelay = 10;
        #endregion
        #region Property
        public string IPAdd { get { return _ipAdd; } set { _ipAdd = value; } }
        public string Port { get { return _port; } set { _port = value; } }
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }
        public string Quantity { get { return _quantity; } set { _quantity = value; } }
        public int TimeDelay { get { return _timeDelay; } set { _timeDelay = value; } }
        #endregion

    }
}
