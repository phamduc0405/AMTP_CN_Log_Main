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

namespace ACO2.Views.Config
{
    /// <summary>
    /// Interaction logic for PartialEqpView.xaml
    /// </summary>
    public partial class PartialEqpView : UserControl
    {
        private string _header;
        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }
        public bool IsError { 
        get { return CheckError(); }
        }

        private int Error = 0;
        public PartialEqpView()
        {
            InitializeComponent();
            this.DataContext = this;
            CreateEvent();
        }
        private void CreateEvent()
        {

            txtCountPaper.TextChanged += (s, e) =>
            {
                var a = int.TryParse(txtCountPaper.Text, out int avalue);

                txtCountWarning.Visibility = a ? Visibility.Collapsed : Visibility.Visible;
                

            };
        }
        private bool CheckError()
        {
            if (!int.TryParse(txtCountPaper.Text, out int avalue) && tglisFirstMachine.IsChecked==true)
            {
                return true;
            }
            if (tglisUseMcr.IsChecked==true && string.IsNullOrEmpty(txtBarcode.Text.Trim()) && tglisSkip.IsChecked != true)
            {
                return true;
            }
            return false;
        }
    }
}
