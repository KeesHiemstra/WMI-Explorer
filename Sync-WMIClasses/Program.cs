using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMILib.Models;

namespace Sync_WMIClasses
{
	class Program
	{
		public static string ComputerPath = string.Empty;
		public static string DumpJsonFile = $"{ComputerPath}\\WMIClasses.json";
		public static string DiscoverJsonFile = @"C:\Etc\ITAM\WMI\WMIClasses.json";

		public static DumpClasses Dump = new DumpClasses { ComputerName = Environment.MachineName };
		public static List<WMIClass> Discover = new List<WMIClass>();

		static void Main(string[] args)
		{
			Log("Start compare the Discover classes with the Dump classes", true);

			GetComputerPath();

			LoadDumpClasses();

			LoadDiscoverClasses();

			CompareDiscoverDump();

			SaveDiscoverClasses();

			Console.Write("\nPress any key...");
			Console.ReadKey();
		}

		private static void Log(string Message, bool NewFile = false)
		{
			using (StreamWriter stream = new StreamWriter($"C:\\Etc\\Log\\{Environment.MachineName}.log", !NewFile))
			{
				stream.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {Message}");
			}
			Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {Message}");
		}

		private static void GetComputerPath()
		{
			ComputerPath = $"C:\\Etc\\WMI\\{Environment.MachineName}";
			if (!Directory.Exists(ComputerPath))
			{
				Directory.CreateDirectory(ComputerPath);
			}
		}

		private static void LoadDumpClasses()
		{
			if (File.Exists(DumpJsonFile))
			{
				using (StreamReader stream = File.OpenText(DumpJsonFile))
				{
					string json = stream.ReadToEnd();
					Dump = JsonConvert.DeserializeObject<DumpClasses>(json);
				}
			}

			Log($"Loaded Dump classes ({DumpJsonFile})");
		}

		private static void LoadDiscoverClasses()
		{
			if (File.Exists(DiscoverJsonFile))
			{
				using (StreamReader stream = File.OpenText(DiscoverJsonFile))
				{
					string json = stream.ReadToEnd();
					Discover = JsonConvert.DeserializeObject<List<WMIClass>>(json);
				}
			}

			Log($"Loaded Discover classes ({DiscoverJsonFile})");
		}

		private static void SaveDiscoverClasses()
		{
			string json = JsonConvert.SerializeObject(Discover, Formatting.Indented);
			using (StreamWriter stream = new StreamWriter(DiscoverJsonFile))
			{
				stream.Write(json);
			}

			Log($"Saved Discover classes ({DiscoverJsonFile})");
		}

		private static void CompareDiscoverDump()
		{
			Log("Start CompareDiscoverDump()");

			foreach (WMIClass discover in Discover)
			{
				DumpClassName dump = Dump.Classes.Where(x => x.Name == discover.Name).FirstOrDefault();
				if (discover.Status != dump.Status)
				{
					discover.Status = dump.Status;
					Log($"{discover.Status} is updated");
				}
			}

			Log("CompareDiscoverDump() is completed");
		}
	}

	public class DumpClasses
	{
		public string ComputerName { get; set; }

		public List<DumpClassName> Classes = new List<DumpClassName>();
	}

	public class DumpClassName
	{
		public string Name { get; set; }
		public string Status { get; set; } = "Unknown";
		public TimeSpan Duration { get; set; }
	}
}
