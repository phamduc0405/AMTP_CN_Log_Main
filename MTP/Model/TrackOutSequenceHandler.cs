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
            Tool1,
            Tool2
        }
        private Controller _controller;

        private Dictionary<TrackOutAction, Func<Task>> _handlers;
        private TrackOutAction _action;
        private string _cellIDWord = "";
        private string _resultTrackOutWord = "";
        private string _abRuleWord = "";
        private string _reTryWord = "";
        private string _rechecked = "";
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
            { "TRACK_OUT_TOOL1", TrackOutAction .Tool1 },
            { "TRACK_OUT_TOOL2", TrackOutAction .Tool2 }
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
            { TrackOutAction.Tool1, async () => await HandleTrackOut(1) },
            { TrackOutAction.Tool2, async () => await HandleTrackOut(2) }
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

        private async Task HandleTrackOut(int toolNumber)
        {
            switch (toolNumber)
            {
                case 1:
                    _cellIDWord = "CELL_ID_TRACKOUT_TOOL1"; _resultTrackOutWord = "RESULT_TRACKOUT_1";
                    _abRuleWord = "AB_RULE_TRACKOUT_TOOL1"; _reTryWord = "RETRY_TRACKOUT_TOOL1";
                    _rechecked = "RECHECKED_TRACKOUT_TOOL1"; break;
                case 2:
                    _cellIDWord = "CELL_ID_TRACKOUT_TOOL2"; _resultTrackOutWord = "RESULT_TRACKOUT_2"; 
                    _abRuleWord = "AB_RULE_TRACKOUT_TOOL2"; _reTryWord = "RETRY_TRACKOUT_TOOL2";
                    _rechecked = "RECHECKED_TRACKOUT_TOOL2"; break;
            }
            try
            {
                string cellIDTrackOut = "";
                string resultTrackOut = "";
                bool isTimeOut = false;
                (cellIDTrackOut, resultTrackOut, isTimeOut) = await _controller.WaitForPlcData(_cellIDWord, _resultTrackOutWord);
                if (isTimeOut)
                {
                   _controller.SetSignalBitFromPC("TIME_OUT", true);
                    return;
                }
                string abRule = "";
                string retry = "";
                string rechecked = "";
                abRule = _controller.GetWordValueFromPLC(_abRuleWord, true);
                retry = _controller.GetWordValueFromPLC(_reTryWord, true);
                rechecked = _controller.GetWordValueFromPLC(_rechecked, true);
                if(rechecked == "0") { rechecked = "NO"; }
                if (rechecked == "1") { rechecked = "YES"; }
                LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKOUT][TOOL{toolNumber}]:" 
                    + $"RECEIVE DATA PLC: CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                    $"RESULT:{_controller.GetWordValueFromPLC(_resultTrackOutWord, true)}"+
                    $"AB RULE:{_controller.GetWordValueFromPLC(_abRuleWord, true)}" +
                    $"RETRY:{_controller.GetWordValueFromPLC(_reTryWord, true)}" +
                    $"RECHECKED:{_controller.GetWordValueFromPLC(_rechecked, true)}");

                // Save To Log
                CellData cellData = _controller.FindCellInListTemp(cellIDTrackOut, false, false, "", true);
                if (cellData != null)
                {
                    cellData.Rechecked=rechecked;
                    cellData.ABRule=abRule;
                    cellData.Retry=retry;
                    cellData.TrackOut = resultTrackOut;
                    cellData.MCEndTime = DateTime.Now;
                    cellData.MCTackTime = (cellData.MCEndTime - cellData.MCStartTime).TotalSeconds;

                    LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKOUT][TOOL{toolNumber}]" + 
                        $"UPDATE DATA IN LIST: CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                        $"RESULT:{_controller.GetWordValueFromPLC(_resultTrackOutWord, true)}" +
                        $"");
                  await  _controller.SaveDataLog(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKOUT][TOOL{toolNumber}]" + 
                        $"SAVE DATA TO DATALOG: CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " + 
                        $"RESULT:{_controller.GetWordValueFromPLC(_resultTrackOutWord, true)}");

                    string logMessage = _controller.CreateLogFollowCellData(cellData);
                    _controller.ListCellDatas.CellDatas.Remove(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKOUT][TOOL{toolNumber}]:  CellData Remove from List:" + logMessage);
                }
                else
                {
                    string m = _controller.CreateLogFollowCellData(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKOUT][TOOL{toolNumber}]:CANNOT FIND CELL IN QUEUE  CellData:" + m);
                }

            }
            catch (Exception e)
            {
                string debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, e.Message);
                LogTxt.Add(LogTxt.Type.Exception, $"[TRACKOUT][TOOL{toolNumber}]" + debug);
                LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKOUT][TOOL{toolNumber}]" + debug);
            }
        }
    }
}
