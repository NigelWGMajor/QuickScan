using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel; // for INotifyPropertychanged
using System.Collections.ObjectModel; // for ObservableCollection
using System.Windows.Input;
using QuickScan.Properties;

// for routed command
namespace QuickScan
{
    class QuickScanVM : INotifyPropertyChanged
    {
        public static bool AllowAll { get { return _extensions[0].IsActive; } }
        public static string UserName { get; set; }
        public static string FromDate { get; set; }
        public static string ToDate { get; set; }

        private static ObservableCollection<Extension> _extensions;
        public ObservableCollection<Extension> Extensions
        {
            get { return _extensions; }
            set
            {
                _extensions = value;

                NotifyPropertyChanged("Extensions");
            }
        }
        private static string[] _allowableExtensions;
        public static string[] AllowableExtensions
        {
            get
            {
                if (_allowableExtensions == null)
                    _allowableExtensions = _extensions.Where(e => e.IsActive).Select(e => e.Name).ToArray();
                return _allowableExtensions;
            }
            set
            {	// used to clear
                _allowableExtensions = value;
            }
        }


        private bool _ignoreLeftOnly;
        public bool IgnoreLeftOnly
        {
            get { return _ignoreLeftOnly; }
            set
            {
                if (value == _ignoreLeftOnly) return;
                _ignoreLeftOnly = value;
                NotifyPropertyChanged("IgnoreLeftOnly");
            }
        }

        private bool _ignoreRightOnly;
        public bool IgnoreRightOnly
        {
            get { return _ignoreRightOnly; }
            set
            {
                if (value == _ignoreRightOnly) return;
                _ignoreRightOnly = value;
                NotifyPropertyChanged("IgnoreRightOnly");
            }
        }


        public static string EditCommand
        {
            get
            {
                return Settings.Default.EditCommand;
            }
            set
            {
                Settings.Default.EditCommand = value;
            }
        }


        public static string MergeCommand
        {
            get
            {
                return Settings.Default.MergeCommand;
            }
            set
            {
                Settings.Default.MergeCommand = value;
            }
        }
        
        public static string EditArguments
        {
            get { return Settings.Default.EditArguments; }
            set
            {
                Settings.Default.EditArguments =  value;
                }
        }
        
        public static string MergeArguments
        {
            get
            {
                return Settings.Default.MergeArguments;
            }
            set
            {
                Settings.Default.MergeArguments  = value;
            }
        }
        private void Launch(object c)
        {
            throw new NotImplementedException();
        }
        private Paths _paths = new Paths(null, null);
        public Paths Paths { get { return _paths; } }
        private bool _showMatched;
        public bool ShowMatched
        {
            get { return _showMatched; }
            set
            {
                _showMatched = value;
                Refresh();
            }
        }
        private bool _showObj;
        public bool ShowObj
        {
            get { return _showObj; }
            set
            {
                _showObj = value;
                Refresh();
            }
        }

        private bool _showNode;

        public bool ShowNode
        {
            get
            {
                return _showNode;
            }
            set
            {
                _showNode = value;
                Refresh();
            }
        }
        private FolderComparison _folderComparison;
        public static FileComparison LastFileComparison;
        static QuickScanVM()
        {
            _extensions = new ObservableCollection<Extension>();
        }
        public QuickScanVM()
        {
            initializeExtensions();
        }
        private void initializeExtensions()
        {
            if (Extensions == null || Extensions.Count == 0)
                Extensions = new ObservableCollection<Extension>
                {
                    new Extension(".*", false), // must be first in list!
                    new Extension(".accessor", false),
                    new Extension(".asmx", false),
                    new Extension(".bat", false),
                    new Extension(".c"),
                    new Extension(".cmd", false),
                    new Extension(".coffee"),
                    new Extension(".config"),
                    new Extension(".cpp"),
                    new Extension(".cs"),
                    new Extension(".csproj", false),
                    new Extension(".css"),
                    new Extension(".g.cs", false),
                    new Extension(".html"),
                    new Extension(".idl", false),
                    new Extension(".licx", false),
                    new Extension(".log"),
                    new Extension(".js"),
                    new Extension(".json"),
                    new Extension(".map", false),
                    new Extension(".markdown"),
                    new Extension(".md"),
                    new Extension(".npmignore", false),
                    new Extension(".opts", false),
                    new Extension(".png", false),
                    new Extension(".resx", false),
                    new Extension(".sql"),
                    new Extension(".sln", false),
                    new Extension(".snk", false),
                    new Extension(".suo", false),
                    new Extension(".targ", false),
                    new Extension(".ts"),
                    new Extension(".txt", false),
                    new Extension(".vb", false),
                    new Extension(".xaml"),
                    new Extension(".xaml.cs"),
                    new Extension(".xml", false),
                    new Extension(".xslt", false),
                    new Extension(".xsd", false),
                    new Extension(".xsl", false),
                    new Extension(".watchr", false),
                    new Extension(".yml", false),
                };
        }

