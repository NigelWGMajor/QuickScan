using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel; // for INotifyPropertyChanged

namespace QuickScan
{
	class Paths : INotifyPropertyChanged
	{
		private string _left, _right;
		public string Left
		{
			get { return _left; }
			set
			{
				if (value == _left) return;
				_left = value;
				NotifyPropertyChanged("Left", "LeftPath");
			}
		}
		public string Right 
		{ get{ return _right; }
			set
			{
				if (value == _right) return;
				_right = value;
				NotifyPropertyChanged("Right", "RightPath");
			}
		}
		public Paths(string left, string right)
		{
			Left = left; 
			Right = right;
		}
		public bool IsComplete
		{
			get
			{
				return !(String.IsNullOrEmpty(Left) || String.IsNullOrEmpty(Right));
			}
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
