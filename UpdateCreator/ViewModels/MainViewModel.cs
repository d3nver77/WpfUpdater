using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using UpdateCreator.Models;
using UpdateCreator.ViewModels.Commands;

namespace UpdateCreator.ViewModels
{
    public class MainViewModel : ViewModelBase, IFileProperties
    {
        #region Fields
        private readonly Package _package = new Package();
        private string _progressFileName;
        private bool _isEnabled;
        private PackageBuilder _packageBuilder;
        #endregion Fields

        public MainViewModel()
        {
            this._createUpdatePackageCommand = new CommandViewModel("Create update package", new RelayCommand(this.CreateUpdatePackage, p => this.IsPackageValid));
            this._abortUpdatePackageCommand = new CommandViewModel("Cancel...", new RelayCommand(this.AbortUpdatePackage));
            this.UpdatePackageCommand = this._createUpdatePackageCommand;

            FileProvider.Default.FilelistChangedHandler += (sender, args) =>
            {
                this.OnPropertyChanged(() => this.FileList);
            };
            FileProvider.Default.SelectedFileChangedHandler += this.OnSelectedFileChangedHandler;

            this._package.PackageNameChanged += (sender, args) =>
            {
                this.OnPropertyChanged(() => this.PackageFilename);
                FileProvider.Default.PackageChanged(args.Package);
            };

            PackageBuilder.PackProgressChanged += this.OnPackProgressChanged;

            PackageBuilder.PackCompleted += this.OnPackCompleted;
            this.IsEnabled = true;
            this._package.PackageName = "update";
        }

