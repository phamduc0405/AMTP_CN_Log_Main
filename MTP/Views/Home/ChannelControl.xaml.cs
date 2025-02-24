using ACO2_App._0;
using ACO2_App._0.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MTP.Views.Home
{
    /// <summary>
    /// Interaction logic for ChannelControl.xaml
    /// </summary>
    public partial class ChannelControl : UserControl
    {
        private Equipment _equipment;
        private Channel _channel;
        private Controller _controller;
        private ChannelStatus _channelStatus;
        private int _eqpIndex = 0;
        private bool _isSkip = false;
        public ChannelControl(Equipment equipment,Channel channel)
        {
            InitializeComponent();
            _controller = MainWindow.Controller;
            _channel = channel;
            _equipment = equipment;
            LoadDataFromController();
            _controller.CurrDataEvent -= OnCurrDataUpdated;
            _controller.CurrDataEvent += OnCurrDataUpdated;
            _channel.ResultInsEvent -= _channel_ResultInsEvent;
            _channel.ResultInsEvent += _channel_ResultInsEvent;
            _eqpIndex = int.Parse(_equipment.EqpConfig.EQPIndex.ToString());
            _channelStatus = _controller.MachineStatus.ChannelStatus.FirstOrDefault(x => x.ZoneNo == (_eqpIndex + 1).ToString() && x.Channnel == channel.ChannelNo);

            if(_channelStatus!=null)
            {
                if (_channelStatus.Status == "SKIP")
                {
                    _isSkip = true;
                    txtResult.Text = "SKIP";
                    brdPopup.Background = Brushes.Gray;
                    txtResult.Foreground = Brushes.Black;
                    brdPopup.Visibility = Visibility.Visible;
                }
                _channelStatus.PropertyChannelChanged += _channelStatus_PropertyChannelChanged; ;

            }
            // UpdateUI();
        }

        private void _channelStatus_PropertyChannelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var channel = (ChannelStatus)sender;
                switch (e.PropertyName)
                {
                   
                    case "Status":
                        if(channel.Status == "SKIP")
                        {
                            _isSkip = true;
                            txtResult.Text = "SKIP";
                            brdPopup.Background = Brushes.Gray;
                            txtResult.Foreground = Brushes.Black;
                            brdPopup.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            _isSkip = false;
                            txtResult.Text = "";
                            brdPopup.Visibility = Visibility.Hidden;
                        }
                        break;
                }
            });
        }

        private void OnCurrDataUpdated(List<CurrentData> currDatas)
        {
            Dispatcher.Invoke(() =>
            {
                LoadDataFromController();
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

                    txtChTotal.Text = total.ToString();
                    txtChOK.Text = good.ToString();
                    txtChContactNG.Text = ngContact.ToString();
                    txtCInspNG.Text = ngIns.ToString();
                    txtPerChOK.Text = $"({perGood:F1}%)";
                    txtPerChContactNG.Text = $"({perNGContact:F1}%)";
                    txtPerChInsNG.Text = $"({perNGIns:F1}%)";
                }
                else
                {
                    txtChTotal.Text = "0";
                    txtChOK.Text = "0";
                    txtChContactNG.Text = "0";
                    txtCInspNG.Text = "0";
                    txtPerChOK.Text = "(0%)";
                    txtPerChContactNG.Text = "(0%)";
                    txtPerChInsNG.Text = "(0%)";
                }
            });
        }

        private void _channel_ResultInsEvent(string channelNo, string ContactResult, string MTPWriteResult, string DefectCode)
        {
            Dispatcher.Invoke(() =>
            {
                txtHeader.Text = "CH" + _channel.ChannelNo;
                txtCellID.Text = _channel.CellID;
                if(ContactResult == "GOOD"|| MTPWriteResult == "GOOD"|| !string.IsNullOrEmpty(DefectCode))
                {
                    if (ContactResult == "GOOD")
                    {
                        txtResult.Text = "CONTACT OK";
                        brdPopup.Background = Brushes.Yellow;
                        txtResult.Foreground = Brushes.Black;
                        brdPopup.Visibility = Visibility.Visible;
                    }
                    if (MTPWriteResult == "GOOD")
                    {
                        txtResult.Text = "MTP WRITE OK";
                        brdPopup.Background = Brushes.Green;
                        txtResult.Foreground = Brushes.White;
                        brdPopup.Visibility = Visibility.Visible;
                    }
                    if (!string.IsNullOrEmpty(DefectCode))
                    {
                        txtResult.Text = _channel.DefectCode;
                        brdPopup.Background = Brushes.Red;
                        txtResult.Foreground = Brushes.White;
                        brdPopup.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (!_isSkip)
                    {
                        brdPopup.Visibility = Visibility.Hidden;
                    }
                }
                if (!string.IsNullOrEmpty(DefectCode))
                {
                    txtResult.Text = $"{DefectCode}";
                }
            });
        }

        public void UpdateUI()
        {
            Dispatcher.Invoke(() =>
            {
                txtHeader.Text = "CH"+_channel.ChannelNo; 
                txtCellID.Text = _channel.CellID; 
                if(_channel.ContactResult == "GOOD"|| _channel.MTPWriteResult == "GOOD"|| !string.IsNullOrEmpty(_channel.DefectCode))
                {
                    if (_channel.ContactResult == "GOOD")
                    {
                        txtResult.Text = "CONTACT OK";
                        brdPopup.Background = Brushes.Yellow;
                        txtResult.Foreground = Brushes.Black;
                        brdPopup.Visibility = Visibility.Visible;
                    }
                    if (_channel.MTPWriteResult == "GOOD")
                    {
                        txtResult.Text = "MTP WRITE OK";
                        brdPopup.Background = Brushes.Green;
                        txtResult.Foreground = Brushes.White;
                        brdPopup.Visibility = Visibility.Visible;
                    }
                    if (!string.IsNullOrEmpty(_channel.DefectCode))
                    {
                        txtResult.Text = _channel.DefectCode;
                        brdPopup.Background = Brushes.Red;
                        txtResult.Foreground = Brushes.White;
                        brdPopup.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (!_isSkip)
                    {
                        brdPopup.Visibility = Visibility.Hidden;
                    }
                }
                if (!string.IsNullOrEmpty(_channel.DefectCode))
                {
                    txtResult.Text=$"{_channel.DefectCode}";
                }
            });
        }
    }
}
