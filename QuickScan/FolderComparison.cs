using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;	// for ObservableCollection
using System.ComponentModel;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

// for INotifyPropertyChanged

namespace QuickScan
{
    class FolderComparison : ObservableCollection<FolderComparison>
    {
        public ICommand FolderChangesetsLeftCommand { get { return _folderChangesetsLeftCommand; } }
        public ICommand FolderChangesetsRightCommand { get { return _folderChangesetsRightCommand; } }
        private readonly ICommand _folderChangesetsLeftCommand, _folderChangesetsRightCommand;

        private FolderComparison()
        {
            _folderChangesetsLeftCommand = new CommandClass(c => true, c => FolderHistoryLeft());
            _folderChangesetsRightCommand = new CommandClass(c => true, c => FolderHistoryRight());
        }

        public static string LeftPath { get { return _leftPath_; } }
        public static string RightPath { get { return _rightPath_; } }
        private bool _showMatched, _showObj, _showNode;
        private static string _leftPath_, _rightPath_;
        private string _name;
        public string Name
        {
            get { return _name; }
        }
        private int _changeCount;
        public int ChangeCount
        {
            get { return _changeCount; }
        }
        public string RelativePath
        {
            get { return string.Format("...{0}", _relativePath); }
        }
        private int _fullCount;
        public int FullCount
        {
            get { return _fullCount; }
            set
            {
                if (value == _fullCount) return;
                _fullCount = value;
                RaisePropertyChanged("FullCount");
            }
        }
        public string Totals
        {
            get { return string.Format("{0:D} / {1:D}", ChangeCount, FullCount); }
        }
        public List<FileComparison> Files { get; set; }
        private FolderStatus _folderStatus;
        public FolderStatus FolderStatus
        {
            get
            { return _folderStatus; }
            set
            {
                if (value == _folderStatus) return;
                _folderStatus = value;
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Status"));
            }
        }
        public FileStatus SummaryFileState { get; set; }
        private string _relativePath = String.Empty;
        public Paths CurrentPaths
        {
            get
            {
                if (String.IsNullOrEmpty(_relativePath))
                    return new Paths(_leftPath_, _rightPath_);
                return new Paths(Path.Combine(_leftPath_, _relativePath), Path.Combine(_rightPath_, _relativePath));
            }
        }

        internal void SetFlags()
        {
            HasMatches |= Files.Any(f => f.FileStatus == FileStatus.Matched);
            HasLeftOnly |= Files.Any(f => f.FileStatus == FileStatus.LeftOnly);
            HasRightOnly |= Files.Any(f => f.FileStatus == FileStatus.RightOnly);
            HasDifferences |= Files.Any(f => f.FileStatus == FileStatus.Different);
        }
        private bool _hasMatches;
        public bool HasMatches
        {
            get { return _hasMatches; }
            set
            {
                if (value == _hasMatches)
                    return;
                _hasMatches = value;
                if (_parent != null)
                    _parent.HasMatches |= value;
                RaisePropertyChanged("HasMatches");
            }
        }

        private bool _hasLeftOnly;

        public bool HasLeftOnly
        {
            get { return _hasLeftOnly; }
            set
            {
                if (value == _hasLeftOnly)
                    return;
                _hasLeftOnly = value;
                if (_parent != null)
                    _parent.HasLeftOnly |= value;
                RaisePropertyChanged("HasLeftOnly");
            }
        }

        private bool _hasRightOnly;

        public bool HasRightOnly
        {
            get
            { return _hasRightOnly; }
            set
            {
                if (value == _hasRightOnly)
                    return;
                _hasRightOnly = value;
                if (_parent != null)
                    _parent.HasRightOnly |= value;
                RaisePropertyChanged("HasRightOnly");
            }
        }

        private bool _hasDifferences;
        public bool HasDifferences
        {
            get { return _hasDifferences; }
            set
            {
                if (value == _hasDifferences)
                    return;
                _hasDifferences = value;
                if (_parent != null)
                    _parent.HasDifferences |= value;
                RaisePropertyChanged("HasDifferences");
            }
        }

        private bool _isShown = true;
        public bool IsShown
        {
            get { return _isShown; }
            set
            {
                if (value == _isShown) return;
                _isShown = value;
                RaisePropertyChanged("IsShown");
                if (_parent != null)
                    _parent.RaisePropertyChanged("Folders");
            }
        }

