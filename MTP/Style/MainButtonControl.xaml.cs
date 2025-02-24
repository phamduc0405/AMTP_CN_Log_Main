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

namespace ACO2_App.Style
{
    /// <summary>
    /// Interaction logic for MainButtonControl.xaml
    /// </summary>
    public partial class MainButtonControl : UserControl
    {
        public MainButtonControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        // public string btnColor { get; set; }
        private string _btnColor;
        public string btnColor
        {
            get { return _btnColor; }
            set
            {
                _btnColor = value;
            }
        }


        //public event RoutedEventHandler btn_Automode_Click;
        //        public event RoutedEventHandler btn_Manualmode_Click;
        //        private void btn_AutoMode_Click(object sender, RoutedEventArgs e)
        //        {
        //            if (btn_Automode_Click != null)
        //            {
        //                btn_Automode_Click(this, e);
        //            }
        //        }
        //        private void btn_ManualMode_Click(object sender, RoutedEventArgs e)
        //        {
        //            if (btn_Manualmode_Click != null)
        //            {
        //                btn_Manualmode_Click(this, e);
        //            }
        //        }
    }
}
