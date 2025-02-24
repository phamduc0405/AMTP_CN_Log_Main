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

namespace MTP.Views.Config
{
    /// <summary>
    /// Interaction logic for PartialNameView.xaml
    /// </summary>
    public partial class PartialNameView : UserControl
    {
        public event EventHandler TxtNameRequested;
        public event EventHandler TxtIDRequested;
        public int _index;

        public PartialNameView(int index, string label = "")
        {
            InitializeComponent();
            CreateEvent();
            _index = index;
            txtLabel.Text = label;
        }
        private void CreateEvent()
        {
            txtName.GotFocus += (s, e) =>
            {
                TxtNameRequested?.Invoke(this, EventArgs.Empty);

            };
            txtName.TextChanged += (s, e) =>
            {
                TxtNameRequested?.Invoke(this, EventArgs.Empty);
            };
            txtID.GotFocus += (s, e) =>
            {
                TxtIDRequested?.Invoke(this, EventArgs.Empty);

            };
            txtID.TextChanged += (s, e) =>
            {
                TxtIDRequested?.Invoke(this, EventArgs.Empty);
            };
        }
    }
}
