using APlc;
using ACO2_App._0.INIT;
using ACO2_App._0.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
using System.Xml.Linq;
using Application = System.Windows.Application;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using TextBox = System.Windows.Controls.TextBox;
using Button = System.Windows.Controls.Button;
using Orientation = System.Windows.Controls.Orientation;
using System.IO.Ports;
using ACO2.Views.Config;
using static APlc.PlcComm;
using ACO2_App.Style;

namespace ACO2_App._0.Views
{
    /// <summary>
    /// Interaction logic for ConfigView.xaml
    /// </summary>
    public partial class ConfigView : System.Windows.Controls.UserControl
    {
        #region Field

        private Controller _controller;
        private ControllerConfig _controllerConfig;
        EquipmentConfig _eqpConfig;
        IPAddressTextBox ipPLCTextBox = new IPAddressTextBox();
        IPAddressTextBox ipPCTextBox = new IPAddressTextBox();

        #endregion


        #region Property

        #endregion
        #region Event

        #endregion
        #region Constructor
        public ConfigView()
        {
            InitializeComponent();
            _controller = MainWindow.Controller;

            Initial();
            CreaterEven();

        }
        #endregion
        #region Private Void
        private void Initial()
        {
            //for (int i = 0; i < 4; i++)
            //{
            //    PartialSQLString sql = new PartialSQLString();
            //    _partialSQLString.Add(sql);
            //    stkSqlConfig.Children.Add(sql);
            //}
        }
        private void CreaterEven()
        {
            Loaded += async (s, e) =>
            {
                try
                {
                    grdPLCIP.Children.Add(ipPLCTextBox);
                    grdPCIP.Children.Add(ipPCTextBox);

                    _controllerConfig = _controller.ControllerConfig;
                    if (_controllerConfig == null)
                    {
                        _controllerConfig = new ControllerConfig();
                    }
                    cbbplcConnectType.ItemsSource = Enum.GetValues(typeof(PlcConnectType));
                    cbbplcConnectType.SelectedIndex = 0;
                    await LoadConfig();
                }
                catch (Exception ex)
                {
                    // _controller.DisplayMessage(false, "Check Input Type!");
                    var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", MethodBase.GetCurrentMethod().DeclaringType.Name.ToString(), MethodBase.GetCurrentMethod().Name, ex.Message);
                    LogTxt.Add(LogTxt.Type.Exception, debug);
                }

            };
            btnDirPLCExcel.Click += (s, e) =>
            {
                LibMethod.SelectFile(LibMethod.extension.excel, txtPathPlcExcel);

                var debug = string.Format("Class:{0} Method:{1} Event:{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ((System.Windows.Controls.Control)s).Name);
                LogTxt.Add(LogTxt.Type.UI, debug);
            };
            btnSavePlcConfig.Click += async (s, e) =>
            {
                try
                {
                    LoadingPlcImage.Visibility = Visibility.Visible;

                    _controllerConfig.PLCConfig = new PLCConfig()
                    {
                        Channel = short.Parse(txtPLCChannel.Text),
                        NetworkNo = short.Parse(txtPLCNetWork.Text),
                        Path = int.Parse(txtPLCPath.Text),
                        StationNo = int.Parse(txtPLCStation.Text),
                       

                        ReadStartBitAddress = txtPLCStartInBAdd.Text,
                        SizeReadBit = int.Parse(txtPLCLengthInB.Text),
                        ReadStartWordAddress = txtPLCStartInWAdd.Text,
                        SizeReadWord = int.Parse(txtPLCLengthInW.Text),

                        WriteStartBitAddress = txtPLCStartOutB.Text,
                        SizeWriteBit = int.Parse(txtPLCLengthOutB.Text),
                        WriteStartWordAddress = txtPLCStartOutW.Text,
                        SizeWriteWord = int.Parse(txtPLCLengthOutW.Text),

                        PlcConnectType = (PlcConnectType)cbbplcConnectType.SelectedItem,

                        PortPlc = int.Parse(txtPLCPort.Text),
                        IpPlc = ipPLCTextBox.FullIpAddress,
                        IpPc = ipPCTextBox.FullIpAddress,
                    };
                    if (File.Exists(txtPathPlcExcel.Text))
                    {
                        await _controllerConfig.PLCHelper.LoadExcel(txtPathPlcExcel.Text);
                    }
                    if (_controllerConfig.PLCHelper.PlcMemms?.Count > 0)
                    {
                        if (_controllerConfig.PLCHelper.Bits.Any(x => x.Item == "ALIVE"))
                        {
                            BitModel bAlive = _controllerConfig.PLCHelper.Bits.FirstOrDefault(x => x.Item == "ALIVE");
                            if (_controllerConfig.PLCHelper.PlcMemms.Any(x => x.BPLCStart == bAlive.PLCHexAdd))
                            {
                                PlcMemmory plcmem = _controllerConfig.PLCHelper.PlcMemms.FirstOrDefault(x => x.BPLCStart == bAlive.PLCHexAdd);

                                _controllerConfig.PLCConfig.ReadStartBitAddress = plcmem.BPLCStart;
                                _controllerConfig.PLCConfig.SizeReadBit = int.Parse(plcmem.BPLCPoints);
                                _controllerConfig.PLCConfig.ReadStartWordAddress = plcmem.WPLCStart;
                                _controllerConfig.PLCConfig.SizeReadWord = int.Parse(plcmem.WPLCPoints);

                                _controllerConfig.PLCConfig.WriteStartBitAddress = plcmem.BPCStart;
                                _controllerConfig.PLCConfig.SizeWriteBit = int.Parse(plcmem.BPCPoints);
                                _controllerConfig.PLCConfig.WriteStartWordAddress = plcmem.WPCStart;
                                _controllerConfig.PLCConfig.SizeWriteWord = int.Parse(plcmem.WPCPoints);

                                _controllerConfig.PLCConfig.BitDevice = plcmem.BitDevice;
                                _controllerConfig.PLCConfig.WordDevice = plcmem.WordDevice;


                            }
                        }

                    }

                    var debug = string.Format("Class:{0} Method:{1} Event:{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ((System.Windows.Controls.Control)s).Name);
                    LogTxt.Add(LogTxt.Type.UI, debug);
                    await SaveConfig();

                    await LoadConfig();
                }
                catch (Exception ex)
                {
                    //  _controller.DisplayMessage(false, "Check Input Type!");
                    var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", MethodBase.GetCurrentMethod().DeclaringType.Name.ToString(), MethodBase.GetCurrentMethod().Name, ex.Message);
                    LogTxt.Add(LogTxt.Type.Exception, debug);
                }
                LoadingPlcImage.Visibility = Visibility.Hidden;
            };
            btnSaveConfig.Click += async (s, e) =>
            {
                LoadingImage.Visibility = Visibility.Visible;
                _controllerConfig.EQPID = txtNameMachine.Text;
                _controllerConfig.PathLog = txtPathLog.Text;
                _controllerConfig.DelLog = int.Parse(txtTimeDelLog.Text);
                _controllerConfig.AdminPass = txtAdminPass.Text;
                _controllerConfig.OperatorPass = txtOperatorPass.Text;
                _controllerConfig.EngineerPass = txtEngineerPass.Text;

                await SaveConfig();
                LoadingImage.Visibility = Visibility.Hidden;

                var debug = string.Format("Class:{0} Method:{1} Event:{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ((System.Windows.Controls.Control)s).Name);
                LogTxt.Add(LogTxt.Type.UI, debug);
            };
            btnDirLog.Click += (s, e) =>
            {
                FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
                folderBrowser.Description = "Chọn thư mục";

                // Hiển thị cửa sổ dialog và lấy đường dẫn thư mục nếu người dùng chọn
                DialogResult result = folderBrowser.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string folderPath = folderBrowser.SelectedPath;
                    txtPathLog.Text = folderPath;
                }
            };
            btnTest.Click += async (s, e) =>
            {
                LoadingTestImage.Visibility = Visibility.Visible;
                await _controller.TestLog();

                LoadingTestImage.Visibility = Visibility.Hidden;

                var debug = string.Format("Class:{0} Method:{1} Event:{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ((System.Windows.Controls.Control)s).Name);
                LogTxt.Add(LogTxt.Type.UI, debug);
            };
        }
        private async Task SaveConfig()
        {
            await Task.Run(async () =>
            {
                _controller.ControllerConfig = _controllerConfig;
                _controller.SaveControllerConfig();
            });

        }
        private async Task LoadConfig()
        {
            await Task.Run(() =>
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        {
                            txtNameMachine.Text = _controllerConfig.EQPID.ToString();
                            txtPathLog.Text = _controllerConfig.PathLog.ToString();
                            txtTimeDelLog.Text = _controllerConfig.DelLog.ToString();
                            txtAdminPass.Text = _controllerConfig.AdminPass.ToString();
                            txtOperatorPass.Text = _controllerConfig.OperatorPass.ToString();
                            txtEngineerPass.Text = _controllerConfig.EngineerPass.ToString();
                        }
                        //PlcConfig
                        {
                            txtPLCChannel.Text = _controllerConfig.PLCConfig.Channel.ToString();
                            txtPLCNetWork.Text = _controllerConfig.PLCConfig.NetworkNo.ToString();
                            txtPLCPath.Text = _controllerConfig.PLCConfig.Path.ToString();
                            txtPLCStation.Text = _controllerConfig.PLCConfig.StationNo.ToString();
                          
                            stkCCLinkIe.Visibility = _controllerConfig.PLCConfig.IsCCLinkIe ? Visibility.Visible : Visibility.Collapsed;


                            txtPLCStartInBAdd.Text = _controllerConfig.PLCConfig.ReadStartBitAddress.ToString();
                            txtPLCLengthInB.Text = _controllerConfig.PLCConfig.SizeReadBit.ToString();
                            txtPLCStartInWAdd.Text = _controllerConfig.PLCConfig.ReadStartWordAddress.ToString();
                            txtPLCLengthInW.Text = _controllerConfig.PLCConfig.SizeReadWord.ToString();

                            txtPLCStartOutB.Text = _controllerConfig.PLCConfig.WriteStartBitAddress.ToString();
                            txtPLCLengthOutB.Text = _controllerConfig.PLCConfig.SizeWriteBit.ToString();
                            txtPLCStartOutW.Text = _controllerConfig.PLCConfig.WriteStartWordAddress.ToString();
                            txtPLCLengthOutW.Text = _controllerConfig.PLCConfig.SizeWriteWord.ToString();

                            cbbplcConnectType.SelectedItem = _controllerConfig.PLCConfig.PlcConnectType;
                            var ipPlcSegments = _controllerConfig.PLCConfig.IpPlc.Split('.');
                            if (ipPlcSegments.Length == 4)
                            {
                                ipPLCTextBox.FirstSegment = ipPlcSegments[0];
                                ipPLCTextBox.SecondSegment = ipPlcSegments[1];
                                ipPLCTextBox.ThirdSegment = ipPlcSegments[2];
                                ipPLCTextBox.LastSegment = ipPlcSegments[3];
                            }

                            var ipPcSegments = _controllerConfig.PLCConfig.IpPc.Split('.');
                            if (ipPcSegments.Length == 4)
                            {
                                ipPCTextBox.FirstSegment = ipPcSegments[0];
                                ipPCTextBox.SecondSegment = ipPcSegments[1];
                                ipPCTextBox.ThirdSegment = ipPcSegments[2];
                                ipPCTextBox.LastSegment = ipPcSegments[3];
                            }
                            txtPLCPort.Text = _controllerConfig.PLCConfig.PortPlc.ToString();
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

        #endregion

    }
}
