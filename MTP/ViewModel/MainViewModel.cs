using ACO2.Views.Home;
using ACO2_App._0.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ACO2_App._0.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        #region Fields
        private string _header;
        private static string _styleheader;
        private object _currentview;
        private DateTime _datetime;
        #endregion
        #region Properties
        public RelayCommand T5ViewCommand { get; set; }
        public RelayCommand ConfigViewCommand { get; set; }
        public RelayCommand MonitorIOViewCommand { get; set; }
        public RelayCommand DataViewCommand { get; set; }
        public RelayCommand ShowMonitorCommand {  get; set; }

        public T5ViewModel T5VM { get; set; }
        public ConfigViewModel ConfigVM { get; set; }
        public MonitorIOView MonitorIOVM { get; set; }

        public String Header
        {
            get { return _header; }
            set
            {
                _header = value;
                OnPropertyChanged();
            }
        }
        public String StyleHeader
        {
            get { return _styleheader; }
            set
            {
                _styleheader = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public object Currentview
        {
            get { return _currentview; }
            set
            {
                _currentview = value;
                OnPropertyChanged();
            }
        }
        

        #region CONSTRUCTOR
        public MainViewModel()
        {
            T5VM = new T5ViewModel();
            ConfigVM = new ConfigViewModel();
            MonitorIOVM = new MonitorIOView();
            Currentview = T5VM;
            Header = "HOME";

            #region Chuyển Màn Hình Child Auto,Manual
            T5ViewCommand = new RelayCommand(o =>
            {
                Currentview = T5VM;
                if (StyleHeader == "vi")
                {
                    Header = "TRANG CHỦ";
                }
                else
                {
                    Header = "HOME";
                }
            });
            ConfigViewCommand = new RelayCommand(o =>
            {
                Currentview = ConfigVM;
                if(StyleHeader == "vi")
                {
                    Header = "CẤU HÌNH";
                }
                else
                {
                    Header = "CONFIG";
                }
                

            });
            MonitorIOViewCommand = new RelayCommand(o =>
            {
                Currentview = MonitorIOVM;
                if (StyleHeader == "vi")
                {
                    Header = "TÍN HIỆU IO";
                }
                else
                {
                    Header = "MONITOR IO";
                }
            });
            #endregion


        }
        #endregion
    }
}
