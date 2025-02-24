using ACO2_App._0;
using APlc;
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

namespace ACO2_App.Views
{
    /// <summary>
    /// Interaction logic for MachineStatusMain.xaml
    /// </summary>
    public partial class MachineStatusMain : UserControl
    {
        private Controller _controller;
        public MachineStatusMain()
        {
            InitializeComponent();
            this.DataContext = this;
            _controller = MainWindow.Controller;
            Initial();
        }
        private void Initial()
        {
            HashSet<string> cmd = new HashSet<string>();
            List<WordModel> wordCmd = new List<WordModel>();
            wordCmd = _controller.PlcH.Words.Where(x => x.Area.Contains("MAIN_")).ToList();
            foreach (WordModel w in wordCmd)
            {
                cmd.Add(w.Area.ToString());
            }
            foreach (var area in cmd)
            {
                WordModel[] words = _controller.PlcH.Words.Where(x => x.Area == area).ToArray();
                // Tạo một đối tượng UserControl
                MachineStatus myUserControl = new MachineStatus(words);
                myUserControl.Height = 90;
                myUserControl.Width = 200;
                myUserControl.Margin = new Thickness(10,0,0,5);
                // Thêm UserControl vào Grid
                wrpMain.Children.Add(myUserControl);
            }

        }
    }
}
