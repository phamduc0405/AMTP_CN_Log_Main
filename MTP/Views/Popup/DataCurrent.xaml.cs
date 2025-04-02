using ACO2_App._0;
using ACO2_App._0.INIT;
using ACO2_App._0.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MTP.Views.Popup
{
    /// <summary>
    /// Interaction logic for DataCurrent.xaml
    /// </summary>
    public partial class DataCurrent : Window
    {
        private Controller _controller;
        private Equipment _equipment;
        private Channel _channel;
        /// <summary>
        /// typeNG = 1 (ALL NG), 2 (NG Cont), 3 (NG Ins)
        /// </summary>
        public DataCurrent(Equipment equipment, Channel channel)
        {
            InitializeComponent();
            _controller = MainWindow.Controller;
            _channel = channel;
            _equipment = equipment;
            _controller.CurrDataEvent -= OnCurrDataUpdated;
            _controller.CurrDataEvent += OnCurrDataUpdated;
            _controller.DefectListUpdated -= _controller_DefectListUpdated;
            _controller.DefectListUpdated += _controller_DefectListUpdated;
            CreateEvent();
            LoadDataFromController();
            LoadDefectListFromController();
        }

        private void _controller_DefectListUpdated(List<DefectInfo> obj)
        {
            Dispatcher.Invoke(() =>
            {
                LoadDefectListFromController();
            });
        }

        private void OnCurrDataUpdated(List<CurrentData> currDatas)
        {
            Dispatcher.Invoke(() =>
            {
                LoadDataFromController();
            });
        }
        private void LoadDefectListFromController()
        {
            List<DefectInfo> currDfSnapshot;

            lock (_controller.DefectInfo)
            {
                currDfSnapshot = _controller.DefectInfo.ToList();// Shallow copy danh sách
            }

            Dispatcher.Invoke(() =>
            {
               List<DefectInfo> defectlist= _controller.GetDefectsForChannel(currDfSnapshot, (_equipment.EqpConfig.EQPIndex + 1).ToString(), _channel.ChannelNo);

                if (defectlist != null)
                {
                    lstCurrData.ItemsSource = null;
                    lstCurrData.ItemsSource = defectlist;
                }
                else
                {
                    
                }
            });
        }
        private void LoadDataFromController()
        {
            List<CurrentData> currDatasSnapshot;

            lock (_controller.CurrsDatas)
            {
                currDatasSnapshot = _controller.CurrsDatas.ToList();// Shallow copy danh sách
            }

            Dispatcher.Invoke(() =>
            {
                var currentData = currDatasSnapshot
                    .FirstOrDefault(c => c.Zone == (_equipment.EqpConfig.EQPIndex + 1).ToString()
                                      && c.ChannelName == _channel.ChannelNo);

                if (currentData != null)
                {
                    int total = currentData.Total;
                    int good = currentData.Good;
                    int ngContact = currentData.NGContact;
                    int ngIns = currentData.NGIns;

                    double perGood = total > 0 ? (double)good / total * 100 : 0;
                    double perNGContact = total > 0 ? (double)ngContact / total * 100 : 0;
                    double perNGIns = total > 0 ? (double)ngIns / total * 100 : 0;

                    txtTotal.Text = total.ToString();
                    txtOk.Text = good.ToString();
                    txtNgContact.Text = ngContact.ToString();
                    txtNgIns.Text = ngIns.ToString();
                    txtPerOk.Text = $"({perGood:F1}%)";
                    txtPerNgContact.Text = $"({perNGContact:F1}%)";
                    txtPerNgIns.Text = $"({perNGIns:F1}%)";
                }
                else
                {
                    txtPerNgIns.Text = "0";
                    txtOk.Text = "0";
                    txtNgContact.Text = "0";
                    txtNgIns.Text = "0";
                    txtPerOk.Text = "(0%)";
                    txtPerNgContact.Text = "(0%)";
                    txtPerNgIns.Text = "(0%)";
                }
            });
        }
        private void CreateEvent()
        {
            btnClear.Click += (s, e) =>
            {
                //lstCurrData.Items.Clear();
                //var debug = string.Format("Class:{0} Method:{1} Event:{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ((Control)s).Name);
                //LogTxt.Add(LogTxt.Type.UI, debug);
                //this.DialogResult = true;
            };
            btnClose.Click += (s, e) =>
            {
                var debug = string.Format("Class:{0} Method:{1} Event:{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ((Control)s).Name);
                LogTxt.Add(LogTxt.Type.UI, debug);
                this.DialogResult = false;
                this.Close();
            };
        }
        //private void LoadAllNG(int type)
        //{
        //    int total = 0;
        //    int totalGood = 0;
        //    int totalNGContact = 0;
        //    int totalNGIns = 0;
        //    int totalNG = 0;
        //    List<CurrentData> tCurrData = new List<CurrentData>();

        //    Dispatcher.Invoke(new Action(() =>
        //    {

        //        total += _controller.CurrsDatas.Find.Sum(x => x.Total);
        //        totalGood += _controller.CurrsDatas.Sum(cd => cd.Good);
        //        totalNGContact += _controller.CurrsDatas.Sum(cd => cd.NGContact);
        //        totalNGIns += _controller.CurrsDatas.Sum(cd => cd.NGIns);
        //        totalNG = totalNGContact + totalNGIns;
        //        txtTotal.Text = total.ToString();
        //        txtNgContact.Text = totalNGContact.ToString();
        //        txtNgIns.Text = totalNGIns.ToString();
        //        txtOk.Text = totalGood.ToString();
        //        txtTotalNg.Text = totalNG.ToString();
        //        txtPerOk.Text = String.Format("{0:f}", (float.Parse(txtOk.Text)) * 100 / float.Parse(txtTotal.Text)) + "%";
        //        txtPerNgContact.Text = String.Format("{0:f}", (float.Parse(txtNgContact.Text)) * 100 / float.Parse(txtTotal.Text)) + "%";
        //        txtPerNgIns.Text = String.Format("{0:f}", (float.Parse(txtNgIns.Text)) * 100 / float.Parse(txtTotal.Text)) + "%";
        //        txtPerTotalNg.Text = String.Format("{0:f}", (float.Parse(txtTotalNg.Text)) * 100 / float.Parse(txtTotal.Text)) + "%";

        //        for (int i = 0; i < _controller.Equipment.Count; i++)
        //        {
        //            foreach (var item in _controller.Equipment[i].CurrsDatas)
        //            {
        //                tCurrData.Add(item);
        //            }
        //        }
        //        lstCurrData.Items.Clear();

        //        lstCurrData.Dispatcher.Invoke(new Action(() =>
        //        {
        //            if (type == 1)
        //            {
        //                for (int i = 0; i < _controller.Equipment.Count; i++)
        //                {
        //                    List<NGData> tempDatas = _controller.Equipment[i].NGDatas;
        //                    tempDatas = tempDatas.Where(x => (x.InsResult != "GOOD" && !string.IsNullOrEmpty(x.InsResult)) || (x.ContactResult != "GOOD" && !string.IsNullOrEmpty(x.ContactResult)) || x.Rework == "REWORK").ToList();
        //                    if (tempDatas.Count > 0)
        //                    {
        //                        foreach (var item in tempDatas)
        //                        {
        //                            lstCurrData.Items.Add(item);
        //                        }
        //                    }

        //                }
        //            }
        //            if (type == 2)
        //            {


        //                for (int i = 0; i < _controller.Equipment.Count; i++)
        //                {
        //                    List<NGData> tempDatas = _controller.Equipment[i].NGDatas.Where(x => (string.IsNullOrEmpty(x.InsResult) && x.ContactResult != "GOOD" && !string.IsNullOrEmpty(x.ContactResult)) || x.Rework == "REWORK").ToList();
        //                    if (tempDatas.Count > 0)
        //                    {
        //                        foreach (var item in tempDatas)
        //                        {
        //                            lstCurrData.Items.Add(item);
        //                        }
        //                    }
        //                }
        //            }
        //            if (type == 3)
        //            {
        //                for (int i = 0; i < _controller.Equipment.Count; i++)
        //                {
        //                    List<NGData> tempDatas = _controller.Equipment[i].NGDatas.Where(x => (x.InsResult != "GOOD" && !string.IsNullOrEmpty(x.InsResult) && x.ContactResult == "GOOD") || x.Rework == "REWORK").ToList();
        //                    if (tempDatas.Count > 0)
        //                    {
        //                        foreach (var item in tempDatas)
        //                        {
        //                            lstCurrData.Items.Add(item);

        //                        }
        //                    }
        //                }
        //            }
        //        }));
        //    }));
        //}
    }
}
