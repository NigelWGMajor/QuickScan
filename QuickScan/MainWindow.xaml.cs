using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using QuickScan.Properties;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
// for Routed Command
// ReSharper disable InconsistentNaming

namespace QuickScan
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static bool ShowBusy
        {
            set
            {
                if (value) _window.Cursor = Cursors.Wait;
                else _window.Cursor = Cursors.Arrow;
            }
        }
        private bool _isExpanded;
        private static Control _window;
        private string _leftFolderPath
        {
            get
            { return (string)Settings.Default["LeftFolderPath"]; }
            set
            {
                Settings.Default["LeftFolderPath"] = value;
                _vm.LeftPath = value;
            }
        }
        private string _rightFolderPath
        {
            get { return (string)Settings.Default["RightFolderPath"]; }
            set
            {
                Settings.Default["RightFolderPath"] = value;
                _vm.RightPath = value;
            }
        }
        public const string _MERGE_COMMAND_LINE_ = "C:\\Users\\nickm\\AppData\\Local\\Programs\\Microsoft VS Code Insiders\\Code - Insiders.exe";
        public const string _MERGE_COMMAND_ARGS_ = "--diff ";
       // public const string _MERGE_COMMAND_LINE_ = "C:\\Program Files (x86)\\WinMerge\\WinMergeU.exe";

        internal QuickScanVM _vm;
        //private Paths paths;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm = new QuickScanVM();
            string[] args = Environment.GetCommandLineArgs();
            _window = this;
            if (args.Length > 1)
            {
                _vm.LeftPath = args[1].Replace("\"", "").Trim();

                if (args.Length > 2)
                    _vm.RightPath = args[2].Replace("\"", "").Trim();

            }
            else
            {
                _vm.LeftPath = (string)Settings.Default["LeftFolderPath"];
                _vm.RightPath = (string)Settings.Default["RightFolderPath"];
            }
        }
        private void Path_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true; // this is actually a preview, because TextBoxes need special handling for dragover.
        }
        private void eRightPath_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                _vm.Paths.Right = System.IO.Path.GetDirectoryName(droppedPaths[0]);
                if (droppedPaths.Length > 1) _vm.Paths.Right = droppedPaths[1];
                if (_vm.Paths.IsComplete) tryComparePaths();
            }
        }
        private void eLeftPath_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                _vm.Paths.Left = System.IO.Path.GetDirectoryName(droppedPaths[0]);
                if (droppedPaths.Length > 1) _vm.Paths.Right = droppedPaths[1];
                if (_vm.Paths.IsComplete) tryComparePaths();
            }
        }
        private void tryComparePaths()
        {
            DataContext = _vm = new QuickScanVM(_vm.Paths);
        }
        private void CanAlwaysExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            if (QuickScanVM.LastFileComparison != null && Clipboard.ContainsText())
            {
                QuickScanVM.LastFileComparison.Comment = Clipboard.GetText();
                QuickScanVM.LastFileComparison = null;
            }
        }
        private void CheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            _vm.ClearAllowedExtensions();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.Save();
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in ExtensionsList.SelectedItems)
            {
                var extension = item as Extension;
                if (extension != null) extension.IsActive = true;
            }
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in ExtensionsList.SelectedItems)
            {
                var extension = item as Extension;
                if (extension != null) extension.IsActive = false;
            }
        }
        private void cCheckDifferent(object sender, RoutedEventArgs e)
        {
            _vm.CheckDifferent();
        }

        private void cCheckout(object sender, RoutedEventArgs e)
        {
            _vm.CheckoutLeft();
        }
        private void cClip(object sender, RoutedEventArgs e)
        {   // Capture to clipboard
            _vm.ClipData();
        }
        private void cLeft(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog { ShowNewFolderButton = false };
            if (!String.IsNullOrEmpty(_vm.LeftPath)) d.SelectedPath = _vm.LeftPath;
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                _leftFolderPath = _vm.LeftPath = d.SelectedPath;
        }
        private void cRefresh(object sender, RoutedEventArgs e)
        {
            _vm.Refresh();
        }
        private void cRight(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog { ShowNewFolderButton = false };
            if (!String.IsNullOrEmpty(_vm.RightPath)) d.SelectedPath = _vm.RightPath;
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                _rightFolderPath = _vm.RightPath = d.SelectedPath;
        }
        private void cToggleExpand(object sender, RoutedEventArgs e)
        {   // toggle the expanded status of all tree elements
            _isExpanded = !_isExpanded;
            _vm.ExpandAll(_isExpanded);
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
