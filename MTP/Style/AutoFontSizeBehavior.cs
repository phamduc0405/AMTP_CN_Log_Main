using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace ACO2.Style
{
    public static class AutoFontSizeBehavior
    {
        public static readonly DependencyProperty IsAutoFontSizeEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsAutoFontSizeEnabled",
                typeof(bool),
                typeof(AutoFontSizeBehavior),
                new PropertyMetadata(false, OnIsAutoFontSizeEnabledChanged));

        private const double MinimumFontSize = 10; // Giá trị FontSize nhỏ nhất

        public static void SetIsAutoFontSizeEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsAutoFontSizeEnabledProperty, value);
        }

        public static bool GetIsAutoFontSizeEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsAutoFontSizeEnabledProperty);
        }

        private static void OnIsAutoFontSizeEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                if ((bool)e.NewValue)
                {
                    textBlock.SizeChanged += TextBlock_SizeChanged;
                    UpdateFontSize(textBlock);
                }
                else
                {
                    textBlock.SizeChanged -= TextBlock_SizeChanged;
                }
            }
        }

        private static void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                UpdateFontSize(textBlock);
            }
        }

        private static void UpdateFontSize(TextBlock textBlock)
        {
            if(textBlock.FontSize > 0) { return; }
            // Kiểm tra ActualHeight trước khi tính toán FontSize
            if (textBlock.ActualHeight > 0)
            {
                double calculatedFontSize = textBlock.ActualHeight * 0.5; // Điều chỉnh hệ số 0.5 theo nhu cầu
                textBlock.FontSize = Math.Max(calculatedFontSize, MinimumFontSize); // Đảm bảo FontSize >= MinimumFontSize
            }
        }
    }
}
