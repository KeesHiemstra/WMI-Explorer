using System.Linq;

using WMILib.Models;

namespace WMI_Discover.ViewModels
{
	public static class CodeModelView
	{
		public static string ClassCode(WMIClassPivot pivot)
		{
			string code = "";

			code += $"\tpublic class {pivot.ClassName}\n";
			code += "\t{\n";
			foreach (string name in pivot.Pivots.Where(x => x.Select).Select(x => x.Name).ToList())
			{
				code += $"\t\tpublic string {name}" + " { get; set; }\n";
			}
			code += "\n";
			code += $"\t\tpublic {pivot.ClassName}(WMIRecord data)\n";
			code += "\t\t{\n";
			foreach (string name in pivot.Pivots.Where(x => x.Select).Select(x => x.Name).ToList())
			{
				code += $"\t\t\t{name} = data.Properties[\"{name}\"];\n";
			}
			code += "\t\t}\n";
			code += "\t}\n";

			return code;
		}

		public static string ClassList(WMIClassPivot pivot)
		{
			string code = "";

			code += $"\tpublic class {pivot.ClassName}_List\n";
			code += "\t{\n";
//			code += "\t\tpublic string ComputerName { get; set; }\n";
			code += $"\t\tpublic List<{pivot.ClassName}> Items = new List<{pivot.ClassName}>();\n";
			code += "\n";
			code += $"\t\tpublic {pivot.ClassName}_List(string WMIClass, string members)\n";
			code += "\t\t{\n";
	//		code += "\t\t\tComputerName = System.Environment.MachineName;\n";
			code += "\t\t\tCollectWmiClass(WMIClass, members);\n";
			code += "\t\t}\n";
			code += "\n";
			code += "\t\tprivate async void CollectWmiClass(string wmiClass, string members)\n";
			code += "\t\t{\n";
			code += "\t\t\tItems.Clear();\n";
			code += "\n";
			code += "\t\t\ttry\n";
			code += "\t\t\t{\n";
			code += "\t\t\t\tforeach (ManagementObject managementObject in WMIList.GetCollection(wmiClass, members))\n";
			code += "\t\t\t\t{\n";
			code += "\t\t\t\t\tWMIRecord record = new WMIRecord(members);\n";
			code += "\t\t\t\t\tforeach (PropertyData propertyData in managementObject.Properties)\n";
			code += "\t\t\t\t\t{\n";
			code += "\t\t\t\t\t\trecord.ProcessProperty(propertyData);\n";
			code += "\t\t\t\t\t}\n";
			code += $"\t\t\t\t\tItems.Add(new {pivot.ClassName}(record));\n";
			code += "\t\t\t\t}\n";
			code += "\t\t\t}\n";
			code += "\t\t\tcatch (Exception ex)\n";
			code += "\t\t\t{\n";
			code += $"\t\t\t\tMessageBox.Show($\"Querying the WMI {pivot.ClassName} has an exception:\\";
			code += "n{ex.Message}\", \"Exception\", MessageBoxButton.OK, MessageBoxImage.Exclamation);\n";
			code += "\t\t\t}\n";
			code += "\t\t}\n";
			code += "\t}\n";

			return code;
		}

		public static string ClassApp(WMIClassPivot pivot)
		{
			string members = string.Empty;
			foreach (string name in pivot.Pivots.Where(x => x.Select).Select(x => x.Name).ToList())
			{
				if (!string.IsNullOrEmpty(members))
				{
					members += ",";
				}
				members += name;
			}
			string code = "";

			code += $"\t\tpublic {pivot.ClassName}_List {pivot.ClassName.Substring(0, 1).ToLower() + pivot.ClassName.Substring(1)} = new {pivot.ClassName}_List(\"{pivot.ClassName}\", \"{members}\");\n";

			return code;
		}
	}
}