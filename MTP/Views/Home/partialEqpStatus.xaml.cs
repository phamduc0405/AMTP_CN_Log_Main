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

namespace ACO2.Views.Home
{
    /// <summary>
    /// Interaction logic for partialEqpStatus.xaml
    /// </summary>
    public partial class partialEqpStatus : UserControl
    {
        private Equipment _eqp;
        private int _index;
        private Controller _controller;
        public partialEqpStatus(Equipment eqp,int index)
        {
            InitializeComponent();
            _index = index;
            _controller = MainWindow.Controller;
            _eqp = eqp;
            Connect_OnConnectEvent(_eqp.Conn.IsConnected);
            LoadUI();
            _eqp.Conn.Connect.OnConnectEvent -= Connect_OnConnectEvent;
            _eqp.Conn.Connect.OnConnectEvent += Connect_OnConnectEvent; 
            _eqp.DataChangedEvent -= _eqp_DataChangedEvent;
            _eqp.DataChangedEvent += _eqp_DataChangedEvent; 
            _controller.UpdateStatusEvent -= _controller_UpdateStatusEvent;
            _controller.UpdateStatusEvent += _controller_UpdateStatusEvent;
            _eqp.BarcodeEvent -= _eqp_BarcodeEvent;
            _eqp.BarcodeEvent += _eqp_BarcodeEvent;
            _controller.CurrBarcodeEvent -= _controller_CurrBarcodeEvent;
            _controller.CurrBarcodeEvent += _controller_CurrBarcodeEvent;
        }

        private void _controller_UpdateStatusEvent()
        {
          //  LoadUI();
        }

        private void _eqp_BarcodeEvent(string barcode)
        {
            Task.Factory.StartNew(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    txtCurrBarcode.Text = barcode;
                }));
            });
            
        }

        private void _eqp_DataChangedEvent(Model.ProductData data)
        {
            LoadUI(data);
        }

        private void _controller_CurrBarcodeEvent(string value, string id)
        {
            //Dispatcher.BeginInvoke(new Action(()=>
            //    {
            //        txtCurrBarcode.Text = value;
            //    }));
           
        }

        private void Connect_OnConnectEvent(bool isConnected)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                elpOnOff.Fill = isConnected ? Brushes.YellowGreen : Brushes.Gray; ;

            }));
        }
        private async Task LoadUI()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                        txtHeader.Text = _controller.ModelConfig.CurrentModel.Name;
                        string status = _controller.Plc.Words.FirstOrDefault(x => x.Area == $"MACHINE{_index + 1}" && x.Item == "STATUS").GetValue;
                        bdrAuto.Background = status == "1" ? Brushes.Green : Brushes.LightGray;
                        bdrManual.Background = status == "2" ? Brushes.Yellow : Brushes.LightGray;
                        bdrError.Background = status == "3" ? Brushes.Red : Brushes.LightGray;
                        bdrSkip.Background = status == "4" ? Brushes.Red : Brushes.LightGray;

                    txtProduct.Text = _controller.Plc.Words.FirstOrDefault(x => x.Area == $"MACHINE{_index + 1}" && x.Item == "DATA").GetValue;
                
            }));
        }
        private async Task LoadUI(Model.ProductData data)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                txtHeader.Text = _controller.ModelConfig.CurrentModel.Name;
                string status = _controller.Plc.Words.FirstOrDefault(x => x.Area == $"MACHINE{_index + 1}" && x.Item == "STATUS").GetValue;
                txtProduct.Text = data.ProductOK.ToString();
                if (status == "0") { return; }
                bdrAuto.Background = status == "1" ? Brushes.Green : Brushes.LightGray;
                bdrManual.Background = status == "2" ? Brushes.Yellow : Brushes.LightGray;
                bdrError.Background = status == "3" ? Brushes.Red : Brushes.LightGray;
                bdrSkip.Background = status == "4" ? Brushes.Red : Brushes.LightGray;

               

            }));
        }
    }
}
