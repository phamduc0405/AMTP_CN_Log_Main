using ACO2_App._0;
using ACO2_App._0.INIT;
using ACO2_App._0.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MTP.Model
{
    public class TrackOutSequenceHandler
    {
        public enum TrackOutAction
        {
            Rb1Tool1,
            Rb1Tool2,
            Rb2Tool1,
            Rb2Tool2,
        }
        private Controller _controller;

        private Dictionary<TrackOutAction, Func<Task>> _handlers;
        private TrackOutAction _action;
        private string _cellIDWord = "";
        private string _resultTrackOutWord = "";
        public TrackOutSequenceHandler(string action)
        {
            _controller = MainWindow.Controller;
            Initial(action);
            HandleAction();
        }

        private void Initial(string action)
        {
            var actionMap = new Dictionary<string, TrackOutAction>(StringComparer.OrdinalIgnoreCase)
        {
            { Bit.ROBOT1_1_TRACKOUT, TrackOutAction .Rb1Tool1 },
            { Bit.ROBOT1_2_TRACKOUT, TrackOutAction .Rb1Tool2 },
            { Bit.ROBOT2_1_TRACKOUT, TrackOutAction .Rb2Tool1 },
            { Bit.ROBOT2_2_TRACKOUT, TrackOutAction .Rb2Tool2 }
        };

            if (actionMap.TryGetValue(action, out TrackOutAction mappedAction))
            {
                _action = mappedAction;
            }
            else
            {
                LogTxt.Add(LogTxt.Type.Exception, $"[TRACKOUT] Unknown Action: {action}");
                return;
            }


            _handlers = new Dictionary<TrackOutAction, Func<Task>>
        {
            { TrackOutAction.Rb1Tool1, async () => await HandleTrackOut(1, 1) },
            { TrackOutAction.Rb1Tool2, async () => await HandleTrackOut(1,2) },
             { TrackOutAction.Rb2Tool1, async () => await HandleTrackOut(1,1) },
            { TrackOutAction.Rb2Tool2, async () => await HandleTrackOut(1,2) }
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
                LogTxt.Add(LogTxt.Type.Exception, $"[TRACKOUT] No handler found for action {_action}");
            }
        }

        private async Task HandleTrackOut(int robotNo, int toolNumber)
        {
            switch (robotNo)
            {
                case 1:
                    switch (toolNumber)
                    {
                        case 1:
                            _cellIDWord = Word.ROBOT1_1_TRACKOUT_CELLID; _resultTrackOutWord = Word.ROBOT1_1_TRACKOUT_RESULT; break;
                         
                        case 2:
                            _cellIDWord = Word.ROBOT1_2_TRACKOUT_CELLID; _resultTrackOutWord = Word.ROBOT1_2_TRACKOUT_RESULT; break;
                    }
                    break;
                case 2:
                    switch (toolNumber)
                    {
                        case 1:
                            _cellIDWord = Word.ROBOT2_1_TRACKOUT_CELLID; _resultTrackOutWord = Word.ROBOT2_1_TRACKOUT_RESULT; break;
                        case 2:
                            _cellIDWord = Word.ROBOT2_2_TRACKOUT_CELLID; _resultTrackOutWord = Word.ROBOT2_2_TRACKOUT_RESULT; break;
                    }
                    break;
            }
          
            try
            {
                string cellIDTrackOut = "";
                string resultTrackOut = "";
                bool isTimeOut = false;
                (cellIDTrackOut, resultTrackOut, isTimeOut) = await _controller.WaitForPlcData(_cellIDWord, _resultTrackOutWord);
                //if (isTimeOut)
                //{
                //   _controller.SetSignalBitFromPC("TIME_OUT", true);
                //    return;
                //}
                LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKOUT][RB{robotNo}][TOOL{toolNumber}]:" 
                    + $"RECEIVE DATA PLC: CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                    $"RESULT:{_controller.GetWordValueFromPLC(_resultTrackOutWord, true)}");

                // Convert Data
                if(resultTrackOut == "G") { resultTrackOut = "GOOD"; }
                else if(resultTrackOut == "V") { resultTrackOut= "NG Validation"; }
                else if(resultTrackOut == "O") { resultTrackOut= "Manual TrackOut"; }
                // Save To Log
                CellData cellData = _controller.FindCellInListTemp(cellIDTrackOut, false, false, "", true);
                if (cellData != null)
                {
                    cellData.TrackOut = resultTrackOut;
                    cellData.MCEndTime = DateTime.Now;
                    cellData.MCTackTime = (cellData.MCEndTime - cellData.MCStartTime).TotalSeconds;

                    LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKOUT][RB{robotNo}][TOOL{toolNumber}]" + 
                        $"UPDATE DATA IN LIST: CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                        $"RESULT:{_controller.GetWordValueFromPLC(_resultTrackOutWord, true)}" +
                        $"");
                  await  _controller.SaveDataLog(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKOUT][RB{robotNo}][TOOL{toolNumber}]" + 
                        $"SAVE DATA TO DATALOG: CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " + 
                        $"RESULT:{_controller.GetWordValueFromPLC(_resultTrackOutWord, true)}");

                    string logMessage = _controller.CreateLogFollowCellData(cellData);
                    _controller.ListCellDatas.CellDatas.Remove(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKOUT][RB{robotNo}][TOOL{toolNumber}]:  CellData Remove from List:" + logMessage);
                }
                else
                {
                    string m = _controller.CreateLogFollowCellData(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKOUT][RB{robotNo}][TOOL{toolNumber}]:CANNOT FIND CELL IN QUEUE  CellData:" + m);
                }

            }
            catch (Exception e)
            {
                string debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, e.Message);
                LogTxt.Add(LogTxt.Type.Exception, $"[TRACKOUT][RB{robotNo}][TOOL{toolNumber}]" + debug);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKOUT][RB{robotNo}][TOOL{toolNumber}]" + debug);
            }
        }
    }
}
