using APlc;
using ACO2_App._0.Style;
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

namespace ACO2_App._0.Views
{
    /// <summary>
    /// Interaction logic for MonitorBits.xaml
    /// </summary>
    public partial class MonitorBits : UserControl
    {
        private Controller _controller;
        public MonitorBits()
        {
            InitializeComponent();
            this.DataContext = this;
            _controller = MainWindow.Controller;
            Initial();
        }
        #region Private Method
        private void Initial()
        {
            // Unloaded += MonitorBits_Unloaded;// Khai báo biến để lưu trữ tham chiếu đến hàm xử lý sự kiện BitOutChangedEvent
         //   EventHandler bitOutChangedHandler = null;
            foreach (var bit in _controller.Plc.Bits)
            {
                BitModel b = new BitModel();
                b = bit;
                {
                    IOComment io = new IOComment();
                    io.txtAddress.Text = "B" + bit.PLCHexAdd.ToString();
                    io.txtIO.Text = bit.Comment.ToString();
                    io.elpOnOff.Fill = b.GetPLCValue ? Brushes.YellowGreen : Brushes.Gray;
                    io.UpdateEffect();
                    Ellipse ellOnOff = io.elpOnOff;
                    BitModel.BitChangedEventDelegate bitChangedHandler = () =>
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            ellOnOff.Fill = b.GetPLCValue ? Brushes.YellowGreen : Brushes.Gray;
                            io.UpdateEffect();
                        }));
                    };
                    b.BitChangedEvent += bitChangedHandler;
                    Unloaded += (s, e) => { };
                    wrpInput.Children.Add(io);
                
                
                    IOComment ioOut = new IOComment();
                    ioOut.txtAddress.Text = "B" + bit.PCHexAdd.ToString();
                    ioOut.txtIO.Text = bit.Comment.ToString();
                    ioOut.elpOnOff.Fill = b.GetPCValue ? Brushes.YellowGreen : Brushes.Gray;
                    ioOut.UpdateEffect();
                    Ellipse ellOnOffOut = ioOut.elpOnOff;
                    BitModel.BitOutChangedEventDelegate bitOutChangedHandler = () =>
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            ellOnOffOut.Fill = b.GetPCValue ? Brushes.YellowGreen : Brushes.Gray;
                            ioOut.UpdateEffect();
                        }));
                    };
                    b.BitOutChangedEvent += bitOutChangedHandler;
                    wrpOutput.Children.Add(ioOut);
                    Unloaded += (s, e) =>
                    {
                        b.BitChangedEvent -= bitChangedHandler;
                        b.BitOutChangedEvent -= bitOutChangedHandler;
                    };
                }
            }
        }
        #endregion

    }
}
