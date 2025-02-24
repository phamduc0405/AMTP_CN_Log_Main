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
    /// Interaction logic for MonitorWords.xaml
    /// </summary>
    public partial class MonitorWords : UserControl
    {
        private PLCHelper _plcH;
        private PlcComm _plc;
        private Controller _controller;
        private bool _disposed = false; // Để theo dõi trạng thái dispose
        private List<WordModel.WordChangedEventDelegate> _wordChangedHandlers = new List<WordModel.WordChangedEventDelegate>();
        public MonitorWords()
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
            foreach (var word in _plcH.Words)
            {
                WordModel w = new WordModel { Start = word.Start, Comment = word.Comment, Length = word.Length };
                w = word; // Gán word hiện tại cho model

                WordComment wordCmt = CreateWordComment(w);
                AddToAppropriatePanel(w, wordCmt);

                // Đăng ký sự kiện WordChangedEvent với đúng delegate
                WordModel.WordChangedEventDelegate wordChangedHandler = () =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        wordCmt.txtValue.Text = w.GetValue.ToString();
                    });
                };
                w.WordChangedEvent += wordChangedHandler;
                _wordChangedHandlers.Add(wordChangedHandler);
            }
        }
        private WordComment CreateWordComment(WordModel w)
        {
            var wordCmt = new WordComment
            {
                txtAddress = { Text = "W" + w.Start.ToString() },
                txtComment = { Text = string.IsNullOrEmpty(w.Comment) ? "" : w.Comment },
                txtLength = { Text = w.Length.ToString() },
                txtValue = { Text = w.GetValue.ToString() }
            };
            return wordCmt;
        }

        private void AddToAppropriatePanel(WordModel w, WordComment wordCmt)
        {
            if (w.Address < _plc.WriteStartWordAddress)
            {
                wrpInput.Children.Add(wordCmt);
            }
            else
            {
                wrpOutput.Children.Add(wordCmt);
            }
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
                    // Hủy đăng ký các sự kiện đã đăng ký
                    UnsubscribeAllEvents();
                    wrpInput.Children.Clear();
                    wrpOutput.Children.Clear();
                }
                _disposed = true;
            }
        }

        private void UnsubscribeAllEvents()
        {
            foreach (var word in _plcH.Words)
            {
                foreach (var handler in _wordChangedHandlers)
                {
                    word.WordChangedEvent -= handler;
                }
            }
            _wordChangedHandlers.Clear();
        }

        ~MonitorWords()
        {
            Dispose(false); // Gọi Dispose từ finalizer nếu chưa được gọi
        }
        #endregion
    }
}
