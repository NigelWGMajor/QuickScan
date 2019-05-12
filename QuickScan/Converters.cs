using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;  // for IValueConverter
using System.Windows.Media; // for Colors
using System.Windows;       // for Visibility 
namespace QuickScan
{
	public class FileStatusToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			FileStatus fileState = (FileStatus)value;
			switch (fileState)
			{
				case FileStatus.Different: return new SolidColorBrush(Colors.Yellow);
				case FileStatus.LeftOnly: return new SolidColorBrush(Colors.Red);
				case FileStatus.Matched: return new SolidColorBrush(Colors.White);
				case FileStatus.RightOnly: return new SolidColorBrush(Colors.Green);
				default:
				case FileStatus.Unknown: return new SolidColorBrush(Colors.Gray);
			}
		}
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException("Not intended for round trip");
		}
	}
	public class FileStatusToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			FileStatus fileState = (FileStatus)value;
			switch (fileState)
			{

				case FileStatus.Empty: return Visibility.Collapsed;

				case FileStatus.Matched:
				default:
				case FileStatus.Different:
				case FileStatus.LeftOnly:
				case FileStatus.RightOnly:
				case FileStatus.Unknown: return Visibility.Visible;
			}
		}
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException("Not intended for round trip");
		}
	}
	public class FolderStatusToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			FolderStatus folderState = (FolderStatus)value;
			switch (folderState)
			{
				case FolderStatus.Different: return new SolidColorBrush(Colors.Yellow);
				case FolderStatus.LeftOnly: return new SolidColorBrush(Colors.Red);
				case FolderStatus.RightOnly: return new SolidColorBrush(Colors.Green);
				case FolderStatus.Matched: return new SolidColorBrush(Colors.White);
				case FolderStatus.Contained: return new SolidColorBrush(Colors.DarkGray);
				default:
				case FolderStatus.Unknown: return new SolidColorBrush(Colors.Gray);
			}
		}
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException("Not intended for round trip");
		}
	}
	public class FolderStatusToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			FolderStatus folderStatus = (FolderStatus)value;
			switch (folderStatus)
			{
				case FolderStatus.Matched: return Visibility.Hidden;
				
				default:
				case FolderStatus.Contained:
				case FolderStatus.Different:
				case FolderStatus.LeftOnly:
				case FolderStatus.RightOnly:
				case FolderStatus.Unknown: return Visibility.Visible;
			}
		}
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException("Not intended for round trip");
		}
	}
    public class FileStatusToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FolderStatus folderStatus = (FolderStatus)value;
            switch (folderStatus)
            {
                case FolderStatus.Matched: return "Identical (shift to swap in viewer)";

                default:
                case FolderStatus.Contained: return "Contained";
                case FolderStatus.Different: return "Different (shift to swap in viewer)";
                case FolderStatus.LeftOnly: return "Left only";
                case FolderStatus.RightOnly: return "Right only";
                case FolderStatus.Unknown: return "Unknown";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Not intended for round trip");
        }
    }

    public class FalseToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool) value)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not intended for two-way conversion");
        }
    }

    public class FalseToDimmedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool) value)
                return 1.0;
            return 0.2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not intended for two-way conversion");
        }
    }
}
