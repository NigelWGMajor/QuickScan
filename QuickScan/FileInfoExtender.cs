using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace QuickScan
{
	public static class FileInfoExtender
	{
		public static string GetLongExtension(this FileInfo fileInfo)
		{
			int index = fileInfo.Name.IndexOf(".");
			if (index == -1) return string.Empty;
			return fileInfo.Name.Substring(index);
		}
	}
}
