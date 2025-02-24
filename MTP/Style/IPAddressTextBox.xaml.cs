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

namespace ACO2_App.Style
{
    /// <summary>
    /// Interaction logic for IPAddressTextBox.xaml
    /// </summary>
    public partial class IPAddressTextBox : UserControl
    {
        public IPAddressTextBox()
        {
            InitializeComponent();
        }

        // First Segment
        public string FirstSegment
        {
            get => (string)GetValue(FirstSegmentProperty);
            set => SetValue(FirstSegmentProperty, value);
        }

        public static readonly DependencyProperty FirstSegmentProperty =
            DependencyProperty.Register(nameof(FirstSegment), typeof(string), typeof(IPAddressTextBox), new PropertyMetadata(string.Empty));

        // Second Segment
        public string SecondSegment
        {
            get => (string)GetValue(SecondSegmentProperty);
            set => SetValue(SecondSegmentProperty, value);
        }

        public static readonly DependencyProperty SecondSegmentProperty =
            DependencyProperty.Register(nameof(SecondSegment), typeof(string), typeof(IPAddressTextBox), new PropertyMetadata(string.Empty));

        // Third Segment
        public string ThirdSegment
        {
            get => (string)GetValue(ThirdSegmentProperty);
            set => SetValue(ThirdSegmentProperty, value);
        }

        public static readonly DependencyProperty ThirdSegmentProperty =
            DependencyProperty.Register(nameof(ThirdSegment), typeof(string), typeof(IPAddressTextBox), new PropertyMetadata(string.Empty));

        // Last Segment
        public string LastSegment
        {
            get => (string)GetValue(LastSegmentProperty);
            set => SetValue(LastSegmentProperty, value);
        }

        public static readonly DependencyProperty LastSegmentProperty =
            DependencyProperty.Register(nameof(LastSegment), typeof(string), typeof(IPAddressTextBox), new PropertyMetadata(string.Empty));

        // Full IP Address
        public string FullIpAddress => $"{FirstSegment}.{SecondSegment}.{ThirdSegment}.{LastSegment}";

        private void SegmentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            if (textBox.Text.Length == 3)
            {
                MoveFocusToNextSegment(textBox);
            }
        }

        private void SegmentTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            if (e.Key == Key.Back && string.IsNullOrEmpty(textBox.Text))
            {
                MoveFocusToPreviousSegment(textBox);
            }
        }

        private void MoveFocusToNextSegment(TextBox currentTextBox)
        {
            if (currentTextBox == txtFirstSegment)
            {
                txtSecondSegment.Focus();
            }
            else if (currentTextBox == txtSecondSegment)
            {
                txtThirdSegment.Focus();
            }
            else if (currentTextBox == txtThirdSegment)
            {
                txtLastSegment.Focus();
            }
        }

        private void MoveFocusToPreviousSegment(TextBox currentTextBox)
        {
            if (currentTextBox == txtSecondSegment)
            {
                txtFirstSegment.Focus();
            }
            else if (currentTextBox == txtThirdSegment)
            {
                txtSecondSegment.Focus();
            }
            else if (currentTextBox == txtLastSegment)
            {
                txtThirdSegment.Focus();
            }
        }
    }
}