        private void OnSelectedFileChangedHandler(object sender, SelectedFileEventArgs args)
        {
            var file = args.File;
            if (file == null)
            {
                this.Filename = string.Empty;
                this.LaunchArguments = string.Empty;
                this.Version = string.Empty;
                return;
            }
            this.Filename = file.Filename;
            this.ProductName = Path.GetFileNameWithoutExtension(file.Filename);
            var versionInfo = FileVersionInfo.GetVersionInfo(file.Filename);
            this.Version = versionInfo.ProductVersion ?? string.Empty;
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnPackProgressChanged(object sender, ProgressEventArgs args)
        {
            this.ProgressValue = args.Percent;
            this.ProgressFileName = args.FileName;
        }

        private void OnPackCompleted(object sender, ProgressEventArgs args)
        {
            if (args.Status == ProgressStatus.Completed)
            {
                this.ProgressFileName = args.FileName;
                MessageBox.Show("Update created!", "Update complete");
                this.ProgressValue = 0;
            }
            if (args.Status == ProgressStatus.Error)
            {
                MessageBox.Show(args.Message, "Error creating package update", MessageBoxButton.OK, MessageBoxImage.Error);
                this.ProgressFileName = args.FileName;
                this.ProgressValue = 0;
            }
            if (args.Status == ProgressStatus.Canceled)
            {
                MessageBox.Show("Package update is canceled.", "Update canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                this.ProgressValue = 0;
                this.ProgressFileName = string.Empty;
            }
            this.IsEnabled = true;
            this.UpdatePackageCommand = this._createUpdatePackageCommand;
            FileProvider.Default.Update();
        }

        #region Properties

        public bool IsEnabled
        {
            get { return this._isEnabled; }
            set
            {
                if (this._isEnabled != value)
                {
                    this._isEnabled = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string ExludeMasks
        {
            get { return FileProvider.Default.Filter; }
            set { FileProvider.Default.Filter = value; }
        }

        public List<CheckedFile> FileList => FileProvider.Default.GetFileList();

        public string PackageName
        {
            get { return this._package.PackageName; }
            set
            {
                if (this._package.PackageName != value)
                {
                    this._package.PackageName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string ProgressFileName
        {
            get { return this._progressFileName; }
            set
            {
                if (this._progressFileName != value)
                {
                    this._progressFileName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get { return this._progressValue; }
            set
            {
                if (Math.Abs(this._progressValue - value) > double.Epsilon)
                {
                    this._progressValue = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(() => this.ProgressPercentValue);
                }
            }
        }

        public string ProgressPercentValue => $"{(Math.Abs(this.ProgressValue) < double.Epsilon ? string.Empty : this.ProgressValue + "%")}";

        public string ProductName
        {
            get { return this._package.ProductName; }
            set
            {
                if (this._package.ProductName != value)
                {
                    this._package.ProductName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Description
        {
            get { return this._package.Description; }
            set
            {
                if (this._package.Description != value)
                {
                    this._package.Description = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Filename
        {
            get { return this._package.LaunchFile; }
            set
            {
                if (this._package.LaunchFile != value)
                {
                    this._package.LaunchFile = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string LaunchArguments
        {
            get { return this._package.LaunchArguments; }
            set
            {
                if (this._package.LaunchArguments != value)
                {
                    this._package.LaunchArguments = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Version
        {
            get { return this._package.Version; }
            set
            {
                if (this._package.Version != value)
                {
                    this._package.Version = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string Url
        {
            get { return string.IsNullOrEmpty(this._package.Url) ? this._package.Url : Path.GetDirectoryName(this._package.Url); }
            set
            {
                if (this._package.Url != value)
                {
                    this._package.Url = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string PackageFilename => this._package.PackageFilenameZip;

        public string UploadPath { get; set; }

        private bool IsPackageValid => !string.IsNullOrEmpty(this.ProductName)
            && !string.IsNullOrEmpty(this.PackageName)
            && !string.IsNullOrEmpty(this.Url);

        public IDragable SelectedFile
        {
            //get { throw new NotImplementedException(); }
            set
            {
                var file = (CheckedFile) value;
                file.IsSelected = true;
                FileProvider.Default.SelectedFile = file;
            }
        }

        #endregion Properties

        #region Commands

        private readonly CommandViewModel _createUpdatePackageCommand;
        private readonly CommandViewModel _abortUpdatePackageCommand;
        private CommandViewModel _updatePackageCommand;
        public CommandViewModel UpdatePackageCommand
        {
            get { return this._updatePackageCommand; }
            set
            {
                if (this._updatePackageCommand != value)
                {
                    this._updatePackageCommand = value;
                    this.OnPropertyChanged();
                }
            }
        }
        
        private CommandViewModel _uploadOnServerCommand;
        public CommandViewModel UploadOnServerCommand
        {
            get
            {
                return this._uploadOnServerCommand
                  ?? (this._uploadOnServerCommand = new CommandViewModel("Upload on server", new RelayCommand(this.UploadOnServer,
                      p => !string.IsNullOrEmpty(this.UploadPath) && this.IsEnabled)));
            }
        }

        private CommandViewModel _closeCommand;
        public CommandViewModel CloseCommand
        {
            get
            {
                return this._closeCommand
                  ?? (this._closeCommand = new CommandViewModel("Close", new RelayCommand(this.Close, o => this.IsEnabled)));
            }
        }

        private CommandViewModel _updateFilelistCommand;

        public CommandViewModel UpdateFilelistCommand
        {
            get
            {
                return this._updateFilelistCommand
                  ?? (this._updateFilelistCommand = new CommandViewModel("Refresh list", new RelayCommand(this.UpdateFileList)));
            }
        }

        #endregion Commands

        #region Methods

        private void CreateUpdatePackage(object parameter)
        {
            this.UpdatePackageCommand = this._abortUpdatePackageCommand;
            this.IsEnabled = false;
            this._packageBuilder = new PackageBuilder(this._package);
            this._packageBuilder.Create();
        }

        private void AbortUpdatePackage(object obj)
        {
            this._packageBuilder.IsPause = true;
            var messageBoxResult = MessageBox.Show("Cancel update created?", "Update canceled", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                this._packageBuilder.Cancel();
            }
            this._packageBuilder.IsPause = false;
        }

        private void UploadOnServer(object obj)
        {
            var uploader = new Uploader(this.UploadPath);
            uploader.UploadFiles(this._package.PackageFilenameZip, this._package.PackageFilenameXml);
        }

        private void Close(object obj)
        {
            Application.Current.Shutdown();
        }

        private void UpdateFileList(object obj)
        {
            FileProvider.Default.Update();
        }

        #endregion Methods
    }
}