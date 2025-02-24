using ACO2_App._0.Model;
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
using System.Windows.Threading;
using ACO2_App._0.INIT;
using System.Reflection;

namespace MTP.Views.Config
{
    /// <summary>
    /// Interaction logic for EquipmentConfigView.xaml
    /// </summary>
    public partial class EquipmentConfigView : UserControl
    {
        private Controller _controller;
        private ControllerConfig _controllerConfig;
        private ControllerConfig _tempController;
        private EquipmentConfig _tempEqpConfig;
        private int _tempEqpID = 0;
        private EquipmentConfig _currentEqp;
        public EquipmentConfigView()
        {
            _controller = MainWindow.Controller;
            _controllerConfig = _controller.ControllerConfig;
            _tempController = new ControllerConfig();
            _tempEqpConfig = new EquipmentConfig();
            InitializeComponent();
            Initial();
            CreaterEven();
        }
        private void CreaterEven()
        {
            Loaded += async (s, e) =>
            {
            };
            btnSaveConfig.Click += async (s, e) =>
            {
                try
                {
                    LoadingImage.Visibility = Visibility.Visible;
                    foreach (var item in stkEqp.Children)
                    {
                        var partial = item as PartialNameView;
                        if (partial != null)
                        {
                            var eqp = _tempController.EqpConfigs.FirstOrDefault(x => x.EQPIndex == partial._index);
                            if (eqp != null)
                            {
                                eqp.EQPID = partial.txtID.Text;
                                eqp.EqpName = partial.txtName.Text;
                            }
                        }
                    }
                    _controllerConfig.EqpConfigs = _tempController.EqpConfigs;
                    _controller.SaveControllerConfig(); 
                    await LoadConfig();
                }
                catch (Exception ex)
                {
                    var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", MethodBase.GetCurrentMethod().DeclaringType.Name.ToString(), MethodBase.GetCurrentMethod().Name, ex.Message);
                    LogTxt.Add(LogTxt.Type.Exception, debug);
                }
                LoadingImage.Visibility = Visibility.Hidden;
            };
            cbbChannelNo.SelectionChanged += async (s, e) =>
            {
                if (_currentEqp == null || cbbChannelNo.SelectedIndex < 0)
                    return; 
                if (_currentEqp.Channels.Count > cbbChannelNo.SelectedIndex)
                {
                    grdChannel.Children.Clear(); 
                    PartialChannelConfigView mc = new PartialChannelConfigView(_currentEqp.Channels[cbbChannelNo.SelectedIndex],$"{_currentEqp.Channels[cbbChannelNo.SelectedIndex].ChannelNo}");
                    grdChannel.Children.Add(mc);
                }
            };
        }
        private void UpdateChannelComboBox(EquipmentConfig eqp)
        {
            grdChannel.Children.Clear();

            cbbChannelNo.Items.Clear(); 

            int channelCount = eqp.Channels.Count > 0 ? eqp.Channels.Count : 12;

            for (int i = 1; i <= channelCount; i++)
            {
                cbbChannelNo.Items.Add($"{eqp.Channels[i-1].ChannelNo}");
            }

            cbbChannelNo.SelectedIndex = 0;
        }
        private async Task LoadConfig()
        {
            stkEqp.Children.Clear(); 

            foreach (var eqp in _controllerConfig.EqpConfigs)
            {
                AddNewLine(eqp);
            }
        }
        private void Initial()
        {
            stkEqp.Children.Clear();
            foreach (var eqp in _controllerConfig.EqpConfigs)
            {
                AddNewLine(eqp);
                _tempController.EqpConfigs.Add(eqp);
            }
            if (_tempController.EqpConfigs.Count == 0)
            {
                EquipmentConfig e = new EquipmentConfig() { EQPIndex = 1 };
                AddNewLine(e);
                _tempController.EqpConfigs.Add(e);
            }
        }

