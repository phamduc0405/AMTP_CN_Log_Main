using ACO2.Model;
using ACO2.Views.Config;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ACO2.Views.Home
{
    /// <summary>
    /// Interaction logic for HomeView1.xaml
    /// </summary>
    public partial class HomeView1 : UserControl
    {
        private Controller _controller;
        private List<partialEqpStatus> _partialEqpStatus;
        public HomeView1()
        {
            InitializeComponent();
            _controller = MainWindow.Controller;
            Initial();
        }
        private void Initial()
        {
            ModelName model = _controller.ModelConfig.CurrentModel;
            _partialEqpStatus = new List<partialEqpStatus>();
            for (int i = 0; i < 6; i++)
            {
                partialEqpStatus eqp = new partialEqpStatus(_controller.Equipment[i],i);
                eqp.txtHeader.Text = $"Equiment #{i + 1}";
                eqp.txtBarcode.Text = model.ModelParas[i].BarCode;
                eqp.Margin = new Thickness(5);
                _partialEqpStatus.Add(eqp);
                Grid.SetColumn(eqp, i % 3);
                Grid.SetRow(eqp, i / 3);
                grdMain.Children.Add(eqp);
            }
        }

      
    }
}
