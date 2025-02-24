using APlc;
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

namespace ACO2_App.Views.Machine
{
    /// <summary>
    /// Interaction logic for PopupCellInfo.xaml
    /// </summary>
    public partial class PopupCellInfo : Window
    {
        private int _count;
        private WordModel[] _words;
        private TextBlock[] _txtValues;
        private TextBlock[] _txtNames;
        private ResourceDictionary res;
        public PopupCellInfo(params WordModel[] words)
        {
            InitializeComponent();
            DataContext = this;
            res = (ResourceDictionary)System.Windows.Application.LoadComponent(new Uri("/Style/ButtonStyle.xaml", UriKind.Relative));
            _count = words.Length;
            if (_count < 2) return;

            _words = new WordModel[_count];
            _txtNames = new TextBlock[_count];
            for (int i = 0; i < _count; i++)
            {
                int index = i;
                _words[i] = new WordModel();
                _words[i] = words[i];
                _words[i].WordChangedEvent += () =>
                {
                    Dispatcher.Invoke(() =>
                    {
                       // _txtNames[index].Text = string.Format("{0} : {1}", _words[i].Comment.Replace("_", ""), _words[i].GetValue);
                    });
                };
            }
            Initial();
            CreateEvent();
        }
        private void Initial()
        {
            string[] parts = _words[0].Area.Split('_');
            if (parts.Length > 1)
            {
                string result = string.Join(" ", parts, 1, parts.Length - 1);
                grbHeader.Text = result;
            }

            Grid mainGrid = this.grdMain;
            for (int i = 0; i < _count; i++) // add số hàng
            {
                mainGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < _count; i++)
            {
                int index = i;
                _txtNames[i] = new TextBlock() { FontSize = 12, HorizontalAlignment = HorizontalAlignment.Left };
                _txtNames[i].Style = (System.Windows.Style)res["ManualButtonText"];
                _txtNames[i].Text = string.Format("{0} : {1}", _words[i].Comment.Replace("_", ""), _words[i].GetValue);
                Grid.SetRow(_txtNames[i], i);
                grdMain.Children.Add(_txtNames[i]);
            }

        }

        private void CreateEvent()
        {
            btnClose.Click += (sender, args) =>
            {
                this.DialogResult = true;

            };
        }
    }
}
