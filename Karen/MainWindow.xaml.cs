using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System;
using Karen.Interop;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using Windows.ApplicationModel;
using MessageBox = System.Windows.MessageBox;
using Windows.Storage.Pickers;

namespace Karen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void PickFolder(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker();
            ((IInitializeWithWindow)(object)picker).Initialize(new WindowInteropHelper(this).Handle);
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            var folder = picker.PickSingleFolderAsync().GetAwaiter().GetResult();

            if (folder != null)
                Settings.Default.ContentFolder = folder.Path;
        }

        private void PickThumbFolder(object sender, RoutedEventArgs e)
        {

            var picker = new FolderPicker();
            ((IInitializeWithWindow)(object)picker).Initialize(new WindowInteropHelper(this).Handle);
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            var folder = picker.PickSingleFolderAsync().GetAwaiter().GetResult();

            if (folder != null)
                Settings.Default.ThumbnailFolder = folder.Path;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            // Set first launch to false
            Settings.Default.FirstLaunch = false;

            if (Settings.Default.StartWithWindows)
            {
                if (!AddApplicationToStartup())
                {
                    e.Cancel = true;
                }
            }
            else
                RemoveApplicationFromStartup();
        }

        public static bool AddApplicationToStartup()
        {
            var startupTask = StartupTask.GetAsync("Karen").GetAwaiter().GetResult();
            switch (startupTask.State)
            {
                case StartupTaskState.Disabled:
                    return startupTask.RequestEnableAsync().GetAwaiter().GetResult() == StartupTaskState.Enabled;
                case StartupTaskState.DisabledByUser:
                    MessageBox.Show("Auto startup disabled in Task Manager");
                    return false;
                case StartupTaskState.Enabled:
                    return true;
                case StartupTaskState.DisabledByPolicy:
                    MessageBox.Show("Auto startup disabled by policy");
                    return false;
                case StartupTaskState.EnabledByPolicy:
                    return true;
            }
            return false;
        }

        public static void RemoveApplicationFromStartup()
        {
            var startupTask = StartupTask.GetAsync("Karen").GetAwaiter().GetResult();
            startupTask.Disable();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            if (WCAUtils.IsWin11)
            {
                // Set a transparent background to let the mica brush come through
                Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));

                WCAUtils.UpdateStyleAttributes((HwndSource)sender);
                ModernWpf.ThemeManager.Current.ActualApplicationThemeChanged += (s, ev) => WCAUtils.UpdateStyleAttributes((HwndSource)sender);
            }
            else
            {
                WindowChrome.SetWindowChrome(this, null);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get PresentationSource
            PresentationSource presentationSource = PresentationSource.FromVisual((Visual)sender);

            // Subscribe to PresentationSource's ContentRendered event
            presentationSource.ContentRendered += Window_ContentRendered;
        }
    }
}
