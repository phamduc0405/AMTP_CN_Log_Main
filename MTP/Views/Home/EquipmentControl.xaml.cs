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
using static APlc.MelsecIF;

namespace MTP.Views.Home
{
    /// <summary>
    /// Interaction logic for EquipmentControl.xaml
    /// </summary>
    public partial class EquipmentControl : UserControl
    {
        private Equipment _equipment;
        private bool _isPCSignalConnected = true;
        private Controller _controller;
        public EquipmentControl(Equipment equipment)
        {
            InitializeComponent();
            _controller = MainWindow.Controller;
            _equipment = equipment;
            _equipment.ConnectEvent -= _equipment_ConnectEvent;
            _equipment.ConnectEvent += _equipment_ConnectEvent;
            txtHeader.Text = _equipment.EqpConfig.EqpName;
            LoadChannels();
            LoadDataFromController();
            _controller.CurrDataEvent -= OnCurrDataUpdated;
            _controller.CurrDataEvent += OnCurrDataUpdated;
            DisplayStatus();
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
                                      );

                if (currentData != null)
                {
                    int total = currentData.Total;
                    int good = currentData.Good;
                    int ngContact = currentData.NGContact;
                    int ngIns = currentData.NGIns;
                    int ngtotal = ngIns + ngContact;
                    double perGood = total > 0 ? (double)good / total * 100 : 0;
                    double perNGContact = total > 0 ? (double)ngContact / total * 100 : 0;
                    double perNGIns = total > 0 ? (double)ngIns / total * 100 : 0;

                    txtCountTotalEquip.Text = total.ToString();
                    txtCountOKEquip.Text = good.ToString();
                    txtCountNGInsEquip.Text = ngIns.ToString();
                    txtCountNGEquip.Text = ngContact.ToString();
                    txtNGPercentEquip.Text = $"({ngContact:F1}%)";
                    txtOKPercentEquip.Text = $"({perGood:F1}%)";
                    txtNGInsPercentEquip.Text = $"({perNGIns:F1}%)";
                }
                else
                {
                    txtCountTotalEquip.Text = "0";
                    txtCountOKEquip.Text = "0";
                    txtCountNGInsEquip.Text = "0";
                    txtCountNGEquip.Text = "0";
                    txtNGPercentEquip.Text = "(0%)";
                    txtOKPercentEquip.Text = "(0%)";
                    txtNGInsPercentEquip.Text = "(0%)";
                }
            });
        }
        private void DisplayStatus()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (_equipment.Cons == null) return;
                _isPCSignalConnected = _equipment.TcpT5Mess.IsConnected;

                if (_isPCSignalConnected )
                {
                    brdMes.Visibility = Visibility.Hidden;
                    txtStatus.Text = "";
                    txtStatus.Visibility = Visibility.Hidden;
                }
                else
                {
                    brdMes.Visibility = Visibility.Visible;
                    txtStatus.Visibility = Visibility.Visible;
                    if (!_isPCSignalConnected) txtStatus.Text = $"{_equipment.EqpConfig.EqpName} DISCONNECTED";
                }
            }));
        }
        private void _equipment_ConnectEvent(bool isConnect)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                _isPCSignalConnected = isConnect;
                DisplayStatus();
            }));
        }

        private void LoadChannels()
        {
            stkMain.Children.Clear();
            foreach (var channel in _equipment.Channels)
            {
                var channelControl = new ChannelControl(_equipment,channel);
                stkMain.Children.Add(channelControl);
            }
        }

        public void UpdateUI()
        {
            foreach (var child in stkMain.Children)
            {
                if (child is ChannelControl channelControl)
                {
                    channelControl.UpdateUI();
                }
            }
        }

     

    }
}
