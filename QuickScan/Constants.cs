using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuickScan
{
	public enum FolderStatus
	{
		Unknown = 0,
		Matched,
		Different,	// at least some files don't match
		LeftOnly,	// only on one side 
		RightOnly,  //
		Contained,  // no local changes, but some buried below...
		Empty       // contains no meaningful changes
	}
	public enum FileStatus
	{
		Unknown = 0,
		Matched,
		Different, 	// at least some files don't match
		LeftOnly,  	// only on one side 
		RightOnly,  //
		Empty       // no meaningful files
	}
}
