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

            Zone1Servo1EndFw,
            Zone1Servo1EndBw,
            Zone1Servo2EndFw,
            Zone1Servo2EndBw,

            Zone1Servo3StartFw,
            Zone1Servo3StartBw,
            Zone1Servo4StartFw,
            Zone1Servo4StartBw,

            Zone1Servo3EndFw,
            Zone1Servo3EndBw,
            Zone1Servo4EndFw,
            Zone1Servo4EndBw,

            Zone1Servo5StartFw,
            Zone1Servo5StartBw,
            Zone1Servo6StartFw,
            Zone1Servo6StartBw,

            Zone1Servo5EndFw,
            Zone1Servo5EndBw,
            Zone1Servo6EndFw,
            Zone1Servo6EndBw,

            Zone2Servo1StartFw,
            Zone2Servo1StartBw,
            Zone2Servo2StartFw,
            Zone2Servo2StartBw,

            Zone2Servo1EndFw,
            Zone2Servo1EndBw,
            Zone2Servo2EndFw,
            Zone2Servo2EndBw,

            Zone2Servo3StartFw,
            Zone2Servo3StartBw,
            Zone2Servo4StartFw,
            Zone2Servo4StartBw,

            Zone2Servo3EndFw,
            Zone2Servo3EndBw,
            Zone2Servo4EndFw,
            Zone2Servo4EndBw,

            Zone2Servo5StartFw,
            Zone2Servo5StartBw,
            Zone2Servo6StartFw,
            Zone2Servo6StartBw,

            Zone2Servo5EndFw,
            Zone2Servo5EndBw,
            Zone2Servo6EndFw,
            Zone2Servo6EndBw,
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
            // ZONE 1 - Servo 1&2- FW Start
            { Bit.ZONE1_SERVO_1_FW_START, ActutorAction.Zone1Servo1StartFw },
            { Bit.ZONE1_SERVO_2_FW_START, ActutorAction.Zone1Servo2StartFw },
            // ZONE 1 - Servo 1&2- BW Start
            { Bit.ZONE1_SERVO_1_BW_START, ActutorAction.Zone1Servo1StartBw },
            { Bit.ZONE1_SERVO_2_BW_START, ActutorAction.Zone1Servo2StartBw },
             // ZONE 1 - Servo 1&2- FW End
            { Bit.ZONE1_SERVO_1_FW_END, ActutorAction.Zone1Servo1EndFw },
            { Bit.ZONE1_SERVO_2_FW_END, ActutorAction.Zone1Servo2EndFw },
            // ZONE 1 - Servo 1&2- BW End
            { Bit.ZONE1_SERVO_1_BW_END, ActutorAction.Zone1Servo1EndBw },
            { Bit.ZONE1_SERVO_2_BW_END, ActutorAction.Zone1Servo2EndBw },

             // ZONE 1 - Servo 3&4- FW Start
            { Bit.ZONE1_SERVO_3_FW_START, ActutorAction.Zone1Servo3StartFw },
            { Bit.ZONE1_SERVO_4_FW_START, ActutorAction.Zone1Servo4StartFw },
            // ZONE 1 - Servo 3&4- BW Start
            { Bit.ZONE1_SERVO_3_BW_START, ActutorAction.Zone1Servo3StartBw },
            { Bit.ZONE1_SERVO_4_BW_START, ActutorAction.Zone1Servo4StartBw },
             // ZONE 1 - Servo 3&4- FW End
            { Bit.ZONE1_SERVO_3_FW_END, ActutorAction.Zone1Servo3EndFw },
            { Bit.ZONE1_SERVO_4_FW_END, ActutorAction.Zone1Servo4EndFw },
            // ZONE 1 - Servo 3&4- BW End
            { Bit.ZONE1_SERVO_3_BW_END, ActutorAction.Zone1Servo3EndBw },
            { Bit.ZONE1_SERVO_4_BW_END, ActutorAction.Zone1Servo4EndBw },

             // ZONE 1 - Servo 5&6- FW Start
            { Bit.ZONE1_SERVO_5_FW_START, ActutorAction.Zone1Servo5StartFw },
            { Bit.ZONE1_SERVO_6_FW_START, ActutorAction.Zone1Servo6StartFw },
            // ZONE 1 - Servo 5&6- BW Start
            { Bit.ZONE1_SERVO_5_BW_START, ActutorAction.Zone1Servo5StartBw },
            { Bit.ZONE1_SERVO_6_BW_START, ActutorAction.Zone1Servo6StartBw },
             // ZONE 1 - Servo 5&6- FW End
            { Bit.ZONE1_SERVO_5_FW_END, ActutorAction.Zone1Servo5EndFw },
            { Bit.ZONE1_SERVO_6_FW_END, ActutorAction.Zone1Servo6EndFw },
            // ZONE 1 - Servo 5&6- BW End
            { Bit.ZONE1_SERVO_5_BW_END, ActutorAction.Zone1Servo5EndBw },
            { Bit.ZONE1_SERVO_6_BW_END, ActutorAction.Zone1Servo6EndBw },

            // ZONE 2 - Servo 1&2- FW Start
            { Bit.ZONE2_SERVO_1_FW_START, ActutorAction.Zone2Servo1StartFw },
            { Bit.ZONE2_SERVO_2_FW_START, ActutorAction.Zone2Servo2StartFw },
            // ZONE 2 - Servo 1&2- BW Start
            { Bit.ZONE2_SERVO_1_BW_START, ActutorAction.Zone2Servo1StartBw },
            { Bit.ZONE2_SERVO_2_BW_START, ActutorAction.Zone2Servo2StartBw },
            // ZONE 2 - Servo 1&2- FW End
            { Bit.ZONE2_SERVO_1_FW_END, ActutorAction.Zone2Servo1EndFw },
            { Bit.ZONE2_SERVO_2_FW_END, ActutorAction.Zone2Servo2EndFw },
            // ZONE 2 - Servo 1&2- BW End
            { Bit.ZONE2_SERVO_1_BW_END, ActutorAction.Zone2Servo1EndBw },
            { Bit.ZONE2_SERVO_2_BW_END, ActutorAction.Zone2Servo2EndBw },

             // ZONE 2 - Servo 3&4- FW Start
            { Bit.ZONE2_SERVO_3_FW_START, ActutorAction.Zone2Servo3StartFw },
            { Bit.ZONE2_SERVO_4_FW_START, ActutorAction.Zone2Servo4StartFw },
            // ZONE 2 - Servo 3&4- BW Start
            { Bit.ZONE2_SERVO_3_BW_START, ActutorAction.Zone2Servo3StartBw },
            { Bit.ZONE2_SERVO_4_BW_START, ActutorAction.Zone2Servo4StartBw },
             // ZONE 2 - Servo 3&4- FW End
            { Bit.ZONE2_SERVO_3_FW_END, ActutorAction.Zone2Servo3EndFw },
            { Bit.ZONE2_SERVO_4_FW_END, ActutorAction.Zone2Servo4EndFw },
            // ZONE 2 - Servo 3&4- BW End
            { Bit.ZONE2_SERVO_3_BW_END, ActutorAction.Zone2Servo3EndBw },
            { Bit.ZONE2_SERVO_4_BW_END, ActutorAction.Zone2Servo4EndBw },

             // ZONE 2 - Servo 5&6- FW Start
            { Bit.ZONE2_SERVO_5_FW_START, ActutorAction.Zone2Servo5StartFw },
            { Bit.ZONE2_SERVO_6_FW_START, ActutorAction.Zone2Servo6StartFw },
            // ZONE 2 - Servo 5&6- BW Start
            { Bit.ZONE2_SERVO_5_BW_START, ActutorAction.Zone2Servo5StartBw },
            { Bit.ZONE2_SERVO_6_BW_START, ActutorAction.Zone2Servo6StartBw },
             // ZONE 2 - Servo 5&6- FW End
            { Bit.ZONE2_SERVO_5_FW_END, ActutorAction.Zone2Servo5EndFw },
            { Bit.ZONE2_SERVO_6_FW_END, ActutorAction.Zone2Servo6EndFw },
            // ZONE 2 - Servo 5&6- BW End
            { Bit.ZONE2_SERVO_5_BW_END, ActutorAction.Zone2Servo5EndBw },
            { Bit.ZONE2_SERVO_6_BW_END, ActutorAction.Zone2Servo6EndBw },
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

            { ActutorAction.Zone1Servo1EndFw, async () => await HandleServoEnd(1,1,"FW") },
            { ActutorAction.Zone1Servo1EndBw, async () => await HandleServoEnd(1,1, "BW") },
            { ActutorAction.Zone1Servo2EndFw, async () => await HandleServoEnd(1,2,"FW") },
            { ActutorAction.Zone1Servo2EndBw, async () => await HandleServoEnd(1,2,"BW") },

            { ActutorAction.Zone1Servo3StartFw, async () => await HandleServoStart(1,3,"FW") },
            { ActutorAction.Zone1Servo3StartBw, async () => await HandleServoStart(1,3, "BW") },
            { ActutorAction.Zone1Servo4StartFw, async () => await HandleServoStart(1,4,"FW") },
            { ActutorAction.Zone1Servo4StartBw, async () => await HandleServoStart(1,4,"BW") },

            { ActutorAction.Zone1Servo3EndFw, async () => await HandleServoEnd(1,3,"FW") },
            { ActutorAction.Zone1Servo3EndBw, async () => await HandleServoEnd(1,3, "BW") },
            { ActutorAction.Zone1Servo4EndFw, async () => await HandleServoEnd(1,4,"FW") },
            { ActutorAction.Zone1Servo4EndBw, async () => await HandleServoEnd(1,4,"BW") },

            { ActutorAction.Zone1Servo5StartFw, async () => await HandleServoStart(1,6,"FW") },
            { ActutorAction.Zone1Servo5StartBw, async () => await HandleServoStart(1,6, "BW") },
            { ActutorAction.Zone1Servo6StartFw, async () => await HandleServoStart(1,6,"FW") },
            { ActutorAction.Zone1Servo6StartBw, async () => await HandleServoStart(1,6,"BW") },

            { ActutorAction.Zone1Servo5EndFw, async () => await HandleServoEnd(1,6,"FW") },
            { ActutorAction.Zone1Servo5EndBw, async () => await HandleServoEnd(1,6, "BW") },
            { ActutorAction.Zone1Servo6EndFw, async () => await HandleServoEnd(1,6,"FW") },
            { ActutorAction.Zone1Servo6EndBw, async () => await HandleServoEnd(1,6,"BW") },
            //
            { ActutorAction.Zone2Servo1StartFw, async () => await HandleServoStart(2, 1, "FW") },
            { ActutorAction.Zone2Servo1StartBw, async () => await HandleServoStart(2, 1, "BW") },
            { ActutorAction.Zone2Servo2StartFw, async () => await HandleServoStart(2, 2, "FW") },
            { ActutorAction.Zone2Servo2StartBw, async () => await HandleServoStart(2, 2, "BW") },

            { ActutorAction.Zone2Servo1EndFw, async () => await HandleServoEnd(2, 1, "FW") },
            { ActutorAction.Zone2Servo1EndBw, async () => await HandleServoEnd(2, 1, "BW") },
            { ActutorAction.Zone2Servo2EndFw, async () => await HandleServoEnd(2, 2, "FW") },
            { ActutorAction.Zone2Servo2EndBw, async () => await HandleServoEnd(2, 2, "BW") },

             { ActutorAction.Zone2Servo3StartFw, async () => await HandleServoStart(2,3,"FW") },
            { ActutorAction.Zone2Servo3StartBw, async () => await HandleServoStart(2,3, "BW") },
            { ActutorAction.Zone2Servo4StartFw, async () => await HandleServoStart(2,4,"FW") },
            { ActutorAction.Zone2Servo4StartBw, async () => await HandleServoStart(2,4,"BW") },

            { ActutorAction.Zone2Servo3EndFw, async () => await HandleServoEnd(2,3,"FW") },
            { ActutorAction.Zone2Servo3EndBw, async () => await HandleServoEnd(2,3, "BW") },
            { ActutorAction.Zone2Servo4EndFw, async () => await HandleServoEnd(2,4,"FW") },
            { ActutorAction.Zone2Servo4EndBw, async () => await HandleServoEnd(2,4,"BW") },

            { ActutorAction.Zone2Servo5StartFw, async () => await HandleServoStart(2,6,"FW") },
            { ActutorAction.Zone2Servo5StartBw, async () => await HandleServoStart(2,6, "BW") },
            { ActutorAction.Zone2Servo6StartFw, async () => await HandleServoStart(2,6,"FW") },
            { ActutorAction.Zone2Servo6StartBw, async () => await HandleServoStart(2,6,"BW") },

            { ActutorAction.Zone2Servo5EndFw, async () => await HandleServoEnd(2,6,"FW") },
            { ActutorAction.Zone2Servo5EndBw, async () => await HandleServoEnd(2,6, "BW") },
            { ActutorAction.Zone2Servo6EndFw, async () => await HandleServoEnd(2,6,"FW") },
            { ActutorAction.Zone2Servo6EndBw, async () => await HandleServoEnd(2,6,"BW") },
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
                            LogStorage.Add(_controller.ListCellDatas);
                            LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][START][FW]: CellData Updated");
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
                            LogStorage.Add(_controller.ListCellDatas);
                            LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][START][BW]: CellData Updated");
                        }
                    }
                    else
                    {
                        LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][START][BW]: CellData Cannot find in List");
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
                            LogStorage.Add(_controller.ListCellDatas);
                            LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][END][FW]: CellData Updated");
                        }
                    }
                    else
                    {
                        LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][END][FW]: CellData Cannot find in List");
                    }
                    break;
                case "BW":
                    if (cellData.Count > 0)
                    {
                        foreach (var cell in cellData)
                        {
                            cell.ServoBWEndTime = DateTime.Now;
                            cell.ServoBWTaktTime = (cell.ServoBWEndTime - cell.ServoBWStartTime).TotalSeconds;
                            LogStorage.Add(_controller.ListCellDatas);
                            LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][END][BW]: CellData Updated");
                        }
                    }
                    else
                    {
                        LogTxt.Add(LogTxt.Type.FlowRun, $"[SERVO{zone}][UNIT{unit}][END][BW]: CellData Cannot find in List");
                    }
                    break;
            }
        }
    }
}
