using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
namespace QuickScan
{
	class Extension : INotifyPropertyChanged
	{
		public string Name { get; set; }
		private bool _isActive;
		public bool IsActive 
		{ 
			get { return _isActive; }
			set
			{
				if (value == _isActive) return;
				_isActive = value;
				NotifyPropertyChanged("IsActive");
			}
		}
		public Extension(string name, bool active=true )
		{
			Name = name;
			IsActive = active;
		}
		#region    	  INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		// This method is called by the Set accessor of each property. 
		private void NotifyPropertyChanged(params String[] propertyNames)
		{
			if (PropertyChanged != null)
			{
				foreach (string propertyName in propertyNames)
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion // INotifyPropertyChanged
	}
}
