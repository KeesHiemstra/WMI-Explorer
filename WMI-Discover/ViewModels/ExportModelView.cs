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
		public static string Export;

		public static void ExportAsTable(ObservableCollection<WMIProperty> WMIProperties)
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

			
		}
	}
}
