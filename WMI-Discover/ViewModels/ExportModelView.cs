using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMILib.Models;

namespace WMI_Discover.ViewModels
{
	static class ExportModelView
	{
		public static List<string> ExportNames = new List<string>();
		public static int CollectionCount = -1;
		public static string Export;

		public static string ExportAsTable(ObservableCollection<WMIProperty> WMIProperties)
		{
			Export = string.Empty;
			string line = string.Empty;

			#region Header
			foreach (string col in ExportNames)
			{
				if (!string.IsNullOrEmpty(line))
				{
					line += "\t";
				}
				line += col;
			}
			Export += $"{line}\n";
			#endregion

			#region Data
			for (int row = 0; row < CollectionCount; row++)
			{
				List<WMIProperty> collectionRow = WMIProperties.Where(x => x.CollectionIndex == row).ToList();
				line = string.Empty;

				foreach (string member in ExportNames)
				{
					if (!string.IsNullOrEmpty(line))
					{
						line += "\t";
					}

					if (collectionRow.Where(x => x.Name == member) != null)
					{
						line += collectionRow.Where(x => x.Name == member).Select(x => x.Value).FirstOrDefault();
					}
					else
					{
						line += "<n/a>";
					}
				} //member

				Export += $"{line}\n";

			} //row
			#endregion

			return Export;
		}
	}
}
