using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Mv.Shell
{

    public static class ProcessController
    {
        private const int SW_SHOW_NORMAL = 1;
        private const int SW_RESTORE = 9;
        private const string PROCESS_NAME = "MV.Shell";

        private static Mutex _mutex;

        #region Win32 API functions
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsZoomed(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hWnd, bool bInvert);
        #endregion

        public static void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            IntPtr hwnd = ((HwndSource)PresentationSource.FromVisual((Visual)sender)).Handle;
            Settings.Default.WindowHandle = (long)hwnd;
        }

        public static void Restart()
        {
            Settings.Default.IsRestarting = true;
            Process.Start($"{Directory.GetCurrentDirectory()}/{PROCESS_NAME}.exe");
            Application.Current.Shutdown();
        }

        public static void RunWhenStart(bool Started)
        {
            try
            {
                RegistryKey HKLM = Registry.LocalMachine;
                RegistryKey Run = HKLM.CreateSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\");
                if (Started == true)
                {
                    try
                    {
                        Run.SetValue(PROCESS_NAME.Replace('.', '_'), $"{AppDomain.CurrentDomain.BaseDirectory}/{PROCESS_NAME}.exe");
                        HKLM.Close();
                    }
                    catch (Exception Err)
                    {
                        MessageBox.Show(Err.Message.ToString(), "MUS", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    try
                    {
                        Run.DeleteValue(PROCESS_NAME.Replace('.', '_'));
                        HKLM.Close();
                    }
                    catch (Exception Err)
                    {
                        MessageBox.Show(Err.Message.ToString(), "MUS", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch(Exception Err)
            {
                MessageBox.Show(Err.Message.ToString(), "MUS", MessageBoxButton.OK, MessageBoxImage.Error);
            }
     
        }

        public static void CheckSingleton()
        {
            _mutex = new Mutex(true, PROCESS_NAME, out bool isNew);
            if (isNew || Settings.Default.IsRestarting)
            {
                Settings.Default.IsRestarting = false;
                return;
            }
            ActivateExistedWindow();
            Application.Current.Shutdown();
        }

        private static void ActivateExistedWindow()
        {
            IntPtr windowHandle = (IntPtr)Settings.Default.WindowHandle;

            SetForegroundWindow(windowHandle);
            ShowWindowAsync(windowHandle, IsIconic(windowHandle) ? SW_RESTORE : SW_SHOW_NORMAL);
            GetForegroundWindow();
            FlashWindow(windowHandle, true);
        }
    }
}
