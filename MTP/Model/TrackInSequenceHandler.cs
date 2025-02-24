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
    public class TrackInSequenceHandler
    {
        public enum TrackInAction
        {
            Tool1,
            Tool2
        }
        private Controller _controller;
       
        private Dictionary<TrackInAction, Func<Task>> _handlers;
        private TrackInAction _action;
        private string _cellIDWord = "";
        private string _resultTrackInWord = "";

        public TrackInSequenceHandler(string action)
        {
            _controller = MainWindow.Controller;
            Initial(action);
            HandleAction();
        }

        private void Initial(string action)
        {
            var actionMap = new Dictionary<string, TrackInAction>(StringComparer.OrdinalIgnoreCase)
        {
            { "TRACK_IN_TOOL1", TrackInAction.Tool1 },
            { "TRACK_IN_TOOL2", TrackInAction.Tool2 }
        };

            if (actionMap.TryGetValue(action, out TrackInAction mappedAction))
            {
                _action = mappedAction;
            }
            else
            {
                LogTxt.Add(LogTxt.Type.Exception, $"[TRACKIN] Unknown Action: {action}");
                return;
            }


            _handlers = new Dictionary<TrackInAction, Func<Task>>
        {
            { TrackInAction.Tool1, async () => await HandleTrackIn(1) },
            { TrackInAction.Tool2, async () => await HandleTrackIn(2) }
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
                LogTxt.Add(LogTxt.Type.Exception, $"[TRACKIN] No handler found for action {_action}");
            }
        }

        private async Task HandleTrackIn(int toolNumber)
        {
            switch (toolNumber)

            {
                case 1:
                    _cellIDWord = "CELL_ID_TRACKIN_TOOL1"; _resultTrackInWord = "RESULT_TRACKIN_1"; break;
                case 2:
                    _cellIDWord = "CELL_ID_TRACKIN_TOOL2"; _resultTrackInWord = "RESULT_TRACKIN_2"; break;

            }
                try
                {
                    string cellIDTrackIn = "";
                    string resultTrackIn = "";
                    bool isTimeOut = false;
                    (cellIDTrackIn, resultTrackIn, isTimeOut) = await _controller.WaitForPlcData(_cellIDWord, _resultTrackInWord);
                    if (isTimeOut)
                    {
                    _controller.SetSignalBitFromPC("TIME_OUT", true);
                        return;
                    }
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKIN][TOOL{toolNumber}]:" 
                        + $"RECEIVE DATA PLC: CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                        $"RESULT:{_controller.GetWordValueFromPLC(_resultTrackInWord, true)}");

                    // Save To Log
                    CellData cellData = new CellData();
                    cellData.CellID = cellIDTrackIn;
                    cellData.TrackIn = resultTrackIn;
                    cellData.MCStartTime = DateTime.Now;
                    _controller.ListCellDatas.CellDatas.Add(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKIN][TOOL{toolNumber}]:" + 
                        $"ADD DATA TO QUEUEE: CELLID:{_controller.GetWordValueFromPLC(_cellIDWord, true)} " +
                        $"RESULT:{_controller.GetWordValueFromPLC(_cellIDWord, true)}");
                    string logMessage =_controller.CreateLogFollowCellData(cellData);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKIN][TOOL{toolNumber}] New CellData Added:" + logMessage);

                }
                catch (Exception e)
                {
                    string debug = string.Format("{0} exception occurred. Message is <{1}>.", MethodBase.GetCurrentMethod().Name, e.Message);
                    LogTxt.Add(LogTxt.Type.Exception, $"[TRACKIN][TOOL{toolNumber}]:" + debug);
                    LogTxt.Add(LogTxt.Type.FlowRun, $"[TRACKIN][TOOL{toolNumber}]:" + debug);
                }
        }
    }

}
