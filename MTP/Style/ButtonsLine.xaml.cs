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

namespace MTP_MANUAL_App.Style
{
    /// <summary>
    /// Interaction logic for ButtonsLine.xaml
    /// </summary>
    public partial class ButtonsLine : UserControl
    {
       
        public ButtonsLine()
        {
            InitializeComponent();
            CreaterEvent();
            btnNb1.Tag = "Red";
        }
        private void CreaterEvent()
        {
            btnNb1.Click += (s, e) =>
            {
                btnNb1.Tag = "Red";
                btnNb2.Tag = "White";
                btnNb3.Tag = "White";
            };
            btnNb2.Click += (s, e) =>
            {
                btnNb1.Tag = "White";
                btnNb2.Tag = "Red" ;
                btnNb3.Tag = "White";
            };
            btnNb3.Click += (s, e) =>
            {
                btnNb1.Tag = "White";
                btnNb3.Tag = "Red";
                btnNb2.Tag = "White";
            };
        }
    }
}
