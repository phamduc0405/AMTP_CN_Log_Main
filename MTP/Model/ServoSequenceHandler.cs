using ACO2_App._0.INIT;
using ACO2_App._0.Model;
using ACO2_App._0;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MTP.Model
{
    public class ServoSequenceHandler
    {
        public enum ActutorAction
        {
            Zone1Servo1StartFw,
            Zone1Servo1StartBw,
            Zone1Servo2StartFw,
            Zone1Servo2StartBw,

            Zone2Servo1StartFw,
            Zone2Servo1StartBw,
            Zone2Servo2StartFw,
            Zone2Servo2StartBw,

            Zone1Servo1EndFw,
            Zone1Servo1EndBw,
            Zone1Servo2EndFw,
            Zone1Servo2EndBw,

            Zone2Servo1EndFw,
            Zone2Servo1EndBw,
            Zone2Servo2EndFw,
            Zone2Servo2EndBw,
        }
        private Controller _controller;

        private Dictionary<ActutorAction, Func<Task>> _handlers;
        private ActutorAction _action;

        public ServoSequenceHandler(string action)
        {
            _controller = MainWindow.Controller;
            Initial(action);
            HandleAction();
        }

        private void Initial(string action)
        {
            var actionMap = new Dictionary<string, ActutorAction>(StringComparer.OrdinalIgnoreCase)
        {
            // ZONE 1 - FW Start
            { Bit.ZONE1_SERVO_1_FW_START, ActutorAction.Zone1Servo1StartFw },
            { Bit.ZONE1_SERVO_2_FW_START, ActutorAction.Zone1Servo2StartFw },
            // ZONE 1 - BW Start
            { Bit.ZONE1_SERVO_1_BW_START, ActutorAction.Zone1Servo1StartBw },
            { Bit.ZONE1_SERVO_2_BW_START, ActutorAction.Zone1Servo2StartBw },
            // ZONE 2 - FW Start
            { Bit.ZONE2_SERVO_1_FW_START, ActutorAction.Zone2Servo1StartFw },
            { Bit.ZONE2_SERVO_2_FW_START, ActutorAction.Zone2Servo2StartFw },
            // ZONE 2 - BW Start
            { Bit.ZONE2_SERVO_1_BW_START, ActutorAction.Zone2Servo1StartBw },
            { Bit.ZONE2_SERVO_2_BW_START, ActutorAction.Zone2Servo2StartBw },
            // ZONE 1 - FW End
            { Bit.ZONE1_SERVO_1_FW_END, ActutorAction.Zone1Servo1EndFw },
            { Bit.ZONE1_SERVO_2_FW_END, ActutorAction.Zone1Servo2EndFw },
            // ZONE 1 - BW End
            { Bit.ZONE1_SERVO_1_BW_END, ActutorAction.Zone1Servo1EndBw },
            { Bit.ZONE1_SERVO_2_BW_END, ActutorAction.Zone1Servo2EndBw },
            // ZONE 2 - FW End
            { Bit.ZONE2_SERVO_1_FW_END, ActutorAction.Zone2Servo1EndFw },
            { Bit.ZONE2_SERVO_2_FW_END, ActutorAction.Zone2Servo2EndFw },
            // ZONE 2 - BW End
            { Bit.ZONE2_SERVO_1_BW_END, ActutorAction.Zone2Servo1EndBw },
            { Bit.ZONE2_SERVO_2_BW_END, ActutorAction.Zone2Servo2EndBw },
        };

            if (actionMap.TryGetValue(action, out ActutorAction mappedAction))
            {
                _action = mappedAction;
            }
            else
            {
                LogTxt.Add(LogTxt.Type.Exception, $"[ACTUTOR] Unknown Action: {action}");
                return;
            }


            _handlers = new Dictionary<ActutorAction, Func<Task>>
        {
            { ActutorAction.Zone1Servo1StartFw, async () => await HandleServoStart(1,1,"FW") },
            { ActutorAction.Zone1Servo1StartBw, async () => await HandleServoStart(1,1, "BW") },
            { ActutorAction.Zone1Servo2StartFw, async () => await HandleServoStart(1,2,"FW") },
            { ActutorAction.Zone1Servo2StartBw, async () => await HandleServoStart(1,2,"BW") },

            { ActutorAction.Zone2Servo1StartFw, async () => await HandleServoStart(2, 1, "FW") },
            { ActutorAction.Zone2Servo1StartBw, async () => await HandleServoStart(2, 1, "BW") },
            { ActutorAction.Zone2Servo2StartFw, async () => await HandleServoStart(2, 2, "FW") },
            { ActutorAction.Zone2Servo2StartBw, async () => await HandleServoStart(2, 2, "BW") },

            { ActutorAction.Zone1Servo1EndFw, async () => await HandleServoEnd(1,1,"FW") },
            { ActutorAction.Zone1Servo1EndBw, async () => await HandleServoEnd(1,1, "BW") },
            { ActutorAction.Zone1Servo2EndFw, async () => await HandleServoEnd(1,2,"FW") },
            { ActutorAction.Zone1Servo2EndBw, async () => await HandleServoEnd(1,2,"BW") },

            { ActutorAction.Zone2Servo1EndFw, async () => await HandleServoEnd(2, 1, "FW") },
            { ActutorAction.Zone2Servo1EndBw, async () => await HandleServoEnd(2, 1, "BW") },
            { ActutorAction.Zone2Servo2EndFw, async () => await HandleServoEnd(2, 2, "FW") },
            { ActutorAction.Zone2Servo2EndBw, async () => await HandleServoEnd(2, 2, "BW") },
        };

        }
        private void HandleAction()
        {
            if (_handlers.TryGetValue(_action, out Func<Task> handler))
            {
                handler.Invoke();
            }
            else
            {
                LogTxt.Add(LogTxt.Type.Exception, $"[ACTUTOR] No handler found for action {_action}");
            }
        }

        private async Task HandleServoStart(int zone, int unit, string action)
        {
          var cellData = _controller.ListCellDatas.CellDatas.Where(x => x.ZoneNo == zone.ToString() && x.Unit == unit.ToString()).ToList();
            switch (action)
            {
                case "FW":
                  
                    if (cellData.Count > 0)
                    {
                        foreach(var cell in cellData)
                        {
                            cell.ServoFWStartTime = DateTime.Now;
                            LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][START][FW]: CellData Updated:");
                        }
                    }
                    else
                    {
                        LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][START][FW]: CellData Cannot find in List:" );
                    }
                    break;
                case "BW":
                    if (cellData.Count > 0)
                    {
                        foreach (var cell in cellData)
                        {
                            cell.ServoBWStartTime = DateTime.Now;
                            LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][START][BW]: CellData Updated:");
                        }
                    }
                    else
                    {
                        LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][START][BW]: CellData Cannot find in List:");
                    }
                    break;
            }
        }
        private async Task HandleServoEnd(int zone, int unit, string action)
        {
            var cellData = _controller.ListCellDatas.CellDatas.Where(x => x.ZoneNo == zone.ToString() && x.Unit == unit.ToString()).ToList();
            switch (action)
            {
                case "FW":

                    if (cellData.Count > 0)
                    {
                        foreach (var cell in cellData)
                        {
                            cell.ServoFWEndTime = DateTime.Now;
                            cell.ServoFWTaktTime = (cell.ServoFWEndTime - cell.ServoFWStartTime).TotalSeconds;

                            LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][END][FW]: CellData Updated:");
                        }
                    }
                    else
                    {
                        LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][END][FW]: CellData Cannot find in List:");
                    }
                    break;
                case "BW":
                    if (cellData.Count > 0)
                    {
                        foreach (var cell in cellData)
                        {
                            cell.ServoBWEndTime = DateTime.Now;
                            cell.ServoBWTaktTime = (cell.ServoBWEndTime - cell.ServoBWStartTime).TotalSeconds;

                            LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][END][BW]: CellData Updated:");
                        }
                    }
                    else
                    {
                        LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][END][BW]: CellData Cannot find in List:");
                    }
                    break;
            }
        }
    }
}
