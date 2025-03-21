using ACO2_App._0.INIT;
using ACO2_App._0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACO2_App._0.Model;

namespace MTP.Model
{

    public class CylinderSequenceHandler
    {
       
        private Controller _controller;
        private int _zone;
        private string _state;
        private string _actionStr;
        private int _channel;
        public CylinderSequenceHandler(string action)
        {
            _controller = MainWindow.Controller;
            Initial(action);
            HandlerCylinder();
        }

        private void Initial(string action)
        {
            try
            {
                var parts = action.Split('_');
                // parts[0] = "ZONE1", parts[1] = "CYL", parts[2] = "1", parts[3] = "UP", parts[4] = "START"
                _zone = int.Parse(parts[0].Replace("ZONE", ""));
                _actionStr = parts[3]; // "UP" or "DOWN"
                _state = parts[4]; // "START" or "END"
                _channel = int.Parse(parts[2]); 
            }
           catch(Exception ex)
            {
                LogTxt.Add(LogTxt.Type.Exception, $"[ACTUTOR] {ex.Message}");
                return;
            }
            
        }
    
        private void HandlerCylinder()
        {
            CellData cellData = _controller.ListCellDatas.CellDatas.FirstOrDefault(x => x.ZoneNo==_zone.ToString()&& x.Channel.ChannelNo == _channel.ToString());
            if (cellData != null)
            {
                switch(_state)
                {
                    case "START":
                        if(_actionStr == "DW")
                        {
                            cellData.CylDWStartTime = DateTime.Now;
                        }
                        if (_actionStr == "UP")
                        {
                            cellData.CylUpStartTime = DateTime.Now;
                        }
                        break;
                    case "END":
                        if (_actionStr == "DW")
                        {
                            cellData.CylDWEndTime = DateTime.Now;
                            cellData.CylDWTaktTime = (cellData.CylDWEndTime- cellData.CylDWStartTime).TotalMilliseconds;
                        }
                        if (_actionStr == "UP")
                        {
                            cellData.CylUpEndTime = DateTime.Now;
                            cellData.CylUpTaktTime = (cellData.CylUpEndTime - cellData.CylUpStartTime).TotalMilliseconds;

                        }
                        break;
                }
            }
        }
      
    }
}
