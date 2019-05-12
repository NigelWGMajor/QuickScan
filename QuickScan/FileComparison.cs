using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
// for INotifyPropertychanged
using System.Windows;
using System.Windows.Input;


namespace QuickScan
{
    class FileComparison : INotifyPropertyChanged
    {
        public ICommand LaunchCommand { get { return _launchCommand; } }
        public ICommand CopyCommand { get { return _copyCommand; } }
        public ICommand CheckOutRightCommand { get { return _checkOutRightCommand; } }
        public ICommand ViewInFolderLeftCommand { get { return _viewInFolderLeftCommand; } }
        public ICommand ViewInFolderRightCommand { get { return _viewInFolderRightCommand; } }
        public ICommand CheckOutLeftCommand { get { return _checkOutLeftCommand; } }
        public ICommand HistoryLeftCommand { get { return _historyLeftCommand; } }
        public ICommand HistoryRightCommand { get { return _historyRightCommand; } }
        public ICommand PushLeftCommand { get { return _pushLeftCommand; } }
        public ICommand PushRightCommand { get { return _pushRightCommand; } }

        private readonly ICommand _launchCommand, _copyCommand, _checkOutRightCommand,
            _viewInFolderLeftCommand, _viewInFolderRightCommand, _checkOutLeftCommand,
            _historyLeftCommand, _historyRightCommand, _pushLeftCommand, _pushRightCommand;

        public FileComparison()
        {
            _launchCommand = new CommandClass(c => true /*CanLaunch()*/, c => Launch());
            _copyCommand = new CommandClass(c => CanCopy(), c => Copy());
            _checkOutRightCommand = new CommandClass(c => CanCheckOutRight(), c => CheckOutRight());
            _viewInFolderLeftCommand = new CommandClass(c => CanCheckOutLeft(), c => ViewInFolderLeft());
            _pushRightCommand = new CommandClass(c => true, c => PushRight());
            _viewInFolderRightCommand = new CommandClass(c => CanCheckOutRight(), c => ViewInFolderRight());
            _checkOutLeftCommand = new CommandClass(c => CanCheckOutLeft(), c => CheckOutLeft());
            _historyLeftCommand = new CommandClass(c => CanCheckOutLeft(), c => HistoryLeft());
            _historyRightCommand = new CommandClass(c => CanCheckOutRight(), c => HistoryRight());
            _pushLeftCommand = new CommandClass(c => true, c => PushLeft());
        }

