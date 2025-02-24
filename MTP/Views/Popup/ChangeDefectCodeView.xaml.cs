using ACO2.Style;
using MTP.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MTP.Views.Popup
{
    /// <summary>
    /// Interaction logic for ChangeDefectCodeView.xaml
    /// </summary>
    public partial class ChangeDefectCodeView : Window
    {
        private DefectCode _defectcode;
        private List<LabelTextBox> _labels = new List<LabelTextBox>();

        public DefectCode Defectcode
        {
            get { return _defectcode; }
            set { _defectcode = value; }
        }
        public ChangeDefectCodeView(DefectCode defectcode)
        {
            InitializeComponent();
            _defectcode = defectcode;
            CreateEvent();
            Initial();
        }
        private void CreateEvent()
        {
            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = false;
                this.Close();
            };
            btnSave.Click += (s, e) =>
            {
                _defectcode = GetDataFromUI();
                this.DialogResult = true;
                this.Close();
            };
        }
        private void Initial()
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;
            // Lặp qua từng thuộc tính của lớp DefectCode
            foreach (var property in typeof(DefectCode).GetProperties())
            {
                // Lấy DisplayName của thuộc tính nếu có, nếu không lấy tên thuộc tính
                string displayName = property.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                                              .OfType<DisplayNameAttribute>()
                                              .FirstOrDefault()?.DisplayName ?? property.Name;

                LabelTextBox lb = new LabelTextBox();
                lb.txtLabel.Text = displayName;
                lb.LabelWidth = 180;
                lb.TextBoxWidth = 300;
                lb.ControlHeight = 50;
                lb.Margin = new Thickness(0,-20,0,0);
                object value = property.GetValue(_defectcode);
                lb.txtInput.Text = value != null ? value.ToString() : string.Empty;
                _labels.Add(lb);
                if (displayName == "Index")
                {
                    lb.txtInput.IsEnabled = false;
                }
                stackPanel.Children.Add(lb);
            }
            grdMain.Children.Add(stackPanel);
        }
        private DefectCode GetDataFromUI()
        {
            DefectCode defectCode = new DefectCode();
            foreach (var property in typeof(DefectCode).GetProperties())
            {
                // Find the corresponding LabelTextBox for the property
                var lb = _labels.FirstOrDefault(l => l.txtLabel.Text == property.Name ||
                                                     l.txtLabel.Text == property.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                                                                             .OfType<DisplayNameAttribute>()
                                                                             .FirstOrDefault()?.DisplayName);

                if (lb != null)
                {
                    string inputText = lb.txtInput.Text;

                    if (string.IsNullOrEmpty(inputText))
                    {
                        continue;
                    }

                    // Convert the inputText to the appropriate type
                    object value = Convert.ChangeType(inputText, property.PropertyType);

                    // Set the value to the property
                    property.SetValue(defectCode, value);
                }
            }
            return defectCode;
        }
    }
}