        public ObservableCollection<FolderComparison> Root
        {	// used to generate a level for the root.
            get
            {
                ObservableCollection<FolderComparison> result = new ObservableCollection<FolderComparison>();
                result.Add(this);
                return result;
            }
        }
        public override string ToString()
        {
            return Name;
        }
        private static bool _hasRun = false;
        public void CompareFolders(FolderComparison parent, Paths paths, bool showMatched = true, bool showObj = false, bool showNode = false, bool ignoreLeftOnly=false, bool ignoreRightOnly = false)
        {
            _parent = parent;
            _ignoreLeftOnly = ignoreLeftOnly;
            _ignoreRightOnly = ignoreRightOnly;
            _showMatched = showMatched;
            _showObj = showObj;
            _showNode = showNode;
            Folders.Clear();
            Files.Clear();
            _leftPath_ = paths.Left;
            _rightPath_ = paths.Right;
            _relativePath = string.Empty;
            ProcessCurrentPath(paths);
        }
        public FolderComparison(FolderComparison parent, Paths paths, bool showMatched = true, bool showObj = false, bool showNode = false, bool ignoreLeftOnly=false, bool ignoreRightOnly = false)
            : this()
        {
            _parent = parent;
            _ignoreLeftOnly = ignoreLeftOnly;
            _ignoreRightOnly = ignoreRightOnly;
            _showMatched = showMatched;
            _showObj = showObj;
            _showNode = showNode;
            if (!_hasRun) { _leftPath_ = paths.Left; _rightPath_ = paths.Right; }
            else
            {
                _relativePath = paths.Left.Substring(_leftPath_.Length);
                if (_relativePath.Length > 0 && _relativePath[0] == '\\')
                    _relativePath = _relativePath.Substring(1); //  remove any leading backslash
            }
            _hasRun = true;
            ProcessCurrentPath(paths);
        }

        private void ProcessCurrentPath(Paths paths)
        {
            getComparisons(paths);
            _changeCount = Files.Where(fi => fi.FileStatus != FileStatus.Matched).Count() +
                Folders.Select(fo => fo.ChangeCount).Sum();
            _fullCount = Files.Count() + Folders.Sum(fo => fo.FullCount);
            if (_showMatched | (FolderStatus == QuickScan.FolderStatus.Matched && _changeCount > 0))
                FolderStatus = QuickScan.FolderStatus.Contained;
            if (!_showMatched) Prune();
            SetFlags();
        }
        public void Prune()
        {
            Files = Files.Except(Files.Where(fi => fi.FileStatus == FileStatus.Matched)).ToList();
            foreach (FolderComparison folder in Folders)
            {
                folder.Prune();
            }
            var temp = Folders.Except(Folders.Where(fo => fo.FolderStatus == QuickScan.FolderStatus.Matched || fo.FolderStatus == QuickScan.FolderStatus.Empty)).ToList();
            ClearItems();
            foreach (FolderComparison item in temp)
                Items.Add(item);
            if (Folders.Count == 0 && Files.Count == 0) FolderStatus = QuickScan.FolderStatus.Empty;
        }
        public ObservableCollection<FolderComparison> Folders
        { get { return new ObservableCollection<FolderComparison>(this.Where(fc => fc.IsShown)); } }
        private void getComparisons(Paths paths)
        {	// do files:
            DirectoryInfo left;
            DirectoryInfo right;
            UpdateFiles(out left, out right);
            _name = left.Name;
            if (!String.Equals(_name, right.Name, StringComparison.CurrentCultureIgnoreCase))
                _name = String.Concat(_name, "/", right.Name);
            //do folders (recursive, duh!)
            if (left.Exists)
                foreach (DirectoryInfo directoryInfo in getQualifyingDirectories(left.GetDirectories()))
                    this.Add(new FolderComparison(this, getPaths(this, directoryInfo.Name), _showMatched, _showObj, _showNode, _ignoreLeftOnly, _ignoreRightOnly));
            else FolderStatus = QuickScan.FolderStatus.RightOnly;
            if (!_ignoreRightOnly || left.Exists)
            {
                if (right.Exists)
                    foreach (DirectoryInfo DirectoryInfo in getQualifyingDirectories(right.GetDirectories()))
                    {
                        if (this.Where(
                                f => String.Equals(f.Name, DirectoryInfo.Name,
                                    StringComparison.CurrentCultureIgnoreCase)).Count() == 0)
                            this.Add(new FolderComparison(this, getPaths(this, DirectoryInfo.Name), _showMatched, _showObj, _showNode, _ignoreLeftOnly, _ignoreRightOnly));
                    }
                else FolderStatus = QuickScan.FolderStatus.LeftOnly;
            }

            if (FolderStatus == QuickScan.FolderStatus.Unknown)
                if (Files.Where(f => f.FileStatus != FileStatus.Matched).Count() == 0)
                    FolderStatus = QuickScan.FolderStatus.Matched;
                else
                    FolderStatus = QuickScan.FolderStatus.Different;
        }

