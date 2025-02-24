using ACO2_App._0;
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

        public EquipmentControl(Equipment equipment)
        {
            InitializeComponent();
            _equipment = equipment;
            _equipment.ConnectEvent -= _equipment_ConnectEvent;
            _equipment.ConnectEvent += _equipment_ConnectEvent;
            txtHeader.Text = _equipment.EqpConfig.EqpName;
            LoadChannels();
            DisplayStatus();
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
