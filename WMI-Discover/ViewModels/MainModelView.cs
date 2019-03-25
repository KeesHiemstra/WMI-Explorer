using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using WMILib.Models;

namespace WMI_Discover.ViewModels
{
	public class MainModelView
	{
		private const string JsonFileName = @"C:\Etc\ITAM\WMI\WMIClasses.json";
		//private readonly object JsonConvert;

		private static string JsonClassDataFileName { get { return $"C:\\Etc\\ITAM\\WMI\\{WMIClassName}-Data.json"; } }
		private static string JsonClassPivotFileName { get { return $"C:\\Etc\\ITAM\\WMI\\{WMIClassName}-Pivot.json"; } }

		private MainWindow Main;
		private int _classNameCount = -1;
		private bool WMIClassesUpdated = false;

		public static ObservableCollection<WMIClass> WMIClasses { get; set; } = new ObservableCollection<WMIClass>();
		public ObservableCollection<WMIProperty> WMIProperties { get; set; } = new ObservableCollection<WMIProperty>();
		public static string WMIClassName { get; set; } // Selected class name
		public WMIClassPivot WMIClassPivot { get; set; }

		// Search panel
		public static List<string> CategoryNames { get; set; } = new List<string>();

		public static List<string> StatusNames { get; set; } = new List<string>();
		public string ClassNameContain { get; set; } = string.Empty; // Search box content
		public string CategoryName { get; set; } = string.Empty; // Category dropdown content
		public string StatusName { get; set; } = string.Empty; // Status dropdown content

		// WMIProperties results
		public bool ReadWMIPropertiesFromFile { get; set; } = false;

		public MainModelView(MainWindow main)
		{
			Main = main;
			LoadWMIClasses();

			UpdateFilterWMIClassNames();
		}

		#region On Opening and closing the application

		private void LoadWMIClasses()
		{
			if (File.Exists(JsonFileName))
			{
				using (StreamReader stream = File.OpenText(JsonFileName))
				{
					string json = stream.ReadToEnd();
					WMIClasses = JsonConvert.DeserializeObject<ObservableCollection<WMIClass>>(json);
				}
			}
		}

		public void SaveWMIClasses()
		{
			if (WMIClassesUpdated)
			{
				string json = JsonConvert.SerializeObject(WMIClasses, Formatting.Indented);
				using (StreamWriter stream = new StreamWriter(JsonFileName))
				{
					stream.Write(json);
				}
			}

			if ((WMIClassPivot != null) && WMIClassPivot.IsUpdated)
			{
				SaveWMIClassPivot();
			}
		}

		#endregion On Opening and closing the application

		#region Work on the WMIClassNames filter

		public void UpdateFilterWMIClassNames()
		{
			Main.WMIClassComboBox.ItemsSource = FilterWMIClassNames();
			string extra = "";
			if (_classNameCount != 1)
			{
				extra = "s";
			}
			Main.ClassNameCountTextBlock.Text = $"{_classNameCount} class name{extra}";

			if (WMIProperties.Count != 0)
			{
				Main.WMIPropertiesDataGrid.ItemsSource = null;
			}
		}

		public void ClearFilterSeachPanel()
		{
			Main.SearchTextBox.Text = String.Empty;
			Main.CategoryComboBox.SelectedIndex = -1;
			Main.StatusComboBox.SelectedIndex = -1;

			UpdateFilterWMIClassNames();
		}

		private List<string> FilterWMIClassNames()
		{
			IEnumerable<WMIClass> queryWMIClassNames = SubFilterWMIClassNames();

			List<string> result = queryWMIClassNames
					.Select(x => x.Name)
					.Distinct()
					.ToList();

			_classNameCount = result.Count;

			queryWMIClassNames = SubFilterWMIClassNames();

			CategoryNames = queryWMIClassNames
				.Select(x => x.Catagory)
				.Distinct()
				.ToList();

			Main.CategoryComboBox.ItemsSource = CategoryNames;

			queryWMIClassNames = SubFilterWMIClassNames();

			StatusNames = queryWMIClassNames
				.Select(x => x.Status)
				.Distinct()
				.ToList();

			Main.StatusComboBox.ItemsSource = StatusNames;

			return result;
		}