        private IEnumerable<DirectoryInfo> getQualifyingDirectories(DirectoryInfo[] directoryInfo)
        {
            // we may need to filter out certain directories...
            if (_showObj && _showNode)
                return directoryInfo;
            else if (_showNode)
                return (directoryInfo.Where(di => di.Name != "obj"));
            else if (_showObj)
                return (directoryInfo.Where(di => !di.Name.StartsWith("node_")));
            else
                return (directoryInfo.Where(di => di.Name != "obj" && !di.Name.StartsWith("node_")));
        }
        private Paths getPaths(FolderComparison folderComparison, string localPath)
        {
            return new Paths(
                Path.Combine(folderComparison.CurrentPaths.Left, localPath),
                Path.Combine(folderComparison.CurrentPaths.Right, localPath));
        }
        private void UpdateFiles(out DirectoryInfo left, out DirectoryInfo right)
        {
            QuickScanVM.AllowableExtensions = null; // force rescan.
            Files = new List<FileComparison>();
            left = new DirectoryInfo(Path.Combine(_leftPath_, _relativePath));
            if (left.Exists)
                foreach (FileInfo fileInfo in left.GetFiles())
                    if (FileQualifies(fileInfo.GetLongExtension())) Files.Add(new FileComparison(this, fileInfo.Name));
                    else if (FileQualifies(fileInfo.Extension)) Files.Add(new FileComparison(this, fileInfo.Name));

            right = new DirectoryInfo(Path.Combine(_rightPath_, _relativePath));
            if (!_ignoreRightOnly || left.Exists)
            {
                if (right.Exists)
                    foreach (FileInfo fileInfo in right.GetFiles())
                        if (Files.Where(
                                    f => String.Equals(f.Name, fileInfo.Name,
                                        StringComparison.CurrentCultureIgnoreCase))
                                .Count() == 0)
                            if (FileQualifies(fileInfo.GetLongExtension()))
                                Files.Add(new FileComparison(this, fileInfo.Name));
                            else if (FileQualifies(fileInfo.Extension))
                                Files.Add(new FileComparison(this, fileInfo.Name));
            }

            if (_ignoreRightOnly)
                Files = Files.Where(f => f.FileStatus != FileStatus.RightOnly).ToList();
            if (_ignoreLeftOnly)
                Files=Files.Where(f => f.FileStatus != FileStatus.LeftOnly).ToList();

            if (Files.Where(f => f.FileStatus == FileStatus.Matched).Count() == Files.Count())
                SummaryFileState = FileStatus.Matched;
            else
                SummaryFileState = FileStatus.Different;
        }
        private bool FileQualifies(string extension)
        {
            if (QuickScanVM.AllowAll) return true;
            return (QuickScanVM.AllowableExtensions.Contains(extension.ToLower()));
        }
        internal string BuildData()
        {	// need to mod to exclude folders without checked items!
            StringBuilder sb = new StringBuilder();
            if (_folderStatus != QuickScan.FolderStatus.Matched)
            {
                foreach (FileComparison file in Files)
                    if (file.IsChecked)
                        sb.AppendFormat("\r\n{0}::{1}  {2}", Name, file.Name, file.Comment);
            }
            foreach (FolderComparison folder in Folders)
            {
                string temp = folder.BuildData();
                if (temp.Length > 0)
                    sb.AppendLine(temp);
            }
            return sb.ToString();
        }

        internal void ResetVisibility()
        {
            foreach (var f in Folders)
                f.ResetVisibility();
            IsShown = true;
        }
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    RaisePropertyChanged("IsExpanded");
                }
            }
        }
        private bool _isExpanded;

        private bool _ignoreLeftOnly, _ignoreRightOnly;
        private FolderComparison _parent;

        public bool AllExpanded
        {	// handles recursive expansion
            //get { return _isExpanded; }
            set
            {
                // if (value != _isExpanded) // <(-- omit this check or we won't necessarily ecurse to the full depth.
                {
                    _isExpanded = value;
                    foreach (FolderComparison fc in Folders)
                        fc.AllExpanded = value;
                    RaisePropertyChanged("IsExpanded");
                }
            }
        }

        #region INotifyPropertyChanged Members


        private void RaisePropertyChanged(params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
                OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        internal void CheckoutLeftChecked()
        {
            foreach (FileComparison f in Files)
                if (f.IsChecked)
                    f.CheckOutLeft();
        }
        internal void CheckDifferent()
        {
            foreach (FileComparison f in Files)
                if (f.FileStatus == FileStatus.Different)
                    f.IsChecked = true;
        }

        internal void FolderHistoryLeft()
        {
            try
            {
                string appPath = FileComparison._tfsPath_;
                string target = "history \"" + CurrentPaths.Left + "\" /recursive" + QuickScanVM.GetFilters();
                System.Diagnostics.Process.Start(appPath, target);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sorry, TFS History failed.");
            }
        }
        internal void FolderHistoryRight()
        {
            try
            {
                string appPath = FileComparison._tfsPath_;
                string target = "history \"" + CurrentPaths.Right + "\" /recursive" + QuickScanVM.GetFilters();
                System.Diagnostics.Process.Start(appPath, target);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sorry, TFS History failed.");
            }
        }
    }
}
