using ACO2.Model;
using ACO2_App._0;
using ACO2_App._0.INIT;
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

namespace ACO2.Views.Config
{
    /// <summary>
    /// Interaction logic for PopupCreateModelView.xaml
    /// </summary>
    public partial class PopupCreateModelView : Window
    {
        private Controller _controller;
        private List<PartialEqpView> _partialEqpViews;
        public PopupCreateModelView()
        {
            InitializeComponent();
            _controller = MainWindow.Controller;
            _controller.MainBarcodeEvent += _controller_MainBarcodeEvent;
            Initial();
            CreateEvent();
        }
        private void _controller_MainBarcodeEvent(string code)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                txtModelName.Text = code;
               
            }));

        }
        private void Initial()
        {
            _partialEqpViews = new List<PartialEqpView>();
            for (int i = 0; i < 6; i++)
            {
                PartialEqpView eqp = new PartialEqpView() { Header = $"Equiment #{i + 1}" };
                _partialEqpViews.Add(eqp);
                Grid.SetColumn(eqp, i % 3);
                Grid.SetRow(eqp, i / 3);
                grdMain.Children.Add(eqp);
            }

        }
        private void CreateEvent()
        {
            btnOK.Click += async (sender, args) =>
            {
                // var debug = string.Format("Class:{0} Method:{1} Event:{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ((Control)sender).Name);
                //    LogTxt.Add(LogTxt.Type.UI, debug);
                var result = await CreateNewModel();
                if (!result) return;
                this.DialogResult = true;

            };

            btnCancel.Click += (sender, args) =>
            {

              //  var debug = string.Format("Class:{0} Method:{1} Event:{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ((Control)sender).Name);
              //  LogTxt.Add(LogTxt.Type.UI, debug);
                this.DialogResult = false;

            };
            bdrHeader.MouseDown += (sender, e) =>
            {
                DragMove();
            };
        }
        private async Task<bool> CreateNewModel()
        {
            if (CheckError())
            {
                _controller.PopupMessage($"Cannot Create! \nPlease Check Format Input!");
                return false;
            }
            if (!_controller.ModelConfig.ModelList.Any(x => x.Name == txtModelName.Text.Trim()))
            {
                var result = await _controller.PopupMessage($"Do You Want Create Model : {txtModelName.Text}", true);
                if (result)
                {
                    ModelName model = GetModelFromUI();
                    if (model == null) return false;
                    _controller.ModelConfig.ModelList.Add(model);
                    
                }
                else { return false;  }
            }
            else
            {
                _controller.PopupMessage($"Model {txtModelName.Text} is Exist !");
                return false;
            }
            return true;
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
            model.Name = txtModelName.Text.Trim();
            foreach (var item in _partialEqpViews)
            {
                ModelParameter mdPara = new ModelParameter();
                mdPara.IsSkip = item.tglisSkip.IsChecked == true;
                mdPara.IsRotary = item.tglisRotary.IsChecked == true;
                mdPara.IsUseMCR = item.tglisUseMcr.IsChecked == true;
                mdPara.IsFirstMachine = item.tglisFirstMachine.IsChecked == true;
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
    }
}
