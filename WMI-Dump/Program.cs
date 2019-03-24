using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using WMILib.Models;

namespace WMI_Dump
{
	class Program
	{
		public static string ComputerPath = string.Empty;
		public static WMIClasses ClassName = new WMIClasses { ComputerName = Environment.MachineName };

		static void Main(string[] args)
		{
			Log("Start dump data from all available WMIClasses.", true);

			// Make the data in computer related folder
			GetComputerPath();

			// Collect all WMIClassNames
			GetWMIClassNames();

			// Open WMIClassName and dump data in Json file
			GetWMIClassesData();

			WriteWMIClasses();

			Log("Completed");
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

		private static void GetWMIClassNames()
		{
			Log("Start function GetWMIClassNames()");

			ConnectionOptions options = new ConnectionOptions
			{
				Impersonation = System.Management.ImpersonationLevel.Impersonate
			};

			ManagementScope scope = new ManagementScope("\\\\.\\root\\cimv2", options);
			scope.Connect();

			Log("WMI scope is opened");

			ObjectQuery query = new ObjectQuery($"SELECT * FROM meta_class");

			ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

			ManagementObjectCollection queryCollection = searcher.Get();

			Log("Start WMIClassNames collection");

			try
			{
				foreach (ManagementClass managementClass in queryCollection)
				{
					ClassName.Classes.Add(new WMIClassName
					{
						Name = managementClass.ToString().Split(':')[1].ToString()
					});
				}
			}
			catch (Exception ex)
			{
				Log($"Error: {ex.Message}");
			}

			WriteWMIClasses();

			Log($"{ClassName.Classes.Count} classes are collected");
			Log("GetWMIClassNames() is completed");
		}

		private static void WriteWMIClasses()
		{
			string json = JsonConvert.SerializeObject(ClassName, Formatting.Indented);
			using (StreamWriter stream = new StreamWriter($"{ComputerPath}\\WMIClasses.json"))
			{
				stream.Write(json);
			}

			Log("WMIClasses.json is written");
		}

		private static void GetWMIClassesData()
		{
			Log("Start function GetWMIClassesData()");

			DateTime Start;
			foreach (WMIClassName className in ClassName.Classes.OrderBy(x => x.Name))
			{
				Start = DateTime.Now;

				string Status = GetWMIClassData(className.Name);

				if (!string.IsNullOrEmpty(Status))
				{
					// Json exists already
					className.Status = Status;
					className.Duration = DateTime.Now - Start;
				}

				WriteWMIClasses();
			}

			Log("GetWMIClassesData() is completed");
		}

		private static string GetWMIClassData(string name)
		{
			if (File.Exists($"{ComputerPath}\\{name}.json"))
			{
				Log($"{name}.json is existing");
				return string.Empty;
			}
			string status = "In processing";

			Log($"Start function GetWMIClassData({name})");

			List<WMIProperty> ClassData = new List<WMIProperty>();

			#region Collect data

			ConnectionOptions options = new ConnectionOptions
			{
				Impersonation = System.Management.ImpersonationLevel.Impersonate
			};

			ManagementScope scope = new ManagementScope("\\\\.\\root\\cimv2", options);
			scope.Connect();

			//Query system for Operating System information
			ObjectQuery query = new ObjectQuery($"SELECT * FROM {name}");

			ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

			ManagementObjectCollection queryCollection = searcher.Get();

			try
			{
				int collectionIndex = 0;
				foreach (ManagementObject managementObject in queryCollection)
				{
					int propertyIndex = 0;
					foreach (PropertyData propertyData in managementObject.Properties)
					{
						ClassData.Add(new WMIProperty(collectionIndex, propertyIndex, propertyData));
						propertyIndex++;
					}
					collectionIndex++;
				}

				if (ClassData.Count == 0)
				{
					status = "Empty";
				}
				else
				{
					status = "OK";
				}
			}
			catch (Exception ex)
			{
				Log($"Error: {ex.Message}");
				status = "Error";
			}

			#endregion

			string json = JsonConvert.SerializeObject(ClassData, Formatting.Indented);
			using (StreamWriter stream = new StreamWriter($"{ComputerPath}\\{name}.json"))
			{
				stream.Write(json);
			}

			Log("WMIClasses.json is written");

			ClassData.Clear();
			ClassData = null;

			Log($"GetWMIClassData({name}) is completed with status: {status}");
			return status;
		}
	}

	public class WMIClasses
	{
		public string ComputerName { get; set; }

		public List<WMIClassName> Classes = new List<WMIClassName>();
	}

	public class WMIClassName
	{
		public string Name { get; set; }
		public string Status { get; set; } = "Unknown";
		public TimeSpan Duration { get; set; }
	}
}
