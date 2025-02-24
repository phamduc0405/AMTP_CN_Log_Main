using ACO2.Model;
using ACO2.Views.Popup;
using ACO2_App._0;
using ACO2_App._0.INIT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ACO2.Views.Config
{
    /// <summary>
    /// Interaction logic for ConfigModelView.xaml
    /// </summary>
    public partial class ConfigModelView : UserControl
    {
        private Controller _controller;
        private List<PartialEqpView> _partialEqpViews;
        private ModelName _modelSelect;
        private List<string> _listModelName;
        public ConfigModelView()
        {
            InitializeComponent();
            CreateEvent();
            _controller = MainWindow.Controller;
            _controller.MainBarcodeEvent += _controller_MainBarcodeEvent;
            Initial();
            
            LoadListView();
        }


        private void _controller_MainBarcodeEvent(string code)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
               txtSearch.Text = code;
                LoadListView(code);
            }));
           
        }

        private void CreateEvent()
        {
            lstModel.SelectionChanged += (s, e) =>
            {
                if (lstModel.SelectedItem == null)
                    return;
                var a = lstModel.SelectedItem.ToString().Split(':')[1].Trim();
                _modelSelect = _controller.ModelConfig.ModelList.FirstOrDefault(x=>x.Name==a);

                LoadUI();
            };
            btnCreate.Click += async (s, e) =>
            {
                var result = await PopupCreate();
                if (result)
                {
                    LoadListView();
                }

                Console.WriteLine((bool)_partialEqpViews[1].tglisSkip.IsChecked ? "True" : "False");

            };

            btnDelete.Click += async (s, e) =>
            {
                var result = await _controller.PopupMessage($"Do You Want Delete Model : {txtSelectedRecipeName.Text}", true);
                if (_modelSelect.Name == _controller.ModelConfig.CurrentModel.Name)
                {
                    _controller.PopupMessage($"Cannot Delete Current Model!");
                    return;
                }
                if (result)
                {
                    _controller.ModelConfig.ModelList.Remove(_modelSelect);
                    LoadListView();
                }
            };
            btnApply.Click += async (s, e) =>
            {
                if (CheckError())
                {
                    _controller.PopupMessage($"Cannot Apply! \nPlease Check Format Input!");
                    return;
                }
                var confirm = await PopupMessage(GetModelFromUI());
                if (!confirm) return;
                var result = await _controller.PopupMessage($"Do You Want Change To Model : {txtSelectedRecipeName.Text}", true);
                if (result)
                {
                    _controller.ModelConfig.CurrentModel = _modelSelect;
                    LoadListView();
                    _controller.SetModel(_modelSelect);
                }
            };
            btnSave.Click += async (s, e) =>
            {
                if (CheckError())
                {
                    _controller.PopupMessage($"Cannot Save! \nPlease Check Format Input!");
                    return;
                }
                if (_modelSelect.Name == _controller.ModelConfig.CurrentModel.Name)
                {
                    _controller.PopupMessage($"Cannot Change Current Model!");
                    return;
                }
                var result = await _controller.PopupMessage($"Do You Want Save Model : {txtSelectedRecipeName.Text}", true);
                if (result)
                {
                    ModelName model = _controller.ModelConfig.ModelList.FirstOrDefault(x => x.Name == _modelSelect.Name);
                    int index = _controller.ModelConfig.ModelList.IndexOf(model);
                    if (index != -1)
                    {
                        _controller.ModelConfig.ModelList[index] = GetModelFromUI();
                        LoadListView();
                        _controller.SetModel(_modelSelect);
                    }
                }
            };
            btnSearch.Click += (s, e) =>
            {

                LoadListView(string.IsNullOrEmpty(txtSearch.Text) ? "" : txtSearch.Text);
            };
            btnTrigger.Click += (s, e) =>
            {
                _controller.Trigger();
            };
            txtSearch.TextChanged += (s, e) =>
            {
                Console.WriteLine($"Current text: '{txtSearch.Text}'");
                LoadListView(string.IsNullOrEmpty(txtSearch.Text) ? "" : txtSearch.Text);
            };
        }

        private void Initial()
        {
            _partialEqpViews = new List<PartialEqpView>();
            for (int i = 0; i < 6; i++)
            {
                PartialEqpView eqp = new PartialEqpView() { Header = $"Equiment #{i + 1}" };
                _partialEqpViews.Add(eqp);
                Grid.SetColumn(eqp, i % 2);
                Grid.SetRow(eqp, i / 2);
                grdMain.Children.Add(eqp);
            }

        }
        private bool CheckError()
        {
            foreach (var item in _partialEqpViews)
            {
                if (item.IsError)
                {
                    return true;
                }
            }
            return false;
        }
        private ModelName GetModelFromUI()
        {
            ModelName model = new ModelName();
            model.Name = txtSelectedRecipeName.Text.Trim();
            foreach (var item in _partialEqpViews)
            {
                ModelParameter mdPara = new ModelParameter();
                mdPara.IsSkip = item.tglisSkip.IsChecked == true;
                mdPara.IsRotary = item.tglisRotary.IsChecked == true;
                mdPara.IsUseMCR = item.tglisUseMcr.IsChecked == true;
                mdPara.IsFirstMachine = item.tglisFirstMachine.IsChecked == true;
                mdPara.IsThickness = item.tglisThickness.IsChecked == true;
                if (mdPara.IsFirstMachine)
                {
                    var value = int.TryParse(item.txtCountPaper.Text.Trim(), out int a);
                    if (string.IsNullOrEmpty(item.txtCountPaper.Text.Trim()) || !value)
                    {
                        _controller.PopupMessage($"Plase Check PAPER NEEDED PER RUN !");
                        return null;
                    }
                    mdPara.CountPaper = int.Parse(item.txtCountPaper.Text.Trim());
                }
                mdPara.BarCode = item.txtBarcode.Text.Trim();
                
                model.ModelParas.Add(mdPara);
            }
            return model;
        }
        private void LoadUI()
        {
            Task.Run(() =>
            {
                if (_modelSelect == null)
                    return;

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    for (int i = 0; i < _partialEqpViews.Count; i++)
                    {
                        _partialEqpViews[i].tglisSkip.IsChecked = _modelSelect.ModelParas[i].IsSkip;
                        _partialEqpViews[i].tglisRotary.IsChecked = _modelSelect.ModelParas[i].IsRotary;
                        _partialEqpViews[i].tglisUseMcr.IsChecked = _modelSelect.ModelParas[i].IsUseMCR;
                        _partialEqpViews[i].tglisFirstMachine.IsChecked = _modelSelect.ModelParas[i].IsFirstMachine;
                        _partialEqpViews[i].txtBarcode.Text = _modelSelect.ModelParas[i].BarCode;
                        _partialEqpViews[i].txtCountPaper.Text = _modelSelect.ModelParas[i].CountPaper.ToString();
                        _partialEqpViews[i].tglisThickness.IsChecked = _modelSelect.ModelParas[i].IsThickness;
                    }
                    txtSelectedRecipeName.Text = _modelSelect.Name;
                }));
            });

        }
        private void LoadListView(string search="")
        {
            
            if (search== "")
            {
                _listModelName = _controller.ModelConfig.ModelList.Select(x => x.Name).ToList();
            }
            else
            {
                _listModelName = _controller.ModelConfig.ModelList.Select(x => x.Name).Where(x => x.ToUpper().Contains(search.ToUpper())).ToList();
            }
            Dispatcher.BeginInvoke((Action)(() =>
            {
                
                lstModel.Items.Clear();

                for (var i = 1; i <= _listModelName.Count; i++)
                {
                    lstModel.Items.Add(string.Format("{0} : {1}", i, _listModelName[i - 1]));

                }
                if (_controller.ModelConfig.CurrentModel== null)
                {
                    lstModel.SelectedIndex = 0;
                }
                else
                {
                    lstModel.SelectedIndex = _listModelName.IndexOf(_controller.ModelConfig.CurrentModel.Name);
                    txtWorkModel.Text = _controller.ModelConfig.CurrentModel.Name;
                }    
                
            }));
            _controller.SaveModel();
        }

        private async Task<bool> PopupMessage(ModelName model)
        {
            bool result = false;
            try
            {


                    Dispatcher.Invoke(() => {
                        PopupConfirmModelView _displayMessage = new PopupConfirmModelView(model);
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

                return result;
            }
            catch (Exception ex)
            {
                var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.Message);
                LogTxt.Add(LogTxt.Type.Exception, debug);
                return false;
            }
        }
        private async Task<bool> PopupCreate()
        {
            bool result = false;
            try
            {


                Dispatcher.Invoke(() => {
                    PopupCreateModelView popupCreate = new PopupCreateModelView();
                    result = (bool)popupCreate.ShowDialog();
                    // Check the DialogResult
                    popupCreate.Closing += (sender, a) =>
                    {
                        popupCreate = null;
                    };
                    popupCreate.Topmost = true;
                    popupCreate.Close();
                    popupCreate = null;
                });

                return result;
            }
            catch (Exception ex)
            {
                var debug = string.Format("Class:{0} Method:{1} exception occurred. Message is <{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.Message);
                LogTxt.Add(LogTxt.Type.Exception, debug);
                return false;
            }
        }
    }
}
