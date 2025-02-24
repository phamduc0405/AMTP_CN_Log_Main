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
    /// Interaction logic for PopupMessage.xaml
    /// </summary>
    public partial class PopupMessage : Window
    {
        #region User defined
        public enum Style
        {
            YesNo,
            Ok,

        }
        #endregion

        #region Fields
        private readonly Style _style;
        private readonly string[] _messages;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public PopupMessage(Style style, params string[] messages)
        {

            InitializeComponent();
            _style = style;
            _messages = messages;

            Initialize();

            CreateEvent();
        }

        /// <summary>
        /// Method for initialization.
        /// </summary>
        private void Initialize()
        {
            switch (_style)
            {
                case Style.YesNo:
                    btnYes.Visibility = btnNo.Visibility = Visibility.Visible;
                    btnOk.Visibility = Visibility.Hidden;
                    break;
                case Style.Ok:
                    btnYes.Visibility = btnNo.Visibility = Visibility.Hidden;
                    btnOk.Visibility = Visibility.Visible;
                    break;
            }


            foreach (var message in _messages)
            {
                var split = message.Trim('\r').Split('\n');
                foreach (var content in split)
                {
                    lstBox.Items.Add(content.TrimEnd('\r'));
                }
            }
        }

        /// <summary>
        /// Method for link events.
        /// </summary>
        private void CreateEvent()
        {
            btnOk.Click += (sender, args) =>
            {

                var debug = string.Format("Class:{0} Method:{1} Event:{2}>.", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ((Control)sender).Name);
                LogTxt.Add(LogTxt.Type.UI, debug);
                this.DialogResult = true;

            };

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
        }
    }
}
