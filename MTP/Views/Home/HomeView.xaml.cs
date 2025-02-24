using ACO2_App._0;
using System;
using System.Windows.Controls;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using ACO2_App._0.INIT;
using MTP.Views.Home;
using System.Windows;
using System.Windows.Threading;
using Mitsu3E;

namespace ACO2.Views.Home
{
    /// <summary>
    /// Interaction logic for HomeView1.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        private Controller _controller;
        private Equipment _equipment;
        private Thread _updateTime;
        private DispatcherTimer _updateTimer;

        public HomeView()
        {
            InitializeComponent();
            _controller = MainWindow.Controller;

            _updateTime = new Thread(UpdateTime)
            {
                IsBackground = true,
            };
            _updateTime.Start();
            _controller.MachineStatus.Status = "MANUAL";
            _controller.MachineStatus.PropertyChanged += MachineStatus_PropertyChanged;
        
            CreateEvent();
            LoadEquipments();
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _updateTimer.Tick += UpdateEquipmentUI;
            _updateTimer.Start();
        }

        private void MachineStatus_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                switch (e.PropertyName)
                {
                    case "Status":
                        txtState.Text = _controller.MachineStatus.Status;
                        tblState.Background = GetBackgroundColor(_controller.MachineStatus.Status);
                        txtState.Foreground = GetTextColor(_controller.MachineStatus.Status);
                        break;

                    case "AvailabilityState":
                        txtAvailability.Text = _controller.MachineStatus.AvailabilityState;
                        tblAvailability.Background = GetBackgroundColor(_controller.MachineStatus.AvailabilityState);
                        txtAvailability.Foreground = GetTextColor(_controller.MachineStatus.AvailabilityState);
                        break;

                    case "InterlockState":
                        txtInterlock.Text = _controller.MachineStatus.InterlockState;
                        tblInterlock.Background = GetBackgroundColor(_controller.MachineStatus.InterlockState);
                        txtInterlock.Foreground = GetTextColor(_controller.MachineStatus.InterlockState);
                        break;

                    case "MoveState":
                        txtMoveState.Text = _controller.MachineStatus.MoveState;
                        tblMoveState.Background = GetBackgroundColor(_controller.MachineStatus.MoveState);
                        txtMoveState.Foreground = GetTextColor(_controller.MachineStatus.MoveState);
                        break;

                    case "RunState":
                        txtRunState.Text = _controller.MachineStatus.RunState;
                        tblRunState.Background = GetBackgroundColor(_controller.MachineStatus.RunState);
                        txtRunState.Foreground = GetTextColor(_controller.MachineStatus.RunState);
                        break;
                }
            });
        }

        private SolidColorBrush GetBackgroundColor(string state)
        {
            if (state == "AUTO") return new SolidColorBrush(Colors.Green);
            if (state == "MANUAL") return new SolidColorBrush(Colors.Yellow);
            if (state == "ERROR") return new SolidColorBrush(Colors.Red);
            if (state == "UP") return new SolidColorBrush(Colors.Green);
            if (state == "DOWN") return new SolidColorBrush(Colors.Red);
            if (state == "ON") return new SolidColorBrush(Colors.Red);
            if (state == "OFF") return new SolidColorBrush(Colors.Green);
            if (state == "RUNNING") return new SolidColorBrush(Colors.Blue);
            if (state == "PAUSE") return new SolidColorBrush(Colors.Yellow);
            if (state == "RUN") return new SolidColorBrush(Colors.Blue);
            if (state == "IDLE") return new SolidColorBrush(Colors.Yellow);

            return new SolidColorBrush(Colors.Gray);
        }


        private SolidColorBrush GetTextColor(string state)
        {
            if (state == "AUTO") return new SolidColorBrush(Colors.White);   // Trắng trên nền xanh lá
            if (state == "MANUAL") return new SolidColorBrush(Colors.Black); // Đen trên nền vàng
            if (state == "ERROR") return new SolidColorBrush(Colors.White);  // Trắng trên nền đỏ
            if (state == "UP") return new SolidColorBrush(Colors.White);     // Trắng trên nền xanh lá
            if (state == "DOWN") return new SolidColorBrush(Colors.White);   // Trắng trên nền đỏ
            if (state == "ON") return new SolidColorBrush(Colors.White);     // Trắng trên nền đỏ
            if (state == "OFF") return new SolidColorBrush(Colors.Black);    // Đen trên nền xanh lá
            if (state == "RUNNING") return new SolidColorBrush(Colors.White);// Trắng trên nền xanh dương
            if (state == "PAUSE") return new SolidColorBrush(Colors.Black);  // Đen trên nền vàng
            if (state == "RUN") return new SolidColorBrush(Colors.White);    // Trắng trên nền xanh dương
            if (state == "IDLE") return new SolidColorBrush(Colors.Black);   // Đen trên nền vàng

            return new SolidColorBrush(Colors.Black); 
        }

        private void CreateEvent()
        {
            txtMachineName.TextChanged += (s, e) =>
            {
                if (_controller == null || _controller.ControllerConfig == null)
                    return;
                string newText = txtMachineName.Text.Trim();
                string currentEQPID = _controller.ControllerConfig.EQPID.Trim();
                if (!string.IsNullOrEmpty(newText) && newText != currentEQPID)
                {
                    _controller.ControllerConfig.EQPID = newText;
                    _controller.SaveControllerConfig();
                    LogTxt.Add(LogTxt.Type.Status, $"[CONFIG] EQPNAME Changed: {currentEQPID} ➝ {newText}");
                }
            };
            Unloaded += (s, e) =>
            {
               _updateTime.Abort();       
            };
            Loaded += (s, e) =>
            {
                    {
                        tblNameEq.Text = _controller.ControllerConfig.EqpConfigs[0].EqpName;
                        txtIPPc1.Text = _controller.ControllerConfig.EqpConfigs[0].PCSignalIPAddress;
                        txtPortPc1.Text = _controller.ControllerConfig.EqpConfigs[0].PCSignalPort.ToString();
                    }
                    {
                        tblNameEq2.Text = _controller.ControllerConfig.EqpConfigs[1].EqpName;
                        txtIPPc2.Text = _controller.ControllerConfig.EqpConfigs[1].PCSignalIPAddress;
                        txtPortPc2.Text = _controller.ControllerConfig.EqpConfigs[1].PCSignalPort.ToString();
                    }
                {
                    txtMachineName.Text = _controller.ControllerConfig.EQPID;
                }
            };
            
        }
        private void UpdateEquipmentUI(object sender, EventArgs e)
        {
            foreach (var child in grdMain.Children)
            {
                if (child is EquipmentControl equipControl)
                {
                    equipControl.UpdateUI();
                }
            }
        }
        private void LoadEquipments()
        {
            grdMain.Children.Clear();
            grdMain.ColumnDefinitions.Clear();

            int equipmentCount = _controller.Equipment.Count;
            if (equipmentCount == 0) return;
            for (int i = 0; i < equipmentCount; i++)
            {
                grdMain.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            for (int i = 0; i < equipmentCount; i++)
            {
                var equipment = _controller.Equipment[i];
                var equipControl = new EquipmentControl(equipment);

                Grid.SetColumn(equipControl, i);
                grdMain.Children.Add(equipControl);
            }
        }
        private void UpdateTime()
        {
            while (true)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    _controller.MachineStatus.Status = "AUTO";
                    _controller.MachineStatus.AvailabilityState = "DOWN";
                    _controller.MachineStatus.InterlockState = "ON";
                    _controller.MachineStatus.MoveState = "PAUSE";
                    _controller.MachineStatus.RunState = "IDLE";
                   //_controller.test();
                }));
                Thread.Sleep(500);
            }

        }
    }
}