        public const string _tfsPath_ = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\tf.exe";
        private string _pathInfo;
        public string PathInfo
        {
            get { return _pathInfo; }
            set
            {
                if (value == _pathInfo) return;
                _pathInfo = value;
                raisePropertyChanged("PathInfo");
            }
        }
        public string Name { get; set; }
        private string _comment;
        public string Comment
        {
            get { return _comment; }
            set
            {
                if (_comment == value) return;
                _comment = value;
                raisePropertyChanged("Comment");
            }
        }
        private bool _isChecked = false;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked == value) return;
                _isChecked = value;
                raisePropertyChanged("IsChecked");
            }
        }
        private FolderComparison _parent;
        public FolderComparison Parent { get { return _parent; } }
        private FileStatus _status;
        public FileStatus FileStatus
        {
            get { return _status; }
            set
            {
                if (value == _status) return;
                _status = value;
                raisePropertyChanged("State");
            }
        }
        public bool IsVisible { get; set; }
        public override string ToString()
        {
            return Name;
        }
        public void Launch()
        {
            string command = "";
            string arguments = "";
            try
            {
                if (FileStatus == QuickScan.FileStatus.LeftOnly)
                {
                    command = QuickScanVM.EditCommand;
                    arguments = QuickScanVM.EditArguments.Replace("%1", Path.Combine(_parent.CurrentPaths.Left, Name));
                    //System.Diagnostics.Process.Start(command, arguments);
                }
                else if (FileStatus == QuickScan.FileStatus.RightOnly)
                {
                    command = QuickScanVM.EditCommand;
                    arguments = QuickScanVM.EditArguments.Replace("%1", Path.Combine(_parent.CurrentPaths.Right, Name));
                    //System.Diagnostics.Process.Start(command, arguments);
                }
                else
                {
                    command = QuickScanVM.MergeCommand;
                    if (Keyboard.Modifiers == ModifierKeys.None)
                    {
                        arguments = QuickScanVM.MergeArguments
                            .Replace("%1", Path.Combine(_parent.CurrentPaths.Left, Name))
                            .Replace("%2", Path.Combine(_parent.CurrentPaths.Right, Name));
                    }
                    else
                    {
                        arguments = QuickScanVM.MergeArguments
                            .Replace("%2", Path.Combine(_parent.CurrentPaths.Left, Name))
                            .Replace("%1", Path.Combine(_parent.CurrentPaths.Right, Name));
                    }
                   // System.Diagnostics.Process.Start(command, arguments);
                }
                System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(command, arguments);
                info.UseShellExecute = true;
                info.WindowStyle = ProcessWindowStyle.Hidden;

                System.Diagnostics.Process.Start(info);
                IsChecked = true;
                QuickScanVM.LastFileComparison = this; // yea I know - too direct.  Get over it.
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message + "\r\n" + command + "\r\n" + arguments, "Unable to launch");
            }

        }
        public FileComparison(FolderComparison parent, string fileName)
            : this()
        {
            Name = fileName;
            _parent = parent;
            updateStatus();
        }
        private void updateStatus()
        {
            Paths sources = _parent.CurrentPaths;
            FileInfo left = new FileInfo(Path.Combine(sources.Left, Name));
            FileInfo right = new FileInfo(Path.Combine(sources.Right, Name));
            _pathInfo = (left.Exists ? left.FullName : "-") + "\r\n" +
                (right.Exists ? right.FullName : "-");
            if (IsMissing(left))
                FileStatus = FileStatus.RightOnly;
            else if (IsMissing(right))
                FileStatus = FileStatus.LeftOnly;
            else if (AreSame(left, right))
                FileStatus = FileStatus.Matched;
            else FileStatus = FileStatus.Different;
            raisePropertyChanged("FileStatus", "PathInfo");
            _parent.SetFlags();

        }
        private bool IsMissing(FileInfo fileInfo)
        {
            return !(fileInfo.Exists);
        }
        private bool AreSame(FileInfo left, FileInfo right)
        {
            if (left.Length != right.Length) return false;
            // this is the quick version!
            // we really may want deeper testing here to avoid false true!
            return true;
        }
        #region    	  INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        // This method is called by the Set accessor of each property. 
        private void raisePropertyChanged(params String[] propertyNames)
        {
            if (PropertyChanged != null)
            {
                foreach (string propertyName in propertyNames)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion // INotifyPropertyChanged
        internal bool CanLaunch()
        {
            return (FileStatus != QuickScan.FileStatus.Matched);
        }
        internal bool CanCheckOutLeft()
        {
            return (FileStatus != QuickScan.FileStatus.RightOnly
                && FileStatus != QuickScan.FileStatus.Empty
                && FileStatus != QuickScan.FileStatus.Unknown);
        }
        internal bool CanCheckOutRight()
        {
            return (FileStatus != QuickScan.FileStatus.LeftOnly
                && FileStatus != QuickScan.FileStatus.Empty
                && FileStatus != QuickScan.FileStatus.Unknown);
        }
        internal void Copy()
        {
            try
            {

                string left = Path.Combine(_parent.CurrentPaths.Left, Name);
                string right = Path.Combine(_parent.CurrentPaths.Right, Name);
                if (_status == QuickScan.FileStatus.LeftOnly)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(right));
                    File.Copy(left, right, false);
                    updateStatus();
                }
                else if (_status == QuickScan.FileStatus.RightOnly)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(left));
                    File.Copy(right, left, false);
                    updateStatus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        internal void PushLeft()
        {
            try
            {

                string left = Path.Combine(_parent.CurrentPaths.Left, Name);
                string right = Path.Combine(_parent.CurrentPaths.Right, Name);
                Directory.CreateDirectory(Path.GetDirectoryName(left));
                File.Copy(right, left, true);
                updateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        internal void PushRight()
        {
            try
            {
                string left = Path.Combine(_parent.CurrentPaths.Left, Name);
                string right = Path.Combine(_parent.CurrentPaths.Right, Name);
                Directory.CreateDirectory(Path.GetDirectoryName(left));
                File.Copy(left, right, true);
                updateStatus();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public void CheckOutLeft()
        {
            try
            {
                string appPath = _tfsPath_;
                string filePath = getLeftPath();
                string target = "checkout \"" + filePath + "\"";
                System.Diagnostics.Process.Start(appPath, target);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sorry, TFS Checkout failed.");
            }
        }

        public void CheckOutRight()
        {
            try
            {
                string appPath = _tfsPath_;
                string filePath = getRightPath();
                string target = "checkout \"" + filePath + "\"";
                System.Diagnostics.Process.Start(appPath, target);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sorry, TFS Checkout failed.");
            }
        }
        internal string getLeftPath()
        {
            return _pathInfo.Split("\r\n".ToCharArray())[0];
        }
        internal string getRightPath()
        {
            var x = _pathInfo.Split("\r\n".ToCharArray());
            return x[x.Length - 1];
        }
        public void ViewInFolderLeft()
        {
            try
            {

                string filePath = "/e,/select," + getLeftPath();
                System.Diagnostics.Process.Start("Explorer.exe", filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sorry, Explorer failed");
            }
        }
        public void ViewInFolderRight()
        {
            try
            {

                string filePath = "/e,/select," + getRightPath();
                System.Diagnostics.Process.Start("Explorer", filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sorry, Explorer failed");
            }
        }
        public void HistoryLeft()
        {
            try
            {
                string appPath = _tfsPath_;
                string filePath = getLeftPath();
                string target = "history \"" + filePath + "\" /recursive" + QuickScanVM.GetFilters();
                System.Diagnostics.Process.Start(appPath, target);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sorry, TFS History failed.");
            }
        }
        public void HistoryRight()
        {
            try
            {
                string appPath = _tfsPath_;
                string filePath = getRightPath();
                string target = "history \"" + filePath + "\" /recursive" + QuickScanVM.GetFilters();
                System.Diagnostics.Process.Start(appPath, target);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sorry, TFS History failed.");
            }
        }
        internal bool CanCopy()
        {
            return (FileStatus == QuickScan.FileStatus.LeftOnly || FileStatus == QuickScan.FileStatus.RightOnly);
        }
    }
}
