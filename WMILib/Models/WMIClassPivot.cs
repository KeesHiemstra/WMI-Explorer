using Newtonsoft.Json;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WMILib.Models
{
	public class WMIClassPivot
	{
		private string _className;
		private int _propertyCount = -1;
		private int _collectionCount = -1;
		private int _memberCount = -1;
		private bool _isUpdated = false;

		private ObservableCollection<WMIProperty> properties { get; }
		private List<string> UniqueNames = new List<string>();

		public string ClassName
		{
			get => _className;
			set
			{
				if (_className != value)
				{
					_className = value;
					IsUpdated = true;
				}
			}
		}

		public int PropertyCount
		{
			get => _propertyCount;
			set
			{
				if (_propertyCount != value)
				{
					_propertyCount = value;
					IsUpdated = true;
				}
			}
		}

		public int CollectionCount
		{
			get => _collectionCount;
			set
			{
				if (_collectionCount != value)
				{
					_collectionCount = value;
					IsUpdated = true;
				}
			}
		}

		public int MemberCount
		{
			get => _memberCount;
			set
			{
				if (_memberCount != value)
				{
					_memberCount = value;
					IsUpdated = true;
				}
			}
		}

		[JsonIgnore]
		public bool IsUpdated
		{
			get
			{
				foreach (bool item in Pivots.Select(x => x.IsUpdated).Distinct().ToList())
				{
					_isUpdated = _isUpdated || item;
				}

				NeedUpdate = NeedUpdate || _isUpdated;
				return _isUpdated;
			}
			set => _isUpdated = value;
		}

		[JsonIgnore]
		public bool NeedUpdate { get; set; } = false;

		public ObservableCollection<WMIPropertyPivot> Pivots = new ObservableCollection<WMIPropertyPivot>();

		public WMIClassPivot()
		{
		}

		public WMIClassPivot(ObservableCollection<WMIProperty> Properties, string className)
		{
			properties = Properties;

			ClassName = className;

			// Count all properties
			PropertyCount = properties.Count();

			// Count the number of collections
			CollectionCount = properties.Max(x => x.CollectionIndex) + 1;

			// List all unique property Name
			UniqueNames = CreateUniqueNames();

			// Count the occurrences on Name
			MemberCount = UniqueNames.Count;

			// In theory PropertyCount = CollectionCount * UniqueNameCount
			// >> This will not happen with Win32_Account WMIClass, this WMIClass is not inconsistent.

			foreach (string UniqueName in UniqueNames)
			{
				GetPivots(UniqueName);
			}
		}

		private List<string> CreateUniqueNames()
		{
			// Collect all unique property names
			IEnumerable<WMIProperty> result = from q in properties
																				select q;
			return result
				.Select(x => x.Name)
				.Distinct()
				.ToList();
		}

		private void GetPivots(string uniqueName)
		{
			IEnumerable<WMIProperty> queryProperty = from q in properties
																							 select q;
			queryProperty = queryProperty
				.Where(x => x.Name == uniqueName)
				.ToList();

			WMIPropertyPivot pivot = new WMIPropertyPivot(uniqueName);

			GetTypePivots(queryProperty, pivot);
			GetValuePivots(queryProperty, pivot);

			Pivots.Add(pivot);
		}

		#region Type information

		private void GetTypePivots(IEnumerable<WMIProperty> queryProperty, WMIPropertyPivot pivot)
		{
			GetTypePivot(queryProperty, pivot);
			GetTypeOccPivot(queryProperty, pivot);
		}

		private void GetTypePivot(IEnumerable<WMIProperty> queryProperty, WMIPropertyPivot pivot)
		{
			List<string> Types = new List<string>();
			Types = queryProperty
				.Select(x => x.Type)
				.Distinct()
				.ToList();

			pivot.TypeCount = Types.Count;

			if (Types.Count == 1)
			{
				pivot.Type = Types[0];
			}
			else
			{
				string result = "";
				foreach (string item in Types)
				{
					if (!string.IsNullOrEmpty(result))
					{
						result += ",";
					}
					result += item;
					pivot.Type = "{" + result + "}";
				}
			}
		}

		private void GetTypeOccPivot(IEnumerable<WMIProperty> queryProperty, WMIPropertyPivot pivot)
		{
			List<string> Types = new List<string>();
			Types = queryProperty
				.Select(x => x.Type)
				.ToList();

			pivot.TypeOcc = Types.Count;
		}

		#endregion Type information

		#region Value information

		private void GetValuePivots(IEnumerable<WMIProperty> queryProperty, WMIPropertyPivot pivot)
		{
			GetValuePivot(queryProperty, pivot);
			GetUniqueValuePivot(queryProperty, pivot);
			GetCleanUniqueValuePivot(queryProperty, pivot);
		}

		private void GetValuePivot(IEnumerable<WMIProperty> queryProperty, WMIPropertyPivot pivot)
		{
			List<string> Values = new List<string>();
			Values = queryProperty
				.Select(x => x.Value)
				.ToList();

			pivot.ValueCount = Values.Count();
		}

		private void GetUniqueValuePivot(IEnumerable<WMIProperty> queryProperty, WMIPropertyPivot pivot)
		{
			List<string> Values = new List<string>();
			Values = queryProperty
				.Select(x => x.Value)
				.Distinct()
				.ToList();

			pivot.ValueUniqueCount = Values.Count();
		}

		private void GetCleanUniqueValuePivot(IEnumerable<WMIProperty> queryProperty, WMIPropertyPivot pivot)
		{
			List<string> Values = new List<string>();
			Values = queryProperty
				.Where(x => !((x.Value == "<null>") || (x.Value == "<n/a>") || string.IsNullOrEmpty(x.Value)))
				.Select(x => x.Value)
				.Distinct()
				.ToList();

			pivot.ValueCleanCount = Values.Count();
		}

		#endregion Value information
	}
}