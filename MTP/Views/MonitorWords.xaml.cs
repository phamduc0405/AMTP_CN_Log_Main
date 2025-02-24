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
    /// Interaction logic for MonitorWords.xaml
    /// </summary>
    public partial class MonitorWords : UserControl
    {

        private Controller _controller;
        public MonitorWords()
        {
            InitializeComponent();
            _controller = MainWindow.Controller;
            Initial();
        }
        #region Private Method
        private void Initial()
        {
            foreach (var word in _controller.Plc.Words)
            {
                WordModel w = new WordModel();
                w = word;
                {
                    WordComment wordCmt = new WordComment();
                    wordCmt.txtAddress.Text = "W" + w.Start.ToString();
                    wordCmt.txtComment.Text = string.IsNullOrEmpty(w.Comment) ? "" : w.Comment;
                    wordCmt.txtLength.Text = w.Length.ToString();
                    wordCmt.txtValue.Text = w.GetValue.ToString();
                    TextBlock tblValue = wordCmt.txtValue;
                    w.WordChangedEvent += () =>
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            tblValue.Text = w.GetValue.ToString();
                        }));

                    };
                    if (w.Address < APlc.PlcComm.WriteStartWordAddress)
                    {
                        wrpInput.Children.Add(wordCmt);
                    }
                    else wrpOutput.Children.Add(wordCmt);

                }
            }
        }
        #endregion

    }
}
