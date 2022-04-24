using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Windows.ApplicationModel;
using Windows.Storage;

namespace LANraragi.Installer
{
    public static class Program
    {

        private static ApplicationDataContainer Settings = ApplicationData.Current.LocalSettings;

        public static int Main(string[] args)
        {
            Console.Title = "LANraragi Installer";
            string distro = "lanraragi";

            StringBuilder wsl = new StringBuilder("wsl.exe", 260);
            if (!Win32.PathFindOnPath(wsl, null))
            {
                Console.WriteLine("Windows Subsystem for Linux is not installed on this machine and is required by this program.");
                Console.WriteLine("Please install it by following the instructions in this link: https://docs.microsoft.com/en-us/windows/wsl/install");
                Console.ReadKey();
                return 0;
            }

            var karen = Settings.Values["Karen"] == null ? false : (bool)Settings.Values["Karen"];
            Settings.Values["Karen"] = false;

            if (Process.GetProcessesByName("Karen").Count() >= 1 && !karen)
            {
                Console.WriteLine("LANraragi for Windows is already running.\nClose it to access installer options.\n\n");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                return 0;
            }

            bool needsUpgrade = Version.TryParse(ApplicationData.Current.LocalSettings.Values["Version"]?.ToString() ?? "", out var oldVersion) && oldVersion < GetVersion();

            if (!WslApi.WslIsDistributionRegistered(distro) || needsUpgrade)
            {
                if (needsUpgrade)
                {
                    Console.WriteLine("Upgrading distro...");
                    UnInstall(distro);
                }
                else
                {
                    Console.WriteLine("Installing distro...");
                }
                Install(distro);
            }
            else
            {
                Console.WriteLine(" == LANraragi Distro == ");
                Console.WriteLine(" r - Reinstall");
                Console.WriteLine(" u - Uninstall");
                Console.WriteLine(" c - Exit");
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.R:
                        UnInstall(distro);
                        Install(distro);
                        return 0;
                    case ConsoleKey.U:
                        UnInstall(distro);
                        return 0;
                    case ConsoleKey.C:
                        return 0;
                }
            }
            Package.Current.GetAppListEntriesAsync().GetAwaiter().GetResult()
                .First(app => app.AppUserModelId == Package.Current.Id.FamilyName + "!Karen").LaunchAsync().GetAwaiter().GetResult();
            return 0;
        }

        private static void Install(string distro)
        {
            WslApi.WslRegisterDistribution("lanraragi", "package.tar.gz");
            WslApi.WslLaunchInteractive(distro, "/bin/rm /etc/resolv.conf", true, out uint code);
            new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "wslconfig.exe",
                    Arguments = "/terminate " + distro,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            }.Start();
            Settings.Values["Version"] = GetVersion().ToString();
        }

        private static void UnInstall(string distro)
        {
            WslApi.WslUnregisterDistribution(distro);
            Settings.Values.Remove("Version");
        }

        private static Version GetVersion()
        {
            var version = Package.Current.Id.Version;
            return new Version(version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