        public QuickScanVM(Paths paths)
            : this()
        {
            _paths = paths;
            _folderComparison = new FolderComparison(null, paths, _showMatched, _showNode, _ignoreLeftOnly, _ignoreRightOnly);
            NotifyPropertyChanged("Folders", "LeftPath", "RightPath", "Files", "Root");
        }
        public static string GetFilters()
        {
            string result = "";
            string name = UserName ?? "";
            string fromDate = FromDate ?? "";
            string toDate = ToDate ?? "";
            if (!String.IsNullOrEmpty(name))
                result += " /user:" + name;
            if (fromDate.Length + toDate.Length == 0)
                return result;
            if (fromDate == string.Empty)
            {
                // just add the to date
                result += " /v:D" + toDate.Trim();
                return result;
            }
            // else
            {
                // the fromdate goes first, then a tilde.
                result += " /v:D" + fromDate.Trim() + "~D";
            }
            if (String.IsNullOrEmpty(toDate))
                toDate = string.Format("{0:yyyy-MM-dd}", DateTime.Now);
            result += toDate;
            return result;
        }
        public ObservableCollection<FolderComparison> Root
        {
            get { if (_folderComparison != null) return _folderComparison.Root; return new ObservableCollection<FolderComparison>(); }
        }
        public string LeftPath
        {
            get { return _paths.Left; }
            set
            {
                if (value == _paths.Left) return;
                _paths.Left = value;
                NotifyPropertyChanged("Paths", "LeftPath");
            }
        }
        public string RightPath
        {
            get { return _paths.Right; }
            set
            {
                if (value == _paths.Right) return;
                _paths.Right = value;
                NotifyPropertyChanged("Paths", "RightPath");
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

        internal void ClipData()
        {
            string result = _folderComparison.BuildData();
            System.Windows.Clipboard.SetText(result);
        }
        internal void ClearAllowedExtensions()
        {	// this forces the extensions to be reevaluated before next use.
            _allowableExtensions = null;
        }
        internal void Refresh()
        {
            showBusy();
            System.Windows.Clipboard.Clear();

            _folderComparison = new FolderComparison(null, _paths, _showMatched, _showObj, _showNode, _ignoreLeftOnly, _ignoreRightOnly);
            NotifyPropertyChanged("Folders", "LeftPath", "RightPath", "Files", "Root");
            showBusy(false);
        }
        private void showBusy(bool yes = true)
        {
            MainWindow.ShowBusy = yes;
        }
        internal void ExpandAll(bool isExpanded)
        {
            showBusy();
            foreach (FolderComparison c in Root)
            {
                c.AllExpanded = isExpanded;
            }
            showBusy(false);
        }
        internal void CheckoutLeft()
        {
            checkoutLeft(Root.ToList());
        }
        private void checkoutLeft(List<FolderComparison> folders)
        {
            foreach (var folder in folders)
            {
                foreach (FileComparison file in folder.Files)
                    if (file.IsChecked)
                        file.CheckOutLeft();
                checkoutLeft(folder.Folders.ToList());
            }
        }
        internal void CheckDifferent()
        {
            checkDifferent(Root.ToList());
        }
        private void checkDifferent(List<FolderComparison> folders)
        {
            foreach (var folder in folders)
            {
                foreach (FileComparison file in folder.Files)
                    if (file.FileStatus == FileStatus.Different) file.IsChecked = true;
                checkDifferent(folder.Folders.ToList());
            }
        }
    }
}
