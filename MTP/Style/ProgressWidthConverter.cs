using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ACO2.Style
{
    public class ProgressWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3 || values[0] == null || values[1] == null || values[2] == null)
                return 0;

            double value = System.Convert.ToDouble(values[0]);
            double maximum = System.Convert.ToDouble(values[1]);
            double actualWidth = System.Convert.ToDouble(values[2]);

            if (maximum == 0) return 0;

            // Tính toán chiều rộng dựa trên tỷ lệ
            return (value / maximum) * actualWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ProgressHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3 || values[0] == null || values[1] == null || values[2] == null)
                return 0;

            double value = System.Convert.ToDouble(values[0]);
            double maximum = System.Convert.ToDouble(values[1]);
            double actualHeight = System.Convert.ToDouble(values[2]);

            if (maximum == 0) return 0;

            // Tính toán chiều cao dựa trên tỷ lệ
            return (value / maximum) * actualHeight;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CircularProgressConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return "0,314"; // Giá trị mặc định

            double value = System.Convert.ToDouble(values[0]);
            double maximum = System.Convert.ToDouble(values[1]);

            if (maximum <= 0) return "0,314"; // Tránh chia cho 0

            // Chu vi vòng tròn
            double circumference = 314;

            // Tính toán độ dài nét vẽ
            double dashLength = (value / maximum) * circumference;

            // Phần còn lại của vòng tròn
            double gapLength = circumference - dashLength;

            return $"{dashLength},{gapLength}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CircularPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            double size = System.Convert.ToDouble(value);
            double radius = size / 2 - 5; // Trừ đi StrokeThickness

            // Tạo đường dẫn SVG cho vòng tròn
            return $"M {size / 2},{size / 2} m 0,-{radius} a {radius},{radius} 0 1,1 0,{2 * radius} a {radius},{radius} 0 1,1 0,-{2 * radius}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
