using ACO2_App._0.INIT;
using ACO2_App._0;
using MTP.Model;
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
using System.IO;
using MTP.Views.Popup;

namespace MTP.Views.Config
{
    /// <summary>
    /// Interaction logic for ConfigDefectCodeView.xaml
    /// </summary>
    public partial class ConfigDefectCodeView : UserControl
    {
        Controller _controller;
        private List<DefectCode> _listDefectCode;
        private List<DefectCode> _tempDefectCode;
        public List<DefectCode> ListDefectCodes
        {
            get
            {
                return _listDefectCode;
            }
        }
        public ConfigDefectCodeView()
        {
            InitializeComponent();
            this.DataContext = this;
            _controller = MainWindow.Controller;
            _tempDefectCode = new List<DefectCode>();
            _listDefectCode = new List<DefectCode>();
            _listDefectCode = _controller.DefectCodes.ToList();
            LoadListView();
            CreateEvent();
        }
        void CreateEvent()
        {
            btnCheckResult.Click += (s, e) =>
            {
                LoadListView(txtResult.Text);
            };
            btnDirLog.Click += (s, e) =>
            {
                btnSave.IsEnabled = false;
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.DefaultExt = ".ini";
                dlg.Filter = "INI files (*.ini)|*.ini|All files (*.*)|*.*";
                Nullable<bool> result = dlg.ShowDialog();
                if (result == true)
                {
                    string filename = dlg.FileName;
                    txtPathFile.Text = filename;
                }
                LogTxt.Add(LogTxt.Type.UI, "DefectCode DirLog Button");

            };
            btnLoad.Click += (s, e) =>
            {

                if (File.Exists(txtPathFile.Text))
                {
                    DefectCodeLoad(txtPathFile.Text);
                    lvDefectCode.Items.Clear();
                    foreach (var code in _listDefectCode)
                    {
                        lvDefectCode.Items.Add(code);
                    }
                }
                LogTxt.Add(LogTxt.Type.UI, "DefectCode Load Button");
            };
            btnSave.Click += (s, e) =>
            {
                Task.Run(() =>
                {

                    DefectCodeSave();
                });

            };
            txtResult.KeyUp += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    LoadListView(txtResult.Text);
                }

            };
        }
        private async Task DefectCodeSave()
        {
            Dispatcher.Invoke(new Action(async () =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
                // Hiển thị ProgressBar
                progressBar.Visibility = Visibility.Visible;
                Task.Run(async () =>
                {
                    Dispatcher.Invoke(new Action(async () =>
                    {
                        for (int i = 0; i <= 100; i++)
                        {
                            progressBar.Value = i;
                            await Task.Delay(250); // Đợi một khoảng thời gian nhỏ trước khi tiếp tục
                        }
                        progressBar.Visibility = Visibility.Collapsed;
                    }));
                });

                var result = await _controller.SaveDefectConfig(_listDefectCode);
                if (result) _controller.DefectCodes = _listDefectCode;
                progressBar.Visibility = Visibility.Collapsed;
                //   MainWindow.DisplayMessage(false, "DEFECT CODE UPDATED !");
                LogTxt.Add(LogTxt.Type.UI, "DefectCode Save Button : UPDATED ");
                Mouse.OverrideCursor = null;
            }));

        }
        private void DefectCodeLoad(string path)
        {
            List<DefectCode> lst = _controller.ReadDefectConfig(path);
            btnSave.IsEnabled = true;
            _listDefectCode = new List<DefectCode>();
            _listDefectCode = lst;
        }
        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LogTxt.Add(LogTxt.Type.UI, "DefectCode ListViewItem_PreviewMouseLeftButtonDown");
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {

                var content = item.Content as DefectCode;
                //      var id = selItem["columnName"];

                ChangeDefectCodeView editPopup = new ChangeDefectCodeView(content);
                editPopup.ShowDialog();
                editPopup.Topmost = true;
                if ((bool)editPopup.DialogResult)
                {
                    Console.WriteLine(content.Index);
                    DefectCode df = _listDefectCode.FirstOrDefault(x => x.Index == content.Index);
                    int index = _listDefectCode.IndexOf(df);
                    if (index != -1)
                    {
                        _listDefectCode[index] = editPopup.Defectcode;
                        LoadListView(txtResult.Text);
                    }
                }

            }
        }

        private void LoadListView(string search = "")
        {
            if (search == "" || string.IsNullOrWhiteSpace(search))
            {
                _tempDefectCode = _listDefectCode;
            }
            else
            {
                _tempDefectCode = _listDefectCode.Where(x => x.DefectName.ToUpper().Contains(search.ToUpper())).ToList();
            }
            Dispatcher.Invoke(() =>
            {
                lvDefectCode.Items.Clear();
                foreach (var code in _tempDefectCode)
                {
                    lvDefectCode.Items.Add(code);
                }
            });

        }

    }
}
