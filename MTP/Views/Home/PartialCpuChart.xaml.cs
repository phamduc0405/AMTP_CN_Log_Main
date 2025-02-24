using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using ACO2_App._0;

namespace ACO2.Views.Home
{
    /// <summary>
    /// Interaction logic for PartialCpuChart.xaml
    /// </summary>
    public partial class PartialCpuChart : UserControl
    {
        private PerformanceCounter cpuCounter;
        private Process currentProcess;
        public SeriesCollection LastHourSeries { get; set; }
        private CancellationTokenSource cancellationTokenSource;

        public PartialCpuChart()
        {

            InitializeComponent();
            currentProcess = Process.GetCurrentProcess();
            cpuCounter = new PerformanceCounter("Process", "% Processor Time", currentProcess.ProcessName);
            LastHourSeries = new SeriesCollection
        {
            new LineSeries
            {
                AreaLimit = -10,
                Values = new ChartValues<ObservableValue>(),
                LineSmoothness = 0.8, // 0: đường thẳng, 1: đường rất mượt
                PointGeometry = null, // Ẩn các điểm để làm cho đường mượt hơn
            }
        };
            cancellationTokenSource = new CancellationTokenSource();
            UpdateCpu(cancellationTokenSource.Token);
            DataContext = this;
        }

        private void UpdateCpu(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                var r = new Random();
                while (!cancellationToken.IsCancellationRequested && MainWindow.Running)
                {
                    Thread.Sleep(500);
                    float cpuUsage = cpuCounter.NextValue() / Environment.ProcessorCount;
                    try
                    {
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            LastHourSeries[0].Values.Add(new ObservableValue(cpuUsage));
                            if (LastHourSeries[0].Values.Count > 60)
                                LastHourSeries[0].Values.RemoveAt(0);
                            txtTarget.Text = $"CPU Usage: {cpuUsage:F2}%";
                        });
                    }
                    catch (Exception)
                    {
                        return;
                    }

                }
            }, cancellationToken);
        }

        public void OnUnloaded()
        {
            // Cancel the task when the control is unloaded
            cancellationTokenSource.Cancel();
        }

    }
}
