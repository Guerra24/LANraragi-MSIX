using System;
using System.Linq;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Karen.Interop;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Karen
{
    /// <summary>
    /// Simple application. Check the XAML for comments.
    /// </summary>
    public partial class App : Application
    {
        private TaskbarIcon notifyIcon;
        public WslDistro Distro { get; set; }

        public void ToastNotification(string text)
        {
            notifyIcon.ShowBalloonTip("LANraragi", text, notifyIcon.Icon, true);
        }

        public void ShowConfigWindow()
        {
            var mainWindow = Application.Current.MainWindow;

            if (mainWindow == null || mainWindow.GetType() != typeof(MainWindow) )
                mainWindow = new MainWindow();

            mainWindow.Show();

            if (mainWindow.WindowState == WindowState.Minimized)
                mainWindow.WindowState = WindowState.Normal;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Only one instance of the bootloader allowed at a time
            var exists = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1;
            if (exists)
            {
                MessageBox.Show("Another instance of the application is already running.");
                Application.Current.Shutdown();
            }

            Distro = new WslDistro();

            bool needsUpgrade = Version.TryParse(Settings.Values["Version"]?.ToString() ?? "", out var oldVersion) && oldVersion < GetVersion();
            if (!Distro.CheckDistro() || needsUpgrade)
			{
                Settings.Values["Karen"] = true;
                Package.Current.GetAppListEntries().First(app => app.AppInfo.Id == "Installer").LaunchAsync().GetAwaiter().GetResult();
                Application.Current.Shutdown();
                return;
			}

            Karen.Properties.Settings.Default.Upgrade();

            // First time ?
            if (Karen.Properties.Settings.Default.FirstLaunch)
            {
                MessageBox.Show("Looks like this is your first time running the app! Please setup your Content Folder in the Settings.");
                ShowConfigWindow();
            }

            // Create the Taskbar Icon now so it appears in the tray
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            // Check if server starts with app 
            if (Karen.Properties.Settings.Default.StartServerAutomatically && Distro.Status == AppStatus.Stopped)
            {
                ToastNotification("LANraragi is starting automagically...");
                Distro.StartApp();
            }
            else
                ToastNotification("The Launcher is now running! Please click the icon in your Taskbar.");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            try
            {
                Distro.StopApp();
            } finally
            {
                WslDistro.FreeConsole(); //clean up the console to ensure it's closed alongside the app
                base.OnExit(e);
            }
        }

        private static Version GetVersion()
        {
            var version = Package.Current.Id.Version;
            return new Version(version.Major, version.Minor, version.Build, version.Revision);
        }

        private static ApplicationDataContainer Settings = ApplicationData.Current.LocalSettings;
    }
}
