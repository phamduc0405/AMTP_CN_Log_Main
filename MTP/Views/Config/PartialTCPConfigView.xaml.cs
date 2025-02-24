using ACO2_App._0;
using ACO2_App._0.INIT;
using ACO2_App._0.Model;
using ACO2_App.Style;
using APlc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace MTP.Views.Config
{
    /// <summary>
    /// Interaction logic for PartialTCPConfigView.xaml
    /// </summary>
    public partial class PartialTCPConfigView : UserControl
    {
        EquipmentConfig _eqpConfig;
        Controller _controller;
        IPAddressTextBox ipPCTextBox = new IPAddressTextBox();

        #region Event
        public delegate void SaveEqpEventDelegate(EquipmentConfig eqpConfig);
        public event SaveEqpEventDelegate SaveEqpEvent;

        #endregion
        public PartialTCPConfigView(EquipmentConfig eqpConfig)
        {
            InitializeComponent();
            _controller = MainWindow.Controller;
            _eqpConfig = eqpConfig;
            Initial();
            CreaterEven();
        }
        private void Initial()
        {

        }
        private void CreaterEven()
        {
            Loaded += async (s, e) =>
            {
                try
                {
                    grdPCIP.Children.Add(ipPCTextBox);

                    await LoadConfig();
                }
                catch (Exception ex)
                {
                    // _controller.DisplayMessage(false, "Check Input Type!");
                    var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", MethodBase.GetCurrentMethod().DeclaringType.Name.ToString(), MethodBase.GetCurrentMethod().Name, ex.Message);
                    LogTxt.Add(LogTxt.Type.Exception, debug);
                }

            };
            btnSaveDataConfig.Click += async (s, e) =>
            {
                try
                {
                    LoadingDataImage.Visibility = Visibility.Visible;
                    _eqpConfig.PCSignalIPAddress = ipPCTextBox.FullIpAddress;
                    _eqpConfig.PCSignalPort = ushort.Parse(txtPcPort.Text);
                    _eqpConfig.PCSignalIsActive = tglPcActive.IsChecked == true;
                    _eqpConfig.IsLineCheck = tglLineCheck.IsChecked == true;
                    SaveEqpEventHandle(_eqpConfig);

                    var debug = string.Format("Class:{0} Method:{1} Event:{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ((Control)s).Name);
                    LogTxt.Add(LogTxt.Type.UI, debug);

                    await LoadConfig();
                }
                catch (Exception ex)
                {
                    //  _controller.DisplayMessage(false, "Check Input Type!");
                    var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", MethodBase.GetCurrentMethod().DeclaringType.Name.ToString(), MethodBase.GetCurrentMethod().Name, ex.Message);
                    LogTxt.Add(LogTxt.Type.Exception, debug);
                }
                LoadingDataImage.Visibility = Visibility.Hidden;
            };
        }
        private async Task LoadConfig()
        {
            await Task.Run(() =>
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        //TCPConfig
                        {
                            var ipPcSegments = _eqpConfig.PCSignalIPAddress.ToString().Split('.');
                            if (ipPcSegments.Length == 4)
                            {
                                ipPCTextBox.FirstSegment = ipPcSegments[0];
                                ipPCTextBox.SecondSegment = ipPcSegments[1];
                                ipPCTextBox.ThirdSegment = ipPcSegments[2];
                                ipPCTextBox.LastSegment = ipPcSegments[3];
                            }
                            txtPcPort.Text = _eqpConfig.PCSignalPort.ToString();
                            tglPcActive.IsChecked = _eqpConfig.PCSignalIsActive;
                            tglLineCheck.IsChecked = _eqpConfig.IsLineCheck;
                        }

                    });
                }
                catch (Exception ex)
                {
                    var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", MethodBase.GetCurrentMethod().DeclaringType.Name.ToString(), MethodBase.GetCurrentMethod().Name, ex.Message);
                    LogTxt.Add(LogTxt.Type.Exception, debug);
                }

            });
        }
        #region EventHandle
        private void SaveEqpEventHandle(EquipmentConfig eqpConfig)
        {
            var handle = SaveEqpEvent;
            if (handle != null)
            {
                handle(eqpConfig);
            }
        }
        #endregion

    }
}
