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
using ACO2_App._0.Plc;

namespace ACO2_App._0.Views
{
    /// <summary>
    /// Interaction logic for MonitorBits.xaml
    /// </summary>
    public partial class MonitorBits : UserControl
    {
        private PLCHelper _plcH;
        private PlcComm _plc;
        private Controller _controller;
        private bool _disposed = false; // Theo dõi trạng thái dispose
        private List<BitModel.BitChangedEventDelegate> _bitChangedHandlers = new List<BitModel.BitChangedEventDelegate>();
        private List<BitModel.BitOutChangedEventDelegate> _bitOutChangedHandlers = new List<BitModel.BitOutChangedEventDelegate>();
        public MonitorBits()
        {
            InitializeComponent();
            this.DataContext = this;
            _controller = MainWindow.Controller;
            _plc = _controller.Plc;
            _plcH = _controller.PlcH;
            Initial();
        }
        #region Private Method
        private void Initial()
        {
            if (_plcH == null) return;
            // Unloaded += MonitorBits_Unloaded;// Khai báo biến để lưu trữ tham chiếu đến hàm xử lý sự kiện BitOutChangedEvent
         //   EventHandler bitOutChangedHandler = null;
            foreach (var bit in _plcH.Bits)
            {
                BitModel b = new BitModel(_plc);
                b = bit;

                // Input
                IOComment io = CreateIOComment(bit, b.GetPLCValue,true);
                wrpInput.Children.Add(io);

                var bitChangedHandler = new BitModel.BitChangedEventDelegate(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        io.elpOnOff.Fill = b.GetPLCValue ? Brushes.YellowGreen : Brushes.Gray;
                        io.UpdateEffect();
                    });
                });
                b.BitChangedEvent += bitChangedHandler;
                _bitChangedHandlers.Add(bitChangedHandler);

                // Output
                IOComment ioOut = CreateIOComment(bit, b.GetPCValue,false);
                wrpOutput.Children.Add(ioOut);
                ioOut.elpOnOff.MouseUp += (s, e) =>
                {
                    b.SetPCValue = !b.GetPCValue;
                };
                Ellipse ellOnOffOut = ioOut.elpOnOff;
                var bitOutChangedHandler = new BitModel.BitOutChangedEventDelegate(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ioOut.elpOnOff.Fill = b.GetPCValue ? Brushes.YellowGreen : Brushes.Gray;
                        ioOut.UpdateEffect();
                    });
                });
                b.BitOutChangedEvent += bitOutChangedHandler;
                _bitOutChangedHandlers.Add(bitOutChangedHandler);
            }
        }
        private IOComment CreateIOComment(BitModel bit, bool value,bool isPLC)
        {
            IOComment io = new IOComment();
            if (isPLC)
            {
                io.txtAddress.Text = "B" + bit.PLCHexAdd.ToString();
            }
            else
            {
                io.txtAddress.Text = "B" + bit.PCHexAdd.ToString();
            }
            io.txtIO.Text = bit.Comment.ToString();
            io.elpOnOff.Fill = value ? Brushes.YellowGreen : Brushes.Gray;
            io.UpdateEffect();
            return io;
        }

        #endregion
        #region IDisposable Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Ngăn GC gọi lại Dispose lần nữa
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Giải phóng các sự kiện đã đăng ký
                    UnsubscribeAllEvents();
                    wrpInput.Children.Clear();
                    wrpOutput.Children.Clear();
                }

                _disposed = true;
            }
        }

        private void UnsubscribeAllEvents()
        {
            // Hủy tất cả các sự kiện đã đăng ký
            foreach (var handler in _bitChangedHandlers)
            {
                foreach (var bit in _plcH.Bits)
                {
                    bit.BitChangedEvent -= handler;
                }
            }
            foreach (var handler in _bitOutChangedHandlers)
            {
                foreach (var bit in _plcH.Bits)
                {
                    bit.BitOutChangedEvent -= handler;
                }
            }
            _bitChangedHandlers.Clear();
            _bitOutChangedHandlers.Clear();
        }

        ~MonitorBits()
        {
            Dispose(false); // Gọi Dispose từ finalizer nếu chưa được gọi
        }
        #endregion
    }
}
