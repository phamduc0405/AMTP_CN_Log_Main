using ACO2_App._0;
using HPSocket.Sdk;
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
using System.Windows.Shapes;

namespace MTP.Views.Popup
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        private Controller _controller;

        public LoginView()
        {
            InitializeComponent();
            _controller = MainWindow.Controller;
            btnOperator.IsChecked = true;
            CreaterEvents();

        }
        private void CreaterEvents()
        {
            btnEsc.Click += (s, e) =>
            {
                this.Close();
            };
            btnLogIn.Click += async (s, e) =>
            {

                if (txtPass.passbox.Password == _controller.ControllerConfig.AdminPass && btnAdmin.IsChecked == true)
                {
                    MainWindow.UserLogin = 0;
                    _controller.UserLevelChange(0);
                    this.Close();
                    return;
                }
                if (txtPass.passbox.Password == _controller.ControllerConfig.OperatorPass && btnOperator.IsChecked == true)
                {
                    MainWindow.UserLogin = 1;
                    _controller.UserLevelChange(1);
                    this.Close();
                    return;
                }
                if (txtPass.passbox.Password == _controller.ControllerConfig.EngineerPass && btnEngineer.IsChecked == true)
                {
                    MainWindow.UserLogin = 2;
                    _controller.UserLevelChange(2);
                    this.Close();
                    return;
                }
            };
            btnLogOut.Click += (s, e) =>
            {
                MainWindow.UserLogin = 1;
                _controller.UserLevelChange(1);
                this.Close();
                return;
            };

        }
    }
}
