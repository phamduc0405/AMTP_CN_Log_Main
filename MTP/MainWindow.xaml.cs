using ACO2.Views.Config;
using ACO2.Views.Data;
using ACO2.Views.Home;
using ACO2.Views.Popup;
using ACO2_App._0.INIT;
using ACO2_App._0.Model;
using ACO2_App._0.ViewModel;
using ACO2_App._0.Views;
using ACO2_App.Views;
using APlc;
using MTP.Views.Popup;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ACO2_App._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Controller Controller;
        private static bool _running = true;
        public static int UserLogin = 1;

        public static bool Running 
        { get
            {
                return _running;
            }
        }
        private PerformanceCounter memoryCounter;
        private Thread memoryUsageThread;
        private bool isMonitoringMemory = true;
        private PopupMessage _displayMessage;
        private MainViewModel viewModel;
        private Thread _updateTime;
        private PartialCpuChart _cpuChart;

        public MainWindow()
        {
            InitializeComponent();
            if (Controller == null)
            {
                Controller = new Controller();
            }
            Controller.Initial();
            DataContext = new MainViewModel();
            LogTxt.Start();
            Controller.InitialGetDataProduct();
            _cpuChart = new PartialCpuChart();
            grdCpu.Children.Add(_cpuChart);
            StartMemoryMonitoring();
            maincontent.Content = new HomeView();
            tblScreen.Text = "HOME";
            Controller.PlcConnectChangeEvent -= Controller_PlcConnectChangeEvent;
            Controller.PlcConnectChangeEvent += Controller_PlcConnectChangeEvent;
            Controller.Equipment[0].ConnectEvent -= TCP1_ConnectEvent;
            Controller.Equipment[0].ConnectEvent += TCP1_ConnectEvent;
            Controller.Equipment[1].ConnectEvent -= TCP2_ConnectEvent;
            Controller.Equipment[1].ConnectEvent += TCP2_ConnectEvent;
            CreateEvent();
            _updateTime = new Thread(UpdateTime)
            {
                IsBackground = true,
            };
            _updateTime.Start();
            UiHeader();
            Controller.HeaderStatusChanged += () =>
            {
                UiHeader();
            };
        }
        public void UiHeader()
        {
            if (UserLogin == 0)
            {
                txtUser.Text = "Admin";

            }
            if (UserLogin == 1)
            {
                txtUser.Text = "Operator";

            }
            if (UserLogin == 2)
            {
                 txtUser.Text = "Engineer";

            }
        }
        private void TCP1_ConnectEvent(bool isConnected)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                bdrPC1Connect.Background = isConnected ? Brushes.Green : Brushes.IndianRed;
                txtPC1Connect.Text = isConnected ? $"{Controller.ControllerConfig.EqpConfigs[0].EqpName}Connected" : $"{Controller.ControllerConfig.EqpConfigs[0].EqpName} Disconnected";
            }));
        }
        private void TCP2_ConnectEvent(bool isConnected)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                bdrPC2Connect.Background = isConnected ? Brushes.Green : Brushes.IndianRed;
                txtPC2Connect.Text = isConnected ? $"{Controller.ControllerConfig.EqpConfigs[1].EqpName}Connected" : $"{Controller.ControllerConfig.EqpConfigs[1].EqpName} Disconnected";
            }));
        }
        private void Controller_PlcConnectChangeEvent(bool isConnected)
        {
            Dispatcher.Invoke(new Action(() =>
            {
               
                bdrPlcConnect.Background = isConnected ? Brushes.Green : Brushes.IndianRed;
                txtPlcConnect.Text = isConnected ? "Plc Connected" : "Plc Disconnected";
            }));
        }
        //T: Popup Message
        private async Task<bool> PopupMessage( PopupMessage.Style style, string message)
        {
            bool result = false;
            try
            {

                if (_displayMessage == null)
                {
                    Dispatcher.Invoke(() => {
                        _displayMessage = new PopupMessage(style, message);
                        result = (bool)_displayMessage.ShowDialog();
                        // Check the DialogResult
                        _displayMessage.Closing += (sender, a) =>
                        {
                            _displayMessage = null;
                        };
                        _displayMessage.Topmost = true;
                        _displayMessage.Close();
                        _displayMessage = null;
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.Message);
                LogTxt.Add(LogTxt.Type.Exception, debug);
                return false;
            }
        }

        private void CreateEvent()
        {
            Controller.MessageDisplayEvent += async (isYesNo, message) =>
            {
                var result = await PopupMessage(isYesNo ? ACO2.Views.Popup.PopupMessage.Style.YesNo : ACO2.Views.Popup.PopupMessage.Style.Ok, message);
                return result;
            };
            MouseLeave += (sender, e) =>
            {
                if (tgMenu.IsChecked == true && !st_pnl.IsMouseOver)
                {
                    HideStackPanel.Begin();
                    tgMenu.IsChecked = false;
                }
            };
            btnClose.Click += (sender, e) =>
            {
                _running = false;
                LogTxt.Stop();
                Controller.Dispose();
                memoryUsageThread?.Abort();
                _updateTime?.Abort();
                _cpuChart.OnUnloaded();
                this.Close();
            };
            btnResize.Click += (sender, e) =>
            {
                if (this.WindowState == (WindowState)FormWindowState.Normal)
                {
                    this.WindowState = (WindowState)FormWindowState.Maximized;
                }
                else { this.WindowState = (WindowState)FormWindowState.Normal; }


            };
            btnHideMenu.Click += (sender, e) =>
            {
                this.WindowState = (WindowState)FormWindowState.Minimized;

            };
            grdTopMain.MouseDown += (sender, e) =>
            {

                DragMove();
            };

            grdPanel.MouseEnter += (sender, e) =>
            {
                if (tgMenu.IsChecked == true)
                {
                    ttT5s.Visibility = Visibility.Collapsed;
                    ttConfig.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ttT5s.Visibility = Visibility.Visible;
                    ttConfig.Visibility = Visibility.Visible;
                }
            };
            btnHome.Click += (sender, e) =>
            {
                maincontent.Content = new HomeView();
                tblScreen.Text = "HOME";
            };
            btnMonitor.Click += (sender, e) =>
            {
                if (UserLogin == 0 || UserLogin == 2)
                {
                    maincontent.Content = new MonitorIOView();
                    tblScreen.Text = "MONITOR IO";
                    //CellData cellData = new CellData();
                    //cellData.ZoneNo = "1";
                    //cellData.CellID = "ABCD123456";
                    //cellData.Channel.ChannelNo = "CH02";
                    //cellData.Channel.ContactResult = "OK";
                    //cellData.Channel.MTPWriteResult = "OK";
                    //cellData.Channel.DefectCode = "";
                    //cellData.Channel.X600 = "X600";
                    //Controller.SaveDataLog(cellData);
                }
                else
                {
                    tblScreen.Text = "You do not have sufficient permissions to access this page";

                }
            };
            btnConfig.Click += (sender, e) =>
            {
                if(UserLogin == 0)
                {
                    maincontent.Content = new ConfigMainView();
                    tblScreen.Text = "CONFIG";
                }
                else
                {
                    tblScreen.Text = "You do not have sufficient permissions to access this page";
                }

            };
            btnData.Click += (sender, e) =>
            {
                if (UserLogin == 0 || UserLogin == 2)
                {
                    maincontent.Content = new DataProductView();
                    tblScreen.Text = "DATA";
                }
                else
                {
                    tblScreen.Text = "You do not have sufficient permissions to access this page";

                }

            };
            btnInitAll.Click += (sender, e) =>
            {
                if (UserLogin == 0 || UserLogin == 2)
                {
                    List<WordModel> words = Controller.PlcH.Words.Where(x => x.IsPlc == false).ToList();
                    foreach (var word in words)
                    {
                        word.SetValue = "";
                    }
                    List<BitModel> bits = Controller.PlcH.Bits.Where(x => x.Type == "Event" || x.Type == "Command").ToList();
                    foreach (var bit in bits)
                    {
                        bit.SetPCValue = false;
                    }
                    foreach (var item in Controller.ListCellDatas.CellDatas)
                    {
                        item.Clear();
                    }
                    LogTxt.Add(LogTxt.Type.FlowRun, "[DATA]:" + $"ALL DATA INIT BY CLICK INIT BUTTON");
                    LogTxt.Add(LogTxt.Type.Status, "[DATA]:" + $"ALL DATA INIT BY CLICK INIT BUTTON");
                }
                else
                {
                    tblScreen.Text = "You do not have sufficient permissions to press this button";

                }

            };
            btn_Logout.Click += (sender, e) =>
            {
                var loginview = new LoginView();
                loginview.Closed += (o, a) =>
                {
                    this.Visibility = Visibility.Visible;
                };

                loginview.Show();
                this.Visibility = Visibility.Hidden;

            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            txtVersion.Text = $"Version {version}";
            btnHome.IsChecked = true;
        }
        private void StartMemoryMonitoring()
        {
            memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
            memoryUsageThread = new Thread(() =>
            {
                while (isMonitoringMemory)
                {
                    Thread.Sleep(1000);
                    UpdateMemoryUsage();
                }
            });
            memoryUsageThread.IsBackground = true;
            memoryUsageThread.Start();
        }

        private void UpdateMemoryUsage()
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    double totalMemory = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / (1024.0 * 1024.0);
                    double availableMemory = memoryCounter.NextValue();
                    double usedMemory = totalMemory - availableMemory;
                    double usagePercentage = (usedMemory / totalMemory) * 100;
                    txtMemoryUsage.Text = $"Memory Usage: {usedMemory:F1} MB / {totalMemory:F1} MB ({usagePercentage:F1}%)";
                }
                catch (Exception ex)
                {
                    txtMemoryUsage.Text = "Error retrieving memory info.";
                    Debug.WriteLine($"Error: {ex.Message}");
                }
            });
        }
       
        private void UpdateTime()
        {
            while (_running)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    tblDateTime.Text = DateTime.Now.ToString();
                }));
                Thread.Sleep(100);
            }

        }
    }
}
