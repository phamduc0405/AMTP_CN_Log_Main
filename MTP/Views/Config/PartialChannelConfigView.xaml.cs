using ACO2_App._0;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MTP.Views.Config
{
    /// <summary>
    /// Interaction logic for PartialChannelConfigView.xaml
    /// </summary>
    public partial class PartialChannelConfigView : UserControl
    {

        private object targetObject;
        private ResourceDictionary resTextBlock;
        private ResourceDictionary resTextBox;
        private ResourceDictionary resCbBox;

        public PartialChannelConfigView(object obj, string header = "")
        {
            InitializeComponent();
            CreateEvent();
            resTextBlock = (ResourceDictionary)Application.LoadComponent(new Uri("/style/ButtonStyle.xaml", UriKind.Relative));
            resTextBox = (ResourceDictionary)Application.LoadComponent(new Uri("/style/TextBoxStyle.xaml", UriKind.Relative));
            resCbBox = (ResourceDictionary)Application.LoadComponent(new Uri("/style/ComboBoxStyle.xaml", UriKind.Relative));
            Initial(obj);
            txtHeader.Text = header;
        }
        private void CreateEvent()
        {

        }
        public void Initial(object obj)
        {
            targetObject = obj;

            // Tạo StackPanel Layout
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(10),
                Orientation = Orientation.Vertical
            };
            stkMain.Children.Add(stackPanel);

            // Lấy tất cả các thuộc tính của đối tượng
            var properties = targetObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Thêm các trường nhập liệu cho từng thuộc tính
            foreach (var property in properties)
            {
                string displayName = GetDisplayName(property);
                object propertyValue = property.GetValue(targetObject) ?? string.Empty;

                if (displayName == "HourSplitCheckPerformanceDay")
                {
                    AddField(stackPanel, displayName, propertyValue.ToString(), true, value =>
                    {
                        // Thiết lập giá trị thuộc tính cho đối tượng đích (ComboBox case)
                        if (property.PropertyType == typeof(int) && int.TryParse(value, out int intValue))
                        {
                            property.SetValue(targetObject, intValue);
                        }
                    });
                }
                else
                {
                    AddField(stackPanel, displayName, propertyValue.ToString(), false, value =>
                    {
                        // Thiết lập giá trị thuộc tính cho đối tượng đích (TextBox case)
                        if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(targetObject, value);
                        }
                        else if (property.PropertyType == typeof(int) && int.TryParse(value, out int intValue))
                        {
                            property.SetValue(targetObject, intValue);
                        }
                        else if (property.PropertyType == typeof(double) && double.TryParse(value, out double doubleValue))
                        {
                            property.SetValue(targetObject, doubleValue);
                        }
                        else if (property.PropertyType == typeof(DateTime) && DateTime.TryParse(value, out DateTime dateValue))
                        {
                            property.SetValue(targetObject, dateValue);
                        }
                    });
                }
            }
        }

        private string GetDisplayName(PropertyInfo property)
        {
            var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();
            return displayNameAttribute != null ? displayNameAttribute.DisplayName : property.Name;
        }

        private void AddField(StackPanel stackPanel, string labelText, string initialValue, bool isComboBox, Action<string> onValueChanged)
        {
            // Tạo StackPanel cho mỗi dòng
            var fieldPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

            //TextBlock
            var label = new TextBlock { Text = labelText, Width = 250, Style = (System.Windows.Style)resTextBlock["HeaderTextBlockStyle"] };
            fieldPanel.Children.Add(label);

            if (isComboBox)
            {
                // ComboBox
                var comboBox = new ComboBox { Width = 200, Style = (System.Windows.Style)resCbBox["ComboBoxFlatStyle"] };
                for (int i = 0; i < 24; i++)
                {
                    comboBox.Items.Add(i.ToString());
                }
                comboBox.SelectedValue = initialValue;
                comboBox.SelectionChanged += (s, e) => onValueChanged(comboBox.SelectedValue.ToString());
                fieldPanel.Children.Add(comboBox);
            }
            else
            {
                // TextBox
                var textBox = new TextBox { Margin = new Thickness(0, 0, 0, 0), Text = initialValue, Width = 400, Style = (System.Windows.Style)resTextBox["TextBoxStandard"] };
                textBox.TextChanged += (s, e) => onValueChanged(textBox.Text);
                fieldPanel.Children.Add(textBox);
            }

            stackPanel.Children.Add(fieldPanel);
        }
    }
}