        private void AddNewLine(EquipmentConfig eqp, int index = 0)
        {
            PartialNameView EQPSet = new PartialNameView(eqp.EQPIndex, "EQP NAME : ");
            EQPSet.txtID.Text = eqp.EQPID;
            EQPSet.txtName.Text = eqp.EqpName;
            EQPSet.TxtIDRequested += LineSet_TxtIDRequested;
            EQPSet.TxtNameRequested += LineSet_TxtNameRequested;
            if (index > 0)
            {
                stkEqp.Children.Insert(index, EQPSet);
                _tempController.EqpConfigs.Insert(index, eqp);
                return;
            }
            stkEqp.Children.Add(EQPSet);
        }

        private void LineSet_TxtIDRequested(object sender, EventArgs e)
        {
            var partial = sender as PartialNameView;
            if (partial == null) return;

            // Nếu thiết bị đã được chọn, không cần cập nhật UI
            if (_tempEqpID == partial._index) return;

            // Lưu phần tử đang focus để phục hồi sau khi UI cập nhật
            var focusedElement = Keyboard.FocusedElement;

            // Xóa giao diện cũ
            grdChannel.Children.Clear();
            grdTCP.Children.Clear();

            if (_tempController.EqpConfigs.Any(_ => _.EQPIndex == partial._index))
            {
                ShowEQPConfig(_tempController.EqpConfigs.FirstOrDefault(_ => _.EQPIndex == partial._index));
            }

            _tempEqpID = partial._index;

            foreach (var eqp in stkEqp.Children)
            {
                var p = eqp as PartialNameView;
                p.brdMain.Background = Brushes.Transparent;
            }
            partial.brdMain.Background = Brushes.CornflowerBlue;

            // Phục hồi focus sau khi cập nhật giao diện
            Dispatcher.BeginInvoke((Action)(() =>
            {
                focusedElement?.Focus();
            }), DispatcherPriority.Input);
        }

        private void LineSet_TxtNameRequested(object sender, EventArgs e)
        {
            var partial = sender as PartialNameView;
            if (partial == null) return;

            // Nếu thiết bị đã được chọn, không cần cập nhật UI
            if (_tempEqpID == partial._index) return;

            // Lưu phần tử đang focus để phục hồi sau khi UI cập nhật
            var focusedElement = Keyboard.FocusedElement;

            // Xóa giao diện cũ
            grdChannel.Children.Clear();
            grdTCP.Children.Clear();

            if (_tempController.EqpConfigs.Any(_ => _.EQPIndex == partial._index))
            {
                ShowEQPConfig(_tempController.EqpConfigs.FirstOrDefault(_ => _.EQPIndex == partial._index));
            }

            _tempEqpID = partial._index;

            foreach (var eqp in stkEqp.Children)
            {
                var p = eqp as PartialNameView;
                p.brdMain.Background = Brushes.Transparent;
            }
            partial.brdMain.Background = Brushes.CornflowerBlue;

            // Phục hồi focus sau khi cập nhật giao diện
            Dispatcher.BeginInvoke((Action)(() =>
            {
                focusedElement?.Focus();
            }), DispatcherPriority.Input);
        }

        private void ShowEQPConfig(EquipmentConfig eqp)
        {
            _currentEqp = eqp;
            PartialTCPConfigView tcp = new PartialTCPConfigView(eqp);
            tcp.SaveEqpEvent += TCP_SaveEqpEvent;
            grdTCP.Children.Add(tcp);
            UpdateChannelComboBox(eqp);
        }

        private void TCP_SaveEqpEvent(EquipmentConfig eqpConfig)
        {
            GetName();
            _tempController.EqpConfigs.RemoveAll(x => string.IsNullOrEmpty(x.EQPID));
            var eqp = _tempController.EqpConfigs.First(x => x.EQPIndex.Equals(_tempEqpID));
            _controllerConfig.EqpConfigs = _tempController.EqpConfigs;
            _controller.SaveControllerConfig();
        }
        private void GetName()
        {
            foreach (var line in stkEqp.Children)
            {
                var par = line as PartialNameView;
                if (par != null)
                {
                    if (_tempController.EqpConfigs.Any(x => x.EQPIndex == par._index))
                    {
                        _tempController.EqpConfigs.First(x => x.EQPIndex == par._index).EQPID = par.txtID.Text;
                    }
                }
            }

        }
    }
}
