using ACO2_App._0.Views;
using MTP.Views.Config;
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
    /// Interaction logic for ConfigMainView.xaml
    /// </summary>
    public partial class ConfigMainView : UserControl
    {
        public ConfigMainView()
        {
            InitializeComponent();
            CreateEvent();
        }
        private void CreateEvent()
        {
            btnPLCSetting.Click += (s, e) =>
            {
                this.Content = new ConfigView();
            };
            btnEquipSetting.Click += (s, e) =>
            {
                this.Content = new EquipmentConfigView();
            };
            btnDefectSetting.Click += (s, e) =>
            {
                this.Content = new ConfigDefectCodeView();
            };
        }
    }
}
