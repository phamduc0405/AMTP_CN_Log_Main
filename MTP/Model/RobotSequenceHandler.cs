using ACO2_App._0;
using ACO2_App._0.INIT;
using ACO2_App._0.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MTP.Model
{
    public class RobotSequenceHandler
    {
        public enum RobotAction
        {
            Rb1Tool1StartPut,
            Rb1Tool2StartPut,
            Rb1Tool1EndPut,
            Rb1Tool2EndPut,

            Rb1Tool1StartGet,
            Rb1Tool2StartGet,
            Rb1Tool1EndGet,
            Rb1Tool2EndGet,

            Rb2Tool1StartPut,
            Rb2Tool2StartPut,
            Rb2Tool1EndPut,
            Rb2Tool2EndPut,

            Rb2Tool1StartGet,
            Rb2Tool2StartGet,
            Rb2Tool1EndGet,
            Rb2Tool2EndGet,
        }
        private Controller _controller;
        private  Dictionary<RobotAction, Func<Task>> _handlers;
        private  RobotAction _action;
        private string _cellIDWord = "";
        private string _channelWord = "";
        private string _unitWord = "";
        private string _stageWord = "";
        private string _retryWord = "";
        private string _recheckedWord = "";
        private string _abRuleWord = "";
        private string _temperatureWord = "";

        public RobotSequenceHandler(string action)
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            _controller = MainWindow.Controller;
            Initial(action);
            HandleAction();
        }
        private void Initial(string action)
        {
            var actionMap = new Dictionary<string, RobotAction>(StringComparer.OrdinalIgnoreCase)
            {
                { Bit.ROBOT1_1_START_PUT, RobotAction.Rb1Tool1StartPut },
                { Bit.ROBOT1_2_START_PUT, RobotAction.Rb1Tool2StartPut },
                { Bit.ROBOT1_1_END_PUT, RobotAction.Rb1Tool1EndPut },
                { Bit.ROBOT1_2_END_PUT, RobotAction.Rb1Tool2EndPut },

                { Bit.ROBOT1_1_START_GET, RobotAction.Rb1Tool1StartGet },
                { Bit.ROBOT1_2_START_GET, RobotAction.Rb1Tool2StartGet },
                { Bit.ROBOT1_1_END_GET, RobotAction.Rb1Tool1EndGet },
                { Bit.ROBOT1_2_END_GET, RobotAction.Rb1Tool2EndGet },

                { Bit.ROBOT2_1_START_PUT, RobotAction.Rb2Tool1StartPut },
                { Bit.ROBOT2_2_START_PUT, RobotAction.Rb2Tool2StartPut },
                { Bit.ROBOT2_1_END_PUT, RobotAction.Rb2Tool1EndPut },
                { Bit.ROBOT2_2_END_PUT, RobotAction.Rb2Tool2EndPut },

                { Bit.ROBOT2_1_START_GET, RobotAction.Rb2Tool1StartGet },
                { Bit.ROBOT2_2_START_GET, RobotAction.Rb2Tool2StartGet },
                { Bit.ROBOT2_1_END_GET, RobotAction.Rb2Tool1EndGet },
                { Bit.ROBOT2_2_END_GET, RobotAction.Rb2Tool2EndGet },
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
                { RobotAction.Rb1Tool1StartPut, async () => await HandleToolStartPut(1, 1) },
                { RobotAction.Rb1Tool2StartPut, async () => await HandleToolStartPut(1, 2) },
                { RobotAction.Rb1Tool1EndPut, async () => await HandleToolEndPut(1, 1) },
                { RobotAction.Rb1Tool2EndPut, async () => await HandleToolEndPut(1, 2) },

                { RobotAction.Rb1Tool1StartGet, async () => await HandleToolStartGet(1, 1) },
                { RobotAction.Rb1Tool2StartGet, async () => await HandleToolStartGet(1, 2) },
                { RobotAction.Rb1Tool1EndGet, async () => await HandleToolEndGet(1, 1) },
                { RobotAction.Rb1Tool2EndGet, async () => await HandleToolEndGet(1, 2) },

                { RobotAction.Rb2Tool1StartPut, async () => await HandleToolStartPut(2, 1) },
                { RobotAction.Rb2Tool2StartPut, async () => await HandleToolStartPut(2, 2) },
                { RobotAction.Rb2Tool1EndPut, async () => await HandleToolEndPut(2, 1) },
                { RobotAction.Rb2Tool2EndPut, async () => await HandleToolEndPut(2, 2) },

                { RobotAction.Rb2Tool1StartGet, async () => await HandleToolStartGet(2, 1) },
                { RobotAction.Rb2Tool2StartGet, async () => await HandleToolStartGet(2, 2) },
                { RobotAction.Rb2Tool1EndGet, async () => await HandleToolEndGet(2, 1) },
                { RobotAction.Rb2Tool2EndGet, async () => await HandleToolEndGet(2, 2) },
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
        private string ScaleValueStage(int zone,string unitvalue , string stagevalue,string channelvalue)
        {
            int unit = int.Parse(unitvalue);
            int stage = int.Parse(stagevalue);
            int channel = int.Parse(channelvalue);
            if(zone == 2)
            {
                stage = channel + 12;
            }
            return stage.ToString();
        }
        private async Task HandleToolStartPut(int zone, int toolNumber)
        {
            switch (zone)
            {
                case 1:
                    switch (toolNumber)
                    {
                        case 1:
                            _cellIDWord = Word.ROBOT1_1_CELLID; _channelWord = Word.ROBOT1_1_CHANNEL;
                            _unitWord = Word.ROBOT1_1_UNIT; _stageWord = Word.ROBOT1_1_STAGE; _temperatureWord = Word.ROBOT1_1_TEMPERATURE; break;
                        case 2:
                            _cellIDWord = Word.ROBOT1_2_CELLID; _channelWord = Word.ROBOT1_2_CHANNEL;
                            _unitWord = Word.ROBOT1_2_UNIT; _stageWord = Word.ROBOT1_2_STAGE; _temperatureWord = Word.ROBOT1_2_TEMPERATURE; break;

                    }
                    break;
                case 2:
                    switch (toolNumber)
                    {
                        case 1:
                            _cellIDWord = Word.ROBOT2_1_CELLID; _channelWord = Word.ROBOT2_1_CHANNEL;
                            _unitWord = Word.ROBOT2_1_UNIT; _stageWord = Word.ROBOT2_1_STAGE; _temperatureWord = Word.ROBOT2_1_TEMPERATURE; break;
                        case 2:
                            _cellIDWord = Word.ROBOT2_2_CELLID; _channelWord = Word.ROBOT2_2_CHANNEL;
                            _unitWord = Word.ROBOT2_2_UNIT; _stageWord = Word.ROBOT2_2_STAGE; _temperatureWord = Word.ROBOT2_2_TEMPERATURE; break;

                    }
                    break;

            }
            try
            {
                string cellIdRbDropTool = "";
                string channelRbDropTool = "";
                bool isTimeOut = false;

                (cellIdRbDropTool, channelRbDropTool, isTimeOut) = await _controller.WaitForPlcData(_cellIDWord, _channelWord);
                //if (isTimeOut)
                //{
                //    _controller.SetSignalBitFromPC("TIME_OUT", true);
                //    return;
                //}
                string unitRbDropTool = "";
                string stageRbDropTool = "";
                bool isTimeOut1 = false;

                (unitRbDropTool, stageRbDropTool, isTimeOut1) = await _controller.WaitForPlcData(_unitWord, _stageWord);
                //if (isTimeOut)
                //{
                //    _controller.SetSignalBitFromPC("TIME_OUT", true);
                //    return;
                //}
                string temperature = _controller.GetWordValueFromPLC(_temperatureWord, true);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][PUT]:" + $"RECEIVE DATA PLC: " +
                    $"CELL_ID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                    $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}" +
                    $"UNIT:{_controller.GetWordValueFromPLC(_unitWord, true)}" +
                     $"STAGE:{_controller.GetWordValueFromPLC(_stageWord, true)}" +
                     $"TEMPERATURE:{_controller.GetWordValueFromPLC(_temperatureWord, true)}"
               );
                string channel = "";
                if (int.Parse(channelRbDropTool) < 10)
                {
                    channel = $"CH0{channelRbDropTool}";
                }
                else
                {
                    channel = $"CH{channelRbDropTool}";
                }
                if (!string.IsNullOrEmpty(temperature))
                {
                    temperature = String.Format("{0:f}", float.Parse(temperature) / 10);
                }
                // Save To Log
                CellData cellData = _controller.FindCellInListTemp(cellIdRbDropTool, true);
                if (cellData != null)
                {
                    cellData.Temperater = temperature;
                   cellData.RBDropStartTime = DateTime.Now;
                    cellData.Channel.ChannelNo = channel;
                    cellData.UnitStartTime = DateTime.Now;
                    cellData.ZoneNo = zone.ToString();
                    if (zone == 1) { cellData.InsRobot1ToolNo = toolNumber.ToString(); }
                    if (zone == 2) { cellData.InsRobot2ToolNo = toolNumber.ToString(); }
                    cellData.Unit = unitRbDropTool;
                    cellData.Stage = ScaleValueStage(zone, unitRbDropTool, stageRbDropTool, channelRbDropTool);
                    LogStorage.Add(_controller.ListCellDatas);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][PUT]:" +
                   $"UPDATE DATA IN LIST: CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                   $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}" +
                   $"UNIT:{_controller.GetWordValueFromPLC(_unitWord, true)}" +
                   $"STAGE:{_controller.GetWordValueFromPLC(_stageWord, true)}"
                   );
                    string logMessage = _controller.CreateLogFollowCellData(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][PUT]: CellData Updated:" + logMessage);
                }
                else
                {
                   
                    string Message = _controller.CreateLogFollowCellData(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][PUT]::CANNOT FIND CELL IN QUEUE New CellData Added:" + Message);
                }
               

            }
            catch (Exception e)
            {
                string debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, e.Message);
                LogTxt.Add(LogTxt.Type.Exception, $"[ROBOT{zone}][TOOL{toolNumber}][START][PUT]:" + debug);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][PUT]:" + debug);
            }
        }
        private async Task HandleToolEndPut(int zone, int toolNumber)
        {
            switch (zone)
            {
                case 1:
                    switch (toolNumber)
                    {
                        case 1:
                            _cellIDWord = Word.ROBOT1_1_CELLID; _channelWord = Word.ROBOT1_1_CHANNEL;
                            _unitWord = Word.ROBOT1_1_UNIT; _stageWord = Word.ROBOT1_1_STAGE; 
                           
                            break;
                        case 2:
                            _cellIDWord = Word.ROBOT1_2_CELLID; _channelWord = Word.ROBOT1_2_CHANNEL;
                            _unitWord = Word.ROBOT1_2_UNIT; _stageWord = Word.ROBOT1_2_STAGE; 
                          
                            break;

                    }
                    break;
                case 2:
                    switch (toolNumber)
                    {
                        case 1:
                            _cellIDWord = Word.ROBOT2_1_CELLID; _channelWord = Word.ROBOT2_1_CHANNEL;
                            _unitWord = Word.ROBOT2_1_UNIT; _stageWord = Word.ROBOT2_1_STAGE; break;
                        case 2:
                            _cellIDWord = Word.ROBOT2_2_CELLID; _channelWord = Word.ROBOT2_2_CHANNEL;
                            _unitWord = Word.ROBOT2_2_UNIT; _stageWord = Word.ROBOT2_2_STAGE; break;

                    }
                    break;

            }
            try
            {
                string cellIdRbDropTool = "";
                string channelRbDropTool = "";
                bool isTimeOut = false;

                (cellIdRbDropTool, channelRbDropTool, isTimeOut) = await _controller.WaitForPlcData(_cellIDWord, _channelWord);
                //if (isTimeOut)
                //{
                //    _controller.SetSignalBitFromPC("TIME_OUT", true);
                //    return;
                //}
              
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][END][PUT]:" + $"RECEIVE DATA PLC: " +
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
                    LogStorage.Add(_controller.ListCellDatas);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][END][PUT]:" +
                $"UPDATE DATA IN LIST: CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}" +
                $"UNIT:{_controller.GetWordValueFromPLC(_unitWord, true)}" +
                $"STAGE:{_controller.GetWordValueFromPLC(_stageWord, true)}"
                );
                    string logMessage = _controller.CreateLogFollowCellData(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][END][PUT]: CellData Updated:" + logMessage);
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
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][END][PUT]::CANNOT FIND CELL IN QUEUE New CellData Added:" + Message);
                }
            

            }
            catch (Exception e)
            {
                string debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, e.Message);
                LogTxt.Add(LogTxt.Type.Exception, $"[ROBOT{zone}][TOOL{toolNumber}][END][PUT]:" + debug);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][END][PUT]:" + debug);
            }
        }

        private async Task HandleToolStartGet(int zone, int toolNumber)
        {
            switch (zone)
            {
                case 1:
                    switch (toolNumber)
                    {
                        case 1:
                            _cellIDWord = Word.ROBOT1_1_CELLID; _channelWord = Word.ROBOT1_1_CHANNEL;
                            _unitWord = Word.ROBOT1_1_UNIT; _stageWord = Word.ROBOT1_1_STAGE;
                            _abRuleWord = Word.ROBOT1_1_ABRULE; _recheckedWord = Word.ROBOT1_1_RECHECKED; _retryWord = Word.ROBOT1_1_RETRY;
                            break;
                        case 2:
                            _cellIDWord = Word.ROBOT1_2_CELLID; _channelWord = Word.ROBOT1_2_CHANNEL;
                            _unitWord = Word.ROBOT1_2_UNIT; _stageWord = Word.ROBOT1_2_STAGE;
                            _abRuleWord = Word.ROBOT1_2_ABRULE; _recheckedWord = Word.ROBOT1_2_RECHECKED; _retryWord = Word.ROBOT1_2_RETRY;
                            break;
                    }
                    break;
                case 2:
                    switch (toolNumber)
                    {
                        case 1:
                            _cellIDWord = Word.ROBOT2_1_CELLID; _channelWord = Word.ROBOT2_1_CHANNEL;
                            _unitWord = Word.ROBOT2_1_UNIT; _stageWord = Word.ROBOT2_1_STAGE;
                            _abRuleWord = Word.ROBOT2_1_ABRULE; _recheckedWord = Word.ROBOT2_1_RECHECKED; _retryWord = Word.ROBOT2_1_RETRY;
                            break;
                        case 2:
                            _cellIDWord = Word.ROBOT2_2_CELLID; _channelWord = Word.ROBOT2_2_CHANNEL;
                            _unitWord = Word.ROBOT2_2_UNIT; _stageWord = Word.ROBOT2_2_STAGE;
                            _abRuleWord = Word.ROBOT2_2_ABRULE; _recheckedWord = Word.ROBOT2_2_RECHECKED; _retryWord = Word.ROBOT2_2_RETRY;
                            break;

                    }
                    break;

            }
            try
            {
                string cellIdRbPickTool = "";
                string channelRbPickTool = "";
                bool isTimeOut = false;
                (cellIdRbPickTool, channelRbPickTool, isTimeOut) = await _controller.WaitForPlcData(_cellIDWord, _channelWord);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][GET]:" + $"RECEIVE DATA PLC: " +
                    $"CELL_ID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                    $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}"
                    );
                string channel = "";
                if (int.Parse(channelRbPickTool) < 10)
                {
                    channel = $"CH0{channelRbPickTool}";
                }
                else
                {
                    channel = $"CH{channelRbPickTool}";
                }
                
                // Save To Log
                CellData cellData = _controller.FindCellInListTemp(cellIdRbPickTool, false, true, channelRbPickTool);
                if (cellData != null)
                {
                   cellData.RBPickStartTime = DateTime.Now;
                    LogStorage.Add(_controller.ListCellDatas);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][GET]:" +
               $"UPDATE DATA IN LIST: CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
               $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}" +
               $"UNIT:{_controller.GetWordValueFromPLC(_unitWord, true)}" +
               $"STAGE:{_controller.GetWordValueFromPLC(_stageWord, true)}"
               );
                    string logMessage = _controller.CreateLogFollowCellData(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][GET]: CellData Updated:" + logMessage);
                }
                else
                {
                    string m = _controller.CreateLogFollowCellData(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][GET]:CANNOT FIND CELL IN QUEUE  CellData:" + m);
                }
            }
            catch (Exception e)
            {
                string debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, e.Message);
                LogTxt.Add(LogTxt.Type.Exception, $"[ROBOT{zone}][TOOL{toolNumber}][START][GET]:" + debug);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][START][GET]:" + debug);
            }
        }
        private async Task HandleToolEndGet(int zone, int toolNumber)
        {
            switch (zone)
            {
                case 1:
                    switch (toolNumber)
                    {
                        case 1:
                            _cellIDWord = Word.ROBOT1_1_CELLID; _channelWord = Word.ROBOT1_1_CHANNEL;
                            _unitWord = Word.ROBOT1_1_UNIT; _stageWord = Word.ROBOT1_1_STAGE;
                            _abRuleWord = Word.ROBOT1_1_ABRULE; _recheckedWord = Word.ROBOT1_1_RECHECKED; _retryWord = Word.ROBOT1_1_RETRY;
                            break;
                        case 2:
                            _cellIDWord = Word.ROBOT1_2_CELLID; _channelWord = Word.ROBOT1_2_CHANNEL;
                            _unitWord = Word.ROBOT1_2_UNIT; _stageWord = Word.ROBOT1_2_STAGE;
                            _abRuleWord = Word.ROBOT1_2_ABRULE; _recheckedWord = Word.ROBOT1_2_RECHECKED; _retryWord = Word.ROBOT1_2_RETRY;
                            break;
                    }
                    break;
                case 2:
                    switch (toolNumber)
                    {
                        case 1:
                            _cellIDWord = Word.ROBOT2_1_CELLID; _channelWord = Word.ROBOT2_1_CHANNEL;
                            _unitWord = Word.ROBOT2_1_UNIT; _stageWord = Word.ROBOT2_1_STAGE;
                            _abRuleWord = Word.ROBOT2_1_ABRULE; _recheckedWord = Word.ROBOT2_1_RECHECKED; _retryWord = Word.ROBOT2_1_RETRY;
                            break;
                        case 2:
                            _cellIDWord = Word.ROBOT2_2_CELLID; _channelWord = Word.ROBOT2_2_CHANNEL;
                            _unitWord = Word.ROBOT2_2_UNIT; _stageWord = Word.ROBOT2_2_STAGE;
                            _abRuleWord = Word.ROBOT2_2_ABRULE; _recheckedWord = Word.ROBOT2_2_RECHECKED; _retryWord = Word.ROBOT2_2_RETRY;
                            break;

                    }
                    break;

            }
            try
            {
                string cellIdRbPickTool = "";
                string channelRbPickTool = "";
                bool isTimeOut = false;
                (cellIdRbPickTool, channelRbPickTool, isTimeOut) = await _controller.WaitForPlcData(_cellIDWord, _channelWord);
                //if (isTimeOut)
                //{
                //    _controller.SetSignalBitFromPC("TIME_OUT", true);
                //    return;
                //}
                string unitRbPickTool = "";
                string stageRbPickTool = "";
                bool isTimeOut1 = false;
                (unitRbPickTool, stageRbPickTool, isTimeOut1) = await _controller.WaitForPlcData(_unitWord, _stageWord);
                //if (isTimeOut1)
                //{
                //    _controller.SetSignalBitFromPC("TIME_OUT", true);
                //    return;
                //}
                string retry = "";
                string rechecked = "";
                string abRule = "";
                bool isTimeOut2 = false;

                (retry, rechecked, isTimeOut2) = await _controller.WaitForPlcData(_retryWord, _recheckedWord);
                //if (isTimeOut)
                //{
                //    _controller.SetSignalBitFromPC("TIME_OUT", true);
                //    return;
                //}
                abRule = _controller.GetWordValueFromPLC(_abRuleWord, true);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][END][GET]:" + $"RECEIVE DATA PLC: " +
                    $"CELL_ID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                    $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}" +
                    $"UNIT:{_controller.GetWordValueFromPLC(_unitWord, true)}" +
                    $"STAGE:{_controller.GetWordValueFromPLC(_stageWord, true)}" +
                      $"RETRY:{_controller.GetWordValueFromPLC(_retryWord, true)}" +
                    $"RECHECKED:{_controller.GetWordValueFromPLC(_recheckedWord, true)}" +
                     $"ABRULE:{_controller.GetWordValueFromPLC(_abRuleWord, true)}"
                    );
                string channel = "";
                if (int.Parse(channelRbPickTool) < 10)
                {
                    channel = $"CH0{channelRbPickTool}";
                }
                else
                {
                    channel = $"CH{channelRbPickTool}";
                }
                // Save To Log
                CellData cellData = _controller.FindCellInListTemp(cellIdRbPickTool, false, true, channelRbPickTool);
              
                if (cellData != null)
                {
                    cellData.RBPickEndTime = DateTime.Now;
                    cellData.RBPickTackTime = (cellData.RBPickEndTime - cellData.RBPickStartTime).TotalSeconds;
                    LogStorage.Add(_controller.ListCellDatas);
                    var cell = _controller.Equipment[zone-1].Channels.FirstOrDefault(ch => ch.ChannelNo == channel);
                    if (cell != null)
                    {
                        if (zone == 1) { cellData.InsRobot1ToolNo = toolNumber.ToString(); }
                        if (zone == 2) { cellData.InsRobot2ToolNo = toolNumber.ToString(); }
                        
                        cellData.Unit = unitRbPickTool;
                        cellData.Stage = ScaleValueStage(zone,unitRbPickTool,stageRbPickTool,channelRbPickTool);
                        cellData.ZoneNo = zone.ToString();
                        cellData.Channel = cell;
                        cellData.UnitEndTime = DateTime.Now;
                        cellData.UnitTackTime = (cellData.UnitEndTime - cellData.UnitStartTime).TotalSeconds;
                        LogStorage.Add(_controller.ListCellDatas);
                        LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][END][GET]:" + $"UPDATE DATA IN LIST: " +
                            $"CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                            $"CHANNEL:{_controller.GetWordValueFromPLC(_channelWord, true)}" +
                            $"UNIT:{_controller.GetWordValueFromPLC(_unitWord, true)}" +
                            $"STAGE:{_controller.GetWordValueFromPLC(_stageWord, true)}" +
                             $"RETRY:{_controller.GetWordValueFromPLC(_retryWord, true)}" +
                    $"RECHECKED:{_controller.GetWordValueFromPLC(_recheckedWord, true)}" +
                     $"ABRULE:{_controller.GetWordValueFromPLC(_abRuleWord, true)}");
                        string logMessage = _controller.CreateLogFollowCellData(cellData);
                        LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][END][GET]:  CellData Updated:" + logMessage);


                        if(retry == "0") { cellData.Retry = retry; cellData.Rechecked = rechecked;cellData.ABRule = abRule; } // no need retry
                        if(retry == "1") // need retry
                        {
                            cellData.Retry = retry; cellData.Rechecked = rechecked; cellData.ABRule = abRule;
                            cellData.MCEndTime = DateTime.Now;
                            cellData.MCTackTime = (cellData.MCEndTime - cellData.MCStartTime).TotalSeconds;
                           await _controller.SaveDataLog(cellData);
                            cellData.Channel.Clear();
                            LogStorage.Add(_controller.ListCellDatas);
                            string log1Message = _controller.CreateLogFollowCellData(cellData);
                            LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][END][GET][NEED RETRY]:Save Datalog cell need Retry" + logMessage);
                        }
                    }
                    else
                    {
                        string m = _controller.CreateLogFollowCellData(cellData);
                        LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][END][GET]:CANNOT FIND IN EQUIP Channel have same cellID&Channel with Queue  CellData:" + m);
                    }
                }
                else
                {
                    string m = _controller.CreateLogFollowCellData(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][END][GET]:CANNOT FIND CELL IN QUEUE  CellData:" + m);
                }



            }
            catch (Exception e)
            {
                string debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, e.Message);
                LogTxt.Add(LogTxt.Type.Exception, $"[ROBOT{zone}][TOOL{toolNumber}][END][GET]:" + debug);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[ROBOT{zone}][TOOL{toolNumber}][END][GET]:" + debug);
            }
        }
    }

}
