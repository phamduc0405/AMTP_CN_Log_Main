using ACO2_App._0;
using ACO2_App._0.INIT;
using ACO2_App._0.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace MTP.Model
{
    public class RobotSequenceHandler
    {
        public enum RobotAction
        {
            RB1Tool1StartDrop, 
            RB1Tool2StartDrop,
            RB2Tool1StartDrop,
            RB2Tool2StartDrop,

            RB1Tool1StartPick,
            RB1Tool2StartPick,
            RB2Tool1StartPick,
            RB2Tool2StartPick,

            RB1Tool1Drop,
            RB1Tool2Drop,
            RB2Tool1Drop,
            RB2Tool2Drop,
            RB1Tool1Pick,
            RB1Tool2Pick,
            RB2Tool1Pick,
            RB2Tool2Pick,
        }
        private Controller _controller;
        private  Dictionary<RobotAction, Func<Task>> _handlers;
        private  RobotAction _action;
        private string _cellIDWord = "";
        private string _channelWord = "";
        private string _unitWord = "";
        private string _stageWord = "";
        private string _isNeedRetryWord = "";
        public RobotSequenceHandler(string action)
        {

            Initial(action);
            HandleAction();
        }
        private void Initial(string action)
        {
            var actionMap = new Dictionary<string, RobotAction>(StringComparer.OrdinalIgnoreCase)
        {
            { "ROBOT1_TOOL1_DROP", RobotAction.RB1Tool1Drop },
            { "ROBOT1_TOOL2_DROP", RobotAction.RB1Tool2Drop },
            { "ROBOT2_TOOL1_DROP", RobotAction.RB2Tool1Drop },
            { "ROBOT2_TOOL2_DROP", RobotAction.RB2Tool2Drop },


            { "ROBOT1_TOOL1_PICK", RobotAction.RB1Tool1Pick },
            { "ROBOT1_TOOL2_PICK", RobotAction.RB1Tool2Pick },
            { "ROBOT2_TOOL1_PICK", RobotAction.RB2Tool1Pick },
            { "ROBOT2_TOOL2_PICK", RobotAction.RB2Tool2Pick },

            { "ROBOT1_TOOL1_START_DROP", RobotAction.RB1Tool1StartDrop },
            { "ROBOT1_TOOL2_START_DROP", RobotAction.RB1Tool2StartDrop },
            { "ROBOT2_TOOL1_START_DROP", RobotAction.RB2Tool1StartDrop },
            { "ROBOT2_TOOL2_START_DROP", RobotAction.RB2Tool2StartDrop },

            { "ROBOT1_TOOL1_START_PICK", RobotAction.RB1Tool1StartPick },
            { "ROBOT1_TOOL2_START_PICK", RobotAction.RB1Tool2StartPick },
            { "ROBOT2_TOOL1_START_PICK", RobotAction.RB2Tool1StartPick },
            { "ROBOT2_TOOL2_START_PICK", RobotAction.RB2Tool2StartPick },
        };

            if (actionMap.TryGetValue(action, out RobotAction mappedAction))
            {
                _action = mappedAction;
            }
            else
            {
                LogTxt.Add(LogTxt.Type.Exception, $"[ROBOT] Unknown Action: {action}");
                return;
            }

            _handlers = new Dictionary<RobotAction, Func<Task>>
        {
            { RobotAction.RB1Tool1Drop, async () => await HandleToolDrop(1,1) },
            { RobotAction.RB1Tool2Drop, async () => await HandleToolDrop(1,2) },
            { RobotAction.RB2Tool1Drop, async () => await HandleToolDrop(2,1) },
            { RobotAction.RB2Tool2Drop, async () => await HandleToolDrop(2,2) },

             { RobotAction.RB1Tool1Pick, async () => await HandleToolPick(1,1) },
            { RobotAction.RB1Tool2Pick, async () => await HandleToolPick(1,2) },
            { RobotAction.RB2Tool1Pick, async () => await HandleToolPick(2,1) },
            { RobotAction.RB2Tool2Pick, async () => await HandleToolPick(2,2) },

              { RobotAction.RB1Tool1StartDrop, async () => await HandleToolStartDrop(1,1) },
            { RobotAction.RB1Tool2StartDrop, async () => await HandleToolStartDrop(1,2) },
            { RobotAction.RB2Tool1StartDrop, async () => await HandleToolStartDrop(2,1) },
            { RobotAction.RB2Tool2StartDrop, async () => await HandleToolStartDrop(2,2) },

              { RobotAction.RB1Tool1StartPick, async () => await HandleToolStartPick(1,1) },
            { RobotAction.RB1Tool2StartPick, async () => await HandleToolStartPick(1,2) },
            { RobotAction.RB2Tool1StartPick, async () => await HandleToolStartPick(2,1) },
            { RobotAction.RB2Tool2StartPick, async () => await HandleToolStartPick(2,2) },
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
                LogTxt.Add(LogTxt.Type.Exception, $"[ROBOT] No handler found for action {_action}");
            }
        }

        private async Task HandleToolStartDrop(int zone, int toolNumber)
        {
            switch (toolNumber)
            {
                case 1:
                    _cellIDWord = $"CELL_ID_RB{zone}_DROP_TOOL1"; _channelWord = $"CHANNEL_RB{zone}_DROP_TOOL1";
                    _unitWord = "UNIT_RB1_DROP_1"; _stageWord = "STAGE_RB1_DROP_1"; break;
                case 2:
                    _cellIDWord = $"CELL_ID_RB{zone}_DROP_TOOL2"; _channelWord = $"CHANNEL_RB{zone}_DROP_TOOL2";
                    _unitWord = $"UNIT_RB{zone}_DROP_2"; _stageWord = $"STAGE_RB{zone}_DROP_2"; break;

            }
            try
            {
                string cellIdRbDropTool = "";
                string channelRbDropTool = "";
                bool isTimeOut = false;

                (cellIdRbDropTool, channelRbDropTool, isTimeOut) = await _controller.WaitForPlcData(_cellIDWord, _channelWord);
                if (isTimeOut)
                {
                    _controller.SetSignalBitFromPC("TIME_OUT", true);
                    return;
                }               
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][DROP]:" + $"RECEIVE DATA PLC: " +
                    $"CELL_ID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                    $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}" +
                    $"UNIT:{_controller.GetWordValueFromPLC(_unitWord, true)}" +
                     $"STAGE:{_controller.GetWordValueFromPLC(_stageWord, true)}"
               );
                // Save To Log
                CellData cellData = _controller.FindCellInListTemp(cellIdRbDropTool, true);
                if (cellData != null)
                {
                   cellData.RBDropStartTime = DateTime.Now;
                }
                else
                {
                   
                    string Message = _controller.CreateLogFollowCellData(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][DROP]::CANNOT FIND CELL IN QUEUE New CellData Added:" + Message);
                }
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][DROP]:" +
                    $"UPDATE DATA IN LIST: CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                    $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}" +
                    $"UNIT:{_controller.GetWordValueFromPLC(_unitWord, true)}" +
                    $"STAGE:{_controller.GetWordValueFromPLC(_stageWord, true)}"
                    );
                string logMessage = _controller.CreateLogFollowCellData(cellData);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][DROP]: CellData Updated:" + logMessage);

            }
            catch (Exception e)
            {
                string debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, e.Message);
                LogTxt.Add(LogTxt.Type.Exception, $"[ROBOT{zone}][TOOL{toolNumber}][START][DROP]:" + debug);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][DROP]:" + debug);
            }
        }
        private async Task HandleToolDrop(int zone, int toolNumber)
        {
            switch (toolNumber)
            {
                case 1:
                    _cellIDWord = $"CELL_ID_RB{zone}_DROP_TOOL1"; _channelWord = $"CHANNEL_RB{zone}_DROP_1"; 
                    _unitWord = "UNIT_RB1_DROP_1"; _stageWord = "STAGE_RB1_DROP_1"; break;
                case 2:
                    _cellIDWord = $"CELL_ID_RB{zone}_DROP_TOOL2"; _channelWord = $"CHANNEL_RB{zone}_DROP_2";
                    _unitWord = $"UNIT_RB{zone}_DROP_2"; _stageWord = $"STAGE_RB{zone}_DROP_2"; break;

            }
            try
            {
                string cellIdRbDropTool = "";
                string channelRbDropTool = "";
                bool isTimeOut = false;

                (cellIdRbDropTool, channelRbDropTool, isTimeOut) = await _controller.WaitForPlcData(_cellIDWord, _channelWord);
                if (isTimeOut)
                {
                    _controller.SetSignalBitFromPC("TIME_OUT", true);
                    return;
                }
                string unitRbDropTool = "";
                string stageRbDropTool = "";
                bool isTimeOut1 = false;

                (unitRbDropTool, stageRbDropTool, isTimeOut1) = await _controller.WaitForPlcData(_unitWord, _stageWord);
                if (isTimeOut)
                {
                    _controller.SetSignalBitFromPC("TIME_OUT", true);
                    return;
                }
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][DROP]:" + $"RECEIVE DATA PLC: " +
                    $"CELL_ID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                    $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}" +
                    $"UNIT:{_controller.GetWordValueFromPLC(_unitWord, true)}"+
                     $"STAGE:{_controller.GetWordValueFromPLC(_stageWord, true)}"
               );
                // Save To Log
                CellData cellData = _controller.FindCellInListTemp(cellIdRbDropTool, true);
                if (cellData != null)
                {
                    cellData.RBDropEndTime = DateTime.Now;
                    cellData.RBDropTackTime = (cellData.RBDropEndTime - cellData.RBDropStartTime).TotalSeconds;
                    cellData.Channel.ChannelNo = channelRbDropTool;
                    cellData.UnitStartTime = DateTime.Now;
                    cellData.ZoneNo = zone.ToString() ;
                    if(zone == 1) { cellData.InsRobot1ToolNo = toolNumber.ToString(); }
                    if(zone == 2) { cellData.InsRobot2ToolNo = toolNumber.ToString(); }
                    cellData.Unit=unitRbDropTool;
                    cellData.Stage=stageRbDropTool;
                }
                else
                {
                    //cellData = new CellData
                    //{
                    //    CellID = cellIdRb1DropTool1,
                    //    Channel = new Channel { ChannelNo = channelRb1DropTool1 },
                    //    UnitStartTime = DateTime.Now,
                    //    ZoneNo = "1",
                    //    InsRobotToolNo = "1"
                    //};
                    //_listCellDatas.CellDatas.Add(cellData);
                    string Message = _controller.CreateLogFollowCellData(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][DROP]::CANNOT FIND CELL IN QUEUE New CellData Added:" + Message);
                }
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][DROP]:" + 
                    $"UPDATE DATA IN LIST: CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                    $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}"+
                    $"UNIT:{_controller.GetWordValueFromPLC(_unitWord, true)}" +
                    $"STAGE:{_controller.GetWordValueFromPLC(_stageWord, true)}"
                    );
                string logMessage = _controller.CreateLogFollowCellData(cellData);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][DROP]: CellData Updated:" + logMessage);

            }
            catch (Exception e)
            {
                string debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, e.Message);
                LogTxt.Add(LogTxt.Type.Exception, $"[ROBOT{zone}][TOOL{toolNumber}][DROP]:" + debug);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][DROP]:" + debug);
            }
        }

        private async Task HandleToolStartPick(int zone, int toolNumber)
        {
            switch (toolNumber)
            {
                case 1:
                    _cellIDWord = $"CELL_ID_RB{zone}_PICK_TOOL1"; _channelWord = $"CHANNEL_RB{zone}_PICK_TOOL1";
                    _unitWord = $"UNIT_RB{zone}_PICK_1"; _stageWord = $"STAGE_RB{zone}_PICK_1";
                    _isNeedRetryWord = $"ISNEEDRETRY_RB{zone}_PICK1"; break;
                case 2:
                    _cellIDWord = $"CELL_ID_RB{zone}_PICK_TOOL2"; _channelWord = $"CHANNEL_RB{zone}_PICK_TOOL2";
                    _unitWord = $"UNIT_RB{zone}_PICK_2"; _stageWord = $"STAGE_RB{zone}_PICK_2";
                    _isNeedRetryWord = $"ISNEEDRETRY_RB{zone}_PICK2"; break;

            }
            try
            {
                string cellIdRbPickTool = "";
                string channelRbPickTool = "";
                bool isTimeOut = false;
                (cellIdRbPickTool, channelRbPickTool, isTimeOut) = await _controller.WaitForPlcData(_cellIDWord, _channelWord);
                if (isTimeOut)
                {
                    _controller.SetSignalBitFromPC("TIME_OUT", true);
                    return;
                }
                string isNeedRetry = "";
                isNeedRetry = _controller.GetWordValueFromPLC(_isNeedRetryWord, true);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][PICK]:" + $"RECEIVE DATA PLC: " +
                    $"CELL_ID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                    $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}" +
                    $"ISNEEDRETRY:{_controller.GetWordValueFromPLC(_isNeedRetryWord, true)}"
                    );

                // Save To Log
                CellData cellData = _controller.FindCellInListTemp(cellIdRbPickTool, false, true, channelRbPickTool);
                if (cellData != null)
                {
                   cellData.RBPickStartTime = DateTime.Now;
                }
                else
                {
                    string m = _controller.CreateLogFollowCellData(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][PICK]:CANNOT FIND CELL IN QUEUE  CellData:" + m);
                }
            }
            catch (Exception e)
            {
                string debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, e.Message);
                LogTxt.Add(LogTxt.Type.Exception, $"[ROBOT{zone}][TOOL{toolNumber}][START][PICK]:" + debug);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][PICK]:" + debug);
            }
        }
        private async Task HandleToolPick(int zone, int toolNumber)
        {
            switch (toolNumber)
            {
                case 1:
                    _cellIDWord = $"CELL_ID_RB{zone}_PICK_TOOL1"; _channelWord = $"CHANNEL_RB{zone}_PICK_1";
                    _unitWord = $"UNIT_RB{zone}_PICK_1"; _stageWord = $"STAGE_RB{zone}_PICK_1";
                    _isNeedRetryWord = $"ISNEEDRETRY_RB{zone}_PICK1"; break;
                case 2:
                    _cellIDWord = $"CELL_ID_RB{zone}_PICK_TOOL2"; _channelWord = $"CHANNEL_RB{zone}_PICK_2";
                    _unitWord = $"UNIT_RB{zone}_PICK_2"; _stageWord = $"STAGE_RB{zone}_PICK_2";
                    _isNeedRetryWord = $"ISNEEDRETRY_RB{zone}_PICK2"; break;

            }
            try
            {
                string cellIdRbPickTool = "";
                string channelRbPickTool = "";
                bool isTimeOut = false;
                (cellIdRbPickTool, channelRbPickTool, isTimeOut) = await _controller.WaitForPlcData(_cellIDWord, _channelWord);
                if (isTimeOut)
                {
                    _controller.SetSignalBitFromPC("TIME_OUT", true);
                    return;
                }
                string unitRbPickTool = "";
                string stageRbPickTool = "";
                bool isTimeOut1 = false;
                (unitRbPickTool, stageRbPickTool, isTimeOut1) = await _controller.WaitForPlcData(_unitWord, _stageWord);
                if (isTimeOut1)
                {
                    _controller.SetSignalBitFromPC("TIME_OUT", true);
                    return;
                }
                string isNeedRetry = "";
                isNeedRetry = _controller.GetWordValueFromPLC(_isNeedRetryWord, true);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][PICK]:" + $"RECEIVE DATA PLC: " +
                    $"CELL_ID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                    $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}"+
                    $"UNIT:{_controller.GetWordValueFromPLC(_unitWord, true)}"+
                    $"STAGE:{_controller.GetWordValueFromPLC(_stageWord, true)}" +
                    $"ISNEEDRETRY:{_controller.GetWordValueFromPLC(_isNeedRetryWord, true)}" 
                    );

                // Save To Log
                CellData cellData = _controller.FindCellInListTemp(cellIdRbPickTool, false, true, channelRbPickTool);
                if (cellData != null)
                {
                    cellData.RBPickEndTime = DateTime.Now;
                    cellData.RBPickTackTime = (cellData.RBPickEndTime - cellData.RBPickStartTime).TotalSeconds;
                    var cell = _controller.Equipment[zone-1].Channels.FirstOrDefault(channel => channel.CellID == cellIdRbPickTool);
                    if (cell == null)
                    {
                        if (zone == 1) { cellData.InsRobot1ToolNo = toolNumber.ToString(); }
                        if (zone == 2) { cellData.InsRobot2ToolNo = toolNumber.ToString(); }
                        
                        cellData.Unit = unitRbPickTool;
                        cellData.Stage = stageRbPickTool;
                        cellData.ZoneNo = zone.ToString();
                        cellData.Channel = cell;
                        cellData.Channel.ChannelNo = channelRbPickTool; 
                        cellData.UnitEndTime = DateTime.Now;
                        cellData.UnitTackTime = (cellData.UnitEndTime - cellData.UnitStartTime).TotalSeconds;
                        LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][PICK]:" + $"UPDATE DATA IN LIST: " +
                            $"CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                            $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}" +
                            $"UNIT:{_controller.GetWordValueFromPLC(_unitWord, true)}" +
                            $"STAGE:{_controller.GetWordValueFromPLC(_stageWord, true)}" +
                            $"ISNEEDRETRY:{_controller.GetWordValueFromPLC(_isNeedRetryWord, true)}");
                        string logMessage = _controller.CreateLogFollowCellData(cellData);
                        LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][PICK]:  CellData Updated:" + logMessage);


                        if(isNeedRetry == "0") { } // no need retry
                        if(isNeedRetry == "1") // need retry
                        {
                            cellData.MCEndTime = DateTime.Now;
                            cellData.MCTackTime = (cellData.MCEndTime - cellData.MCStartTime).TotalSeconds;
                           await _controller.SaveDataLog(cellData);
                            cellData.Channel.Clear();
                            string log1Message = _controller.CreateLogFollowCellData(cellData);
                            LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][PICK][NEED RETRY]:Save Datalog cell need Retry" + logMessage);
                        }
                    }
                    else
                    {
                        string m = _controller.CreateLogFollowCellData(cellData);
                        LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][PICK]:CANNOT FIND IN EQUIP Channel have same cellID&Channel with Queue  CellData:" + m);
                    }
                }
                else
                {
                    string m = _controller.CreateLogFollowCellData(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][PICK]:CANNOT FIND CELL IN QUEUE  CellData:" + m);
                }



            }
            catch (Exception e)
            {
                string debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, e.Message);
                LogTxt.Add(LogTxt.Type.Exception, $"[ROBOT{zone}][TOOL{toolNumber}][PICK]:" + debug);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][PICK]:" + debug);
            }
        }
    }

}