		private IEnumerable<WMIClass> SubFilterWMIClassNames()
		{
			IEnumerable<WMIClass> result = from q in WMIClasses
																		 select q;

			if (!string.IsNullOrEmpty(ClassNameContain))
			{
				result = result
					.Where(x => x.Name.ToLower().Contains(ClassNameContain.ToLower()));
			}

			if (!string.IsNullOrEmpty(CategoryName))
			{
				result = result
					.Where(x => x.Catagory == CategoryName);
			}

			if (!string.IsNullOrEmpty(StatusName))
			{
				result = result
					.Where(x => x.Status == StatusName);
			}

			return result;
		}

		#endregion Work on the WMIClassNames filter

		#region Act on selections of SearchBox has effect on the WMIClassNames

		public void SearchTextBoxOnKey(KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Main.WMIClassComboBox.Focus(); // Forces to send the box content to memory
				UpdateFilterWMIClassNames();
			}
		}

		#endregion Act on selections of SearchBox has effect on the WMIClassNames

		#region Act on selected WMIClassName [WMIClasses]

		public async Task<bool> SelectWMIClassName(string wMIClassName)
		{
			// Turn off the pivot tab and save the pivot Json
			DisableExtraTabs();

			WMIClassName = wMIClassName;
			bool collected = false;

			#region Get and save the properties

			if (File.Exists(JsonClassDataFileName))
			{
				try
				{
					using (StreamReader stream = File.OpenText(JsonClassDataFileName))
					{
						string json = await stream.ReadToEndAsync();
						WMIProperties = JsonConvert.DeserializeObject<ObservableCollection<WMIProperty>>(json);
					}
					collected = true;
					ReadWMIPropertiesFromFile = true;
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Error reading json [{JsonClassDataFileName}]:\n{ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
			else
			{
				ReadWMIPropertiesFromFile = false;
				try
				{
					collected = await CollectWMIClass();

					if (collected && WMIProperties.Count > 0)
					{
						string json = JsonConvert.SerializeObject(WMIProperties, Formatting.Indented);
						using (StreamWriter stream = new StreamWriter(JsonClassDataFileName))
						{
							await stream.WriteAsync(json);
						}

						UpdateWMIClassCollection("OK");
						WMIClassesUpdated = true;
					}
					else if (!collected)
					{
						UpdateWMIClassCollection("Error");
						WMIClassesUpdated = true;
					}
					else if (WMIProperties.Count == 0)
					{
						UpdateWMIClassCollection("Empty");
						WMIClassesUpdated = true;
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Collecting WMI Class error:\n{ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}

			#endregion Get and save the properties

			#region MainWindow <PropertiesCountTextBlock> update

			if (collected && WMIProperties.Count == 0)
			{
				Main.ClassResultTextBlock.Text = "Empty class";
				Main.ClassResultTextBlock.Visibility = Visibility.Visible;
			}
			else
			{
				Main.ClassResultTextBlock.Visibility = Visibility.Collapsed;
			}

			string extra = "y";
			if (WMIProperties.Count != 1)
			{
				extra = "ies";
			}
			Main.PropertiesCountTextBlock.Text = $"{WMIProperties.Count} propert{extra}"; // property or properties

			#endregion MainWindow <PropertiesCountTextBlock> update

			if (collected && WMIProperties.Count > 0)
			{
				await GetWMIClassCollectionPivot(WMIProperties);
			}

			return collected;
		}

		private async Task<bool> CollectWMIClass()
		{
			if (string.IsNullOrEmpty(WMIClassName))
			{
				MessageBox.Show("There is no selected WMI Class", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				return false;
			}

			WMIProperties.Clear();

			#region Preparation

			Cursor cursor = Main.Cursor;
			Main.Cursor = Cursors.Wait;
			Thread.Sleep(100);

			Main.WMIClassComboBox.Focus();
			Main.WMIClassComboBox.IsDropDownOpen = false;

			ConnectionOptions options = new ConnectionOptions
			{
				Impersonation = System.Management.ImpersonationLevel.Impersonate
			};

			ManagementScope scope = new ManagementScope("\\\\.\\root\\cimv2", options);
			scope.Connect();

			//Query system for Operating System information
			ObjectQuery query = new ObjectQuery($"SELECT * FROM {WMIClassName}");

			ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

			ManagementObjectCollection queryCollection = searcher.Get();

			#endregion Preparation

			try
			{
				int collectionIndex = 0;
				foreach (ManagementObject managementObject in queryCollection)
				{
					int propertyIndex = 0;
					foreach (PropertyData propertyData in managementObject.Properties)
					{
						WMIProperties.Add(new WMIProperty(collectionIndex, propertyIndex, propertyData));
						propertyIndex++;
					}
					collectionIndex++;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error: {ex.Message}", "Exception error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				Main.Cursor = cursor;
			}

			//Main.WMIPropertiesDataGrid.ItemsSource = WMIProperties;
			return true;
		}

		private void UpdateWMIClassCollection(string status)
		{
			IEnumerable<WMIClass> result = from q in WMIClasses
																		 select q;

			result = result
				.Where(x => x.Name == WMIClassName);

			WMIClass record = result
					.SingleOrDefault();

			record.Status = status;
		}

		#endregion Act on selected WMIClassName [WMIClasses]

		#region Create pivot for success loaded WMIClass

		private async Task GetWMIClassCollectionPivot(ObservableCollection<WMIProperty> _WMIProperties)
		{
			try
			{
				if (File.Exists(JsonClassPivotFileName))
				{
					using (StreamReader stream = File.OpenText(JsonClassPivotFileName))
					{
						string json = stream.ReadToEnd();
						WMIClassPivot = JsonConvert.DeserializeObject<WMIClassPivot>(json);
					}
					WMIClassPivot.IsUpdated = false;
				}
				else
				{
					WMIClassPivot = new WMIClassPivot(_WMIProperties, WMIClassName);
				}

				Main.UniqueNameCountTextBlock.Text = $"{WMIClassPivot.MemberCount} member{((WMIClassPivot.MemberCount == 1) ? "" : "s")}";
				Main.CollectionCountTextBlock.Text = $"{WMIClassPivot.CollectionCount} collect{((WMIClassPivot.CollectionCount == 1) ? "y" : "ies")}";

				Main.PropertyHeaderCountTextBlock.Text = $"{WMIClassPivot.MemberCount * WMIClassPivot.CollectionCount} properties expected, " +
					$"{WMIClassPivot.PropertyCount} propert{((WMIClassPivot.PropertyCount == 1) ? "y" : "ies")} counted";

				EnableExtraTabs();

				WMIClassPivot.IsUpdated = WMIClassPivot.IsUpdated && !File.Exists(JsonClassPivotFileName);
				if (WMIClassPivot.IsUpdated)
				{
					SaveWMIClassPivot();
				}
			}
			catch
			{
				// Class needs to be marked as error
			}
		}

		private void EnableExtraTabs()
		{
			Main.PivotDataGrid.ItemsSource = WMIClassPivot.Pivots;
			Main.PivotTabItem.IsEnabled = true;

			Main.CodeTabItem.IsEnabled = true;

			Main.ExportTabItem.IsEnabled = true;
		}

		public void WriteCodeTextBox()
		{
			if (WMIClassPivot.NeedUpdate)
			{
				Main.ClassCodeTextBox.Text = CodeModelView.ClassCode(WMIClassPivot);
				Main.ClassListTextBox.Text = CodeModelView.ClassList(WMIClassPivot);
				Main.ClassAppTextBox.Text = CodeModelView.ClassApp(WMIClassPivot);

				WMIClassPivot.NeedUpdate = false;
			}
		}

		public void PrepareExportToClipboard()
		{
			ExportModelView.ExportNames = WMIClassPivot.Pivots.Where(x => x.Select).Select(x => x.Name).ToList();
		}

		public void DisableExtraTabs()
		{
			if (Main.PivotTabItem.IsEnabled && WMIClassPivot.IsUpdated)
			{
				SaveWMIClassPivot();
			}
			Main.MainTabControl.SelectedIndex = 0;
			Main.PivotTabItem.IsEnabled = false;
			Main.PivotDataGrid.ItemsSource = null;

			Main.CodeTabItem.IsEnabled = false;

			Main.TableTabItem.IsEnabled = false;
		}

		#endregion Create pivot for success loaded WMIClass

		public void ActOnPivotTabItem(bool IsEnabledChanged)
		{
			if (WMIClassPivot.IsUpdated)
			{
				SaveWMIClassPivot();
			}
		}

		private void SaveWMIClassPivot()
		{
			if (WMIClassPivot.IsUpdated)
			{
				string json = JsonConvert.SerializeObject(WMIClassPivot, Formatting.Indented);
				using (StreamWriter stream = new StreamWriter(JsonClassPivotFileName))
				{
					stream.Write(json);
				}
			}
			WMIClassPivot.IsUpdated = false;
		}
	}
}