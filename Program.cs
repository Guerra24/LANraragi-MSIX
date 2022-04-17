using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LANraragi.Installer
{
	public static class Program
	{
		public static int Main(string[] args)
		{
			string distro = "lanraragi";

			Console.WriteLine(" == LANraragi MSIX Installer == ");
			if (WslApi.WslIsDistributionRegistered(distro))
			{
				Console.WriteLine("Distro already installed");
			}
			else
			{
				Console.WriteLine("Installing distro...");
				WslApi.WslRegisterDistribution("lanraragi", "package.tar.gz");
				WslApi.WslLaunchInteractive(distro, "/bin/rm /etc/resolv.conf", true, out uint code);
				Console.WriteLine("Done");
			}
			Console.ReadKey();
			return 0;
		}
	}
}
