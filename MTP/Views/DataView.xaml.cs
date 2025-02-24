using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
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
using System.Windows.Threading;

namespace ACO2_App.Views
{
    /// <summary>
    /// Interaction logic for DataView.xaml
    /// </summary>
    public partial class DataView : UserControl
    {
        private PerformanceCounter diskReadCounter;
        private PerformanceCounter diskWriteCounter;
        public DataView()
        {
            InitializeComponent();
            // Lấy thông tin về tiến trình đang chạy
            Process process = Process.GetCurrentProcess();

            // Thiết lập timer để cập nhật thông tin mỗi giây
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                //// Hiển thị thông tin sử dụng CPU
                //processName.Text = process.ProcessName;
                //cpuUsage.Text = process.ProcessorAffinity.ToString();

                //// Hiển thị thông tin sử dụng bộ nhớ
                //memoryUsage.Text = process.WorkingSet64.ToString();

                //// Hiển thị thông tin số lượng luồng
                //threadCount.Text = Environment.ProcessorCount.ToString();

                //// Hiển thị thông tin tốc độ CPU
                //PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                //cpuSpeed.Text = cpuCounter.NextValue().ToString();

                //// Hiển thị dung lượng bộ nhớ khả dụng
                //PerformanceCounter memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
                //memoryAvailable.Text = memoryCounter.NextValue().ToString();
            };
            timer.Start();
        }

       


    }
}
