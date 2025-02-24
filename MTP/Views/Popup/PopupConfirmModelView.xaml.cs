using ACO2.Model;
using ACO2_App._0.INIT;
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
using System.Windows.Shapes;

namespace ACO2.Views.Popup
{
    /// <summary>
    /// Interaction logic for PopupConfirmModelView.xaml
    /// </summary>
    public partial class PopupConfirmModelView : Window
    {
        private ModelName _model;
        private int _countConfirm;
        public PopupConfirmModelView(ModelName Model)
        {
            _model = Model;
            InitializeComponent();
            LoadUI();
            CreateEvent();
        }
        private void LoadUI()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                txtHeader.Content = _model.Name;

                //Machine 1
                txtMC1Barcode.Text = _model.ModelParas[0].BarCode.ToString();
                txtMC1Rotaty.Text = _model.ModelParas[0].IsRotary == true ? "YES" : "NO";
                txtMC1Rotaty.Foreground = _model.ModelParas[0].IsRotary == true ? Brushes.Red : Brushes.Gray;
                //Machine 2
                txtMC2Barcode.Text = _model.ModelParas[1].BarCode.ToString();
                txtMC2Rotaty.Text = _model.ModelParas[1].IsRotary == true ? "YES" : "NO";
                txtMC2Rotaty.Foreground = _model.ModelParas[1].IsRotary == true ? Brushes.Red : Brushes.Gray;
                //Machine 3
                txtMC3Barcode.Text = _model.ModelParas[2].BarCode.ToString();
                txtMC3Rotaty.Text = _model.ModelParas[2].IsRotary == true ? "YES" : "NO";
                txtMC3Rotaty.Foreground = _model.ModelParas[2].IsRotary == true ? Brushes.Red : Brushes.Gray;
                //Machine 4
                txtMC4Barcode.Text = _model.ModelParas[3].BarCode.ToString();
                txtMC4Rotaty.Text = _model.ModelParas[3].IsRotary == true ? "YES" : "NO";
                txtMC4Rotaty.Foreground = _model.ModelParas[3].IsRotary == true ? Brushes.Red : Brushes.Gray;
                //Machine 5
                txtMC5Barcode.Text = _model.ModelParas[4].BarCode.ToString();
                txtMC5Rotaty.Text = _model.ModelParas[4].IsRotary == true ? "YES" : "NO";
                txtMC5Rotaty.Foreground = _model.ModelParas[4].IsRotary == true ? Brushes.Red : Brushes.Gray;
                //Machine 6
                txtMC6Barcode.Text = _model.ModelParas[5].BarCode.ToString();
                txtMC6Rotaty.Text = _model.ModelParas[5].IsRotary == true ? "YES" : "NO";
                txtMC6Rotaty.Foreground = _model.ModelParas[5].IsRotary == true ? Brushes.Red : Brushes.Gray;
            }));
        }

        private void CreateEvent()
        {
            btnYes.Click += (sender, args) =>
            {

                var debug = string.Format("Class:{0} Method:{1} Event:{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ((Control)sender).Name);
                LogTxt.Add(LogTxt.Type.UI, debug);
                this.DialogResult = true;

            };

            btnNo.Click += (sender, args) =>
            {

                var debug = string.Format("Class:{0} Method:{1} Event:{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ((Control)sender).Name);
                LogTxt.Add(LogTxt.Type.UI, debug);
                this.DialogResult = false;

            };

            tglMc1Confirm.Click += (sender, args) =>
            {
                if (tglMc1Confirm.IsChecked==true) _countConfirm++;
                else _countConfirm--;

                btnYes.IsEnabled = _countConfirm>=6;
            };
            tglMc2Confirm.Click += (sender, args) =>
            {
                if (tglMc2Confirm.IsChecked == true) _countConfirm++;
                else _countConfirm--;
                btnYes.IsEnabled = _countConfirm >= 6;
            };
            tglMc3Confirm.Click += (sender, args) =>
            {
                if (tglMc3Confirm.IsChecked == true) _countConfirm++;
                else _countConfirm--;
                btnYes.IsEnabled = _countConfirm >= 6;
            };
            tglMc4Confirm.Click += (sender, args) =>
            {
                if (tglMc4Confirm.IsChecked == true) _countConfirm++;
                else _countConfirm--;
                btnYes.IsEnabled = _countConfirm >= 6;
            };
            tglMc5Confirm.Click += (sender, args) =>
            {
                if (tglMc5Confirm.IsChecked == true) _countConfirm++;
                else _countConfirm--;
                btnYes.IsEnabled = _countConfirm >= 6;
            };
            tglMc6Confirm.Click  += (sender, args) =>
            {
                if (tglMc6Confirm.IsChecked == true) _countConfirm++;
                else _countConfirm--;
                btnYes.IsEnabled = _countConfirm >= 6;
            };
        }

        
    }
}
