using System;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel;

namespace LANraragi.Installer
{
	public static class Program
	{
		public static int Main(string[] args)
		{
			Console.Title = "LANraragi";
			string distro = "lanraragi";

			if (WslApi.WslIsDistributionRegistered(distro))
			{
				// TODO Handle upgrading
			}
			else
			{
				Console.WriteLine("Installing distro...");
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
			}
			Package.Current.GetAppListEntries().First(app => app.AppInfo.Id == "Karen").LaunchAsync().GetAwaiter().GetResult();
			return 0;
		}
	}
}
