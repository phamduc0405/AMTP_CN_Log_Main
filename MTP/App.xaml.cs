using ACO2.Views.Popup;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ACO2_App._0
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        private static Mutex _mutex = null;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "ACO2_App";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                Process currentProcess = Process.GetCurrentProcess();
                foreach (Process process in Process.GetProcessesByName(currentProcess.ProcessName))
                {
                    if (process.Id != currentProcess.Id)
                    {
                        SetForegroundWindow(process.MainWindowHandle);
                        break;
                    }
                }
                System.Windows.Application.Current.Shutdown();
                return;
            }
            base.OnStartup(e);
            ShowMainWindowWithWaitingPopup();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex.Dispose();
            base.OnExit(e);
        }

        private async void ShowMainWindowWithWaitingPopup()
        {
            var waitingPopup = new WaitingPopup();
            await Dispatcher.InvokeAsync(() => waitingPopup.Show());
            var mainWindow = new MainWindow();
            mainWindow.Loaded += async (s, a) =>
            {
                waitingPopup.Close();
            };
            await Task.Run(() =>
            {
                Task.Delay(3000).Wait(); // Replace with actual background work
            });
            mainWindow.Show();
        }
    }
}
