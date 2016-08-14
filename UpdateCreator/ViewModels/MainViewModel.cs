using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using UpdateCreator.Models;
using UpdateCreator.ViewModels.Commands;

namespace UpdateCreator.ViewModels
{
    public class MainViewModel : ViewModelBase, IFileProperties
    {
        private readonly Package _package = new Package();

        public MainViewModel()
        {
            FileProvider.Default.FilelistChangedHandler += (sender, args) =>
            {
                this.OnPropertyChanged(() => this.FileList);
            };
            FileProvider.Default.SelectedFileChangedHandler += (sender, args) =>
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
                this.Version = versionInfo.ProductVersion?? string.Empty;
            };
            this._package.PackageNameChanged += (sender, args) =>
            {
                this.OnPropertyChanged(() => this.PackageFilename);
            };
            this._package.PackageNameChanged += (sender, args) =>
            {
                FileProvider.Default.PackageChanged(args.Package);
            };

            this._package.PackageName = "update";
        }

        #region Properties

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
            get { return this._package.Url; }
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

        private CommandViewModel _createUpdatePackageCommand;
        public CommandViewModel CreateUpdatePackageCommand
        {
            get { return this._createUpdatePackageCommand 
                    ?? (this._createUpdatePackageCommand = new CommandViewModel("Create update package", new RelayCommand(this.CreateUpdatePackage, p => this.IsPackageValid))); }
        }

        private CommandViewModel _uploadOnServerCommand;
        public CommandViewModel UploadOnServerCommand
        {
            get
            {
                return this._uploadOnServerCommand
                  ?? (this._uploadOnServerCommand = new CommandViewModel("Upload on server", new RelayCommand(this.UploadOnServer,
                      p => !string.IsNullOrEmpty(this.UploadPath))));
            }
        }

        private CommandViewModel _closeCommand;
        public CommandViewModel CloseCommand
        {
            get
            {
                return this._closeCommand
                  ?? (this._closeCommand = new CommandViewModel("Close", new RelayCommand(this.Close)));
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

        private void CreateUpdatePackage(object parameter)
        {
            var packageBuilder = new PackageBuilder(this._package);
            try
            {
                packageBuilder.Create();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error creating package update.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            this.OnPropertyChanged(()=> this.FileList);
        }

        private void UploadOnServer(object obj)
        {
            throw new NotImplementedException();
        }

        private void Close(object obj)
        {
            Application.Current.Shutdown();
        }

        private void UpdateFileList(object obj)
        {
            this.OnPropertyChanged(()=> this.FileList);
        }
    }
}