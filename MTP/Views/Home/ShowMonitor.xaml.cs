using ACO2_App._0;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace ACO2.Views.Home
{
    /// <summary>
    /// Interaction logic for ShowMonitor.xaml
    /// </summary>
    public partial class ShowMonitor : UserControl
    {
        private Controller _controller;
        private string _data = "0";
        private string _setting = "0";
        private string _remain ="0";
        private string _target = "0";
        private string _tactTime ="0";
        private string _productOk = "0";
        private string _productNg = "0";
        private string _startTime = DateTime.Now.ToString("yyyyMMdd");
        public ShowMonitor()
        {
            InitializeComponent();
            _controller = new Controller();
            _controller.UpdateStatusEvent -= _controller_UpdateStatusEvent;
            _controller.UpdateStatusEvent += _controller_UpdateStatusEvent;
            var result = _controller.ModelConfig.CurrentModel.ModelParas.Any(x => x.IsFirstMachine);
            if (result)
            {
                _setting = _controller.ModelConfig.CurrentModel.ModelParas.FirstOrDefault(x => x.IsFirstMachine).CountPaper.ToString();
            }
            LoadUI();
        }
        private void _controller_UpdateStatusEvent()
        {
            string total = _controller.Plc.Words.FirstOrDefault(x => x.Area == $"COMMON" && x.Item == "DATA").GetValue;
            string productOk = _controller.Plc.Words.FirstOrDefault(x => x.Area == $"COMMON" && x.Item == "PRODUCTOK").GetValue;
            string productNg = _controller.Plc.Words.FirstOrDefault(x => x.Area == $"COMMON" && x.Item == "PRODUCTNG").GetValue;
            string target = _controller.Plc.Words.FirstOrDefault(x => x.Area == $"COMMON" && x.Item == "TARGET").GetValue;
            string remain = _controller.Plc.Words.FirstOrDefault(x => x.Area == $"COMMON" && x.Item == "REMAIN").GetValue;
            string tactTime = _controller.Plc.Words.FirstOrDefault(x => x.Area == $"COMMON" && x.Item == "TACTTIME").GetValue;
            string startTime = _controller.Plc.Words.FirstOrDefault(x => x.Area == $"COMMON" && x.Item == "STARTTIME").GetValue;

            if (!string.IsNullOrEmpty(total)) _data = total;
            if (!string.IsNullOrEmpty(productOk)) _productOk = productOk;
            if (!string.IsNullOrEmpty(productNg)) _productNg = productNg;
            if (!string.IsNullOrEmpty(target)) _target = target;
            if (!string.IsNullOrEmpty(remain)) _remain = remain;
            if (!string.IsNullOrEmpty(tactTime)) _tactTime = tactTime;
            if (!string.IsNullOrEmpty(startTime)) _startTime = startTime;
            LoadUI();
        }
        private async Task LoadUI()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                txtModel.Text = _controller.ModelConfig.CurrentModel.Name;
                txtTotal.Text = _target;
                txtProductOk.Text = _productOk;
                txtProductNg.Text = _remain;
             //   txtTarget.Text = _target;
              //  txtRemain.Text = _remain;
                txtTactTime.Text = _tactTime;
                txtStartTime.Text = _startTime;
             //   txtProduct.Text = _data;
             //   txtSetting.Text = _setting;

            }));
        }
    }
}
