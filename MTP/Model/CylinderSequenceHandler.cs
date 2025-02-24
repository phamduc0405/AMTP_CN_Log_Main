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
            var match = System.Text.RegularExpressions.Regex.Match(action, @"ZONE(?<zone>\d+)_CYL_(START|END)_(?<action>\w+)_(?<channel>\d+)");

            if (match.Success)
            {
                _zone = int.Parse(match.Groups["zone"].Value);
                _state = match.Groups[1].Value; // "START" or "END"
                _actionStr = match.Groups["action"].Value; // "DW"
                _channel = int.Parse(match.Groups["channel"].Value);
            }
            else
            {
                LogTxt.Add(LogTxt.Type.Exception, $"[ACTUTOR] Invalid Action Format: {action}");
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
