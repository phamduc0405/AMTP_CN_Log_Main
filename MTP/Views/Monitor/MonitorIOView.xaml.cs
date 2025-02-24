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
    /// Interaction logic for MonitorIOView.xaml
    /// </summary>
    public partial class MonitorIOView : UserControl
    {
        private Controller _controller;
        public MonitorIOView()
        {
            InitializeComponent();
            _controller = MainWindow.Controller;
            Initial();
            CreateEvent();
        }

        private void Initial()
        {
            grdView.Children.Clear();
            MonitorBits bits = new MonitorBits();
            grdView.Children.Add(bits);
            btnBits.IsChecked = true;
            btnWords.IsChecked = false;
            
        }
        private void CreateEvent()
        {
            btnBits.Checked += (s, e) =>
            {
                grdView.Children.Clear();
                MonitorBits bits = new MonitorBits();
                grdView.Children.Add(bits);
                //bits.Unloaded += (sender, events) => { grdView.Children.Clear(); };
            };
            btnWords.Checked += (s, e) =>
            {
                grdView.Children.Clear();
                MonitorWords words = new MonitorWords();
                grdView.Children.Add(words);
               // words.Unloaded += (sender, events) => { grdView.Children.Clear(); };
            };
        }

        private void Bits_Unloaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
