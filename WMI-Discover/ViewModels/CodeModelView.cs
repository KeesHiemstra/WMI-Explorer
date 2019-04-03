using System.Linq;

using WMILib.Models;

namespace WMI_Discover.ViewModels
{
	public static class CodeModelView
	{
		public static string ClassCode(WMIClassPivot pivot)
		{
			string code = string.Empty;

			code += $"\tpublic class {pivot.ClassName}\r\n";
			code += "\t{\r\n";
			foreach (string name in pivot.Pivots.Where(x => x.Select).Select(x => x.Name).ToList())
			{
				code += $"\t\tpublic string {name}" + " { get; set; }\r\n";
			}
			code += "\r\n";
			code += $"\t\tpublic {pivot.ClassName}() {{ }}\r\n";
			code += "\r\n";
			code += $"\t\tpublic {pivot.ClassName}(WMIRecord data)\r\n";
			code += "\t\t{\r\n";
			foreach (string name in pivot.Pivots.Where(x => x.Select).Select(x => x.Name).ToList())
			{
				code += $"\t\t\t{name} = data.Properties[\"{name}\"];\r\n";
			}
			code += "\t\t}\r\n";
			code += "\t}\r\n";

			return code;
		}

		public static string ClassList(WMIClassPivot pivot)
		{
			string code = "";

			code += $"\tpublic class {pivot.ClassName}_List\r\n";
			code += "\t{\r\n";
//			code += "\t\tpublic string ComputerName { get; set; }\n";
			code += $"\t\tpublic List<{pivot.ClassName}> Items = new List<{pivot.ClassName}>();\r\n";
			code += "\r\n";
			code += $"\t\tpublic {pivot.ClassName}_List(string WMIClass, string members)\r\n";
			code += "\t\t{\r\n";
	//		code += "\t\t\tComputerName = System.Environment.MachineName;\n";
			code += "\t\t\tCollectWmiClass(WMIClass, members);\r\n";
			code += "\t\t}\r\n";
			code += "\r\n";
			code += "\t\tprivate async void CollectWmiClass(string wmiClass, string members)\r\n";
			code += "\t\t{\r\n";
			code += "\t\t\tItems.Clear();\r\n";
			code += "\r\n";
			code += "\t\t\ttry\r\n";
			code += "\t\t\t{\r\n";
			code += "\t\t\t\tforeach (ManagementObject managementObject in WMIList.GetCollection(wmiClass, members))\r\n";
			code += "\t\t\t\t\r\n";
			code += "\t\t\t\t\tWMIRecord record = new WMIRecord(members);\r\n";
			code += "\t\t\t\t\tforeach (PropertyData propertyData in managementObject.Properties)\r\n";
			code += "\t\t\t\t\t\r\n";
			code += "\t\t\t\t\t\trecord.ProcessProperty(propertyData);\r\n";
			code += "\t\t\t\t\t}\n";
			code += $"\t\t\t\t\tItems.Add(new {pivot.ClassName}(record));\n";
			code += "\t\t\t\t}\r\n";
			code += "\t\t\t}\r\n";
			code += "\t\t\tcatch (Exception ex)\r\n";
			code += "\t\t\t{\r\n";
			code += $"\t\t\t\tMessageBox.Show($\"Querying the WMI {pivot.ClassName} has an exception:\\";
			code += "n{ex.Message}\", \"Exception\", MessageBoxButton.OK, MessageBoxImage.Exclamation);\r\n";
			code += "\t\t\t\r\n";
			code += "\t\t}\r\n";
			code += "\t}\r\n";

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

			code += $"\t\tpublic {pivot.ClassName}_List {pivot.ClassName.Substring(0, 1).ToLower() + pivot.ClassName.Substring(1)} = new {pivot.ClassName}_List(\"{pivot.ClassName}\", \"{members}\");\r\n";

			return code;
		}

		public static string SQL(WMIClassPivot pivot)
		{
			string code = string.Empty;

			code += $"CREATE TABLE dbo.{pivot.ClassName}(\r\n";
			code += "\t[Id] [int] IDENTITY(1, 1) NOT NULL,\r\n";
			code += "\t[ComputerName] [varchar](15) NOT NULL,\r\n";
			foreach (var item in pivot.Pivots.Where(x => x.Select))
			{
				string SQLNull = string.Empty;
				if (item.MinLength > 0)
				{
					SQLNull = "NOT ";
				}
				code += $"\t[{item.Name}] [varchar]({item.MaxLength}) {SQLNull}NULL,\r\n";
			}
			code += $"\tCONSTRAINT [PK_{pivot.ClassName}] PRIMARY KEY CLUSTERED\r\n";
			code += "\t(\r\n";
			code += "\t\t[Id] ASC\r\n";
			code += "\t) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]\r\n";
			code += ") ON [PRIMARY]\r\n";

			return code;
		}
	}
}