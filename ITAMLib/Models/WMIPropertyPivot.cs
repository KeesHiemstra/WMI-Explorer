using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ITAMLib.Models
{
	public class WMIPropertyPivot
	{
		private bool _select;
		private string _name;
		private string _type;
		private int _typeCount = -1;
		private int _typeOcc = -1;
		private int _valueCount = -1;
		private int _valueUniqueCount = -1;
		private int _valueCleanCount = -1;

		public bool Select
		{
			get => _select;
			set
			{
				if (_select != value)
				{
					_select = value;
					IsUpdated = true;
				}
			}
		}
		public string Name
		{
			get => _name;
			set
			{
				if (_name != value)
				{
					_name = value;
					IsUpdated = true;
				}
			}
		}
		public string Type
		{
			get => _type;
			set
			{
				if (_type != value)
				{
					_type = value;
					IsUpdated = true;
				}
			}
		}
		public int TypeCount
		{
			get => _typeCount;
			set
			{
				if (_typeCount != value)
				{
					_typeCount = value;
					IsUpdated = true;
				}
			}
		}
		public int TypeOcc
		{
			get => _typeOcc;
			set
			{
				if (_typeOcc != value)
				{
					_typeOcc = value;
					IsUpdated = true;
				}
			}
		}
		public int ValueCount
		{
			get => _valueCount;
			set
			{
				if (_valueCount != value)
				{
					_valueCount = value;
					IsUpdated = true;
				}
			}
		}
		public int ValueUniqueCount
		{
			get => _valueUniqueCount;
			set
			{
				if (_valueUniqueCount != value)
				{
					_valueUniqueCount = value;
					IsUpdated = true;
				}
			}
		}
		public int ValueCleanCount
		{
			get => _valueCleanCount;
			set
			{
				if (_valueCleanCount != value)
				{
					_valueCleanCount = value;
					IsUpdated = true;
				}
			}
		}

		[JsonIgnore]
		public bool IsUpdated { get; set; } = false;

		[JsonIgnore]
		public List<string> UniqueValues { get; set; }

		[JsonIgnore]
		public string Result { get { return Evaluate(); } }

		public WMIPropertyPivot(string name)
		{
			Name = name;
		}

		private string Evaluate()
		{
			string result = string.Empty;

			if (TypeCount > 1)
			{
				result = "Inconstant type";
			}

			return result;
		}
	}
}
