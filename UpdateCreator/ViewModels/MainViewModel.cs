using System;
using System.Collections.Generic;
using System.Windows;
using UpdateCreator.Models;
using UpdateCreator.ViewModels.Commands;

namespace UpdateCreator.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Package _package = new Package();

        public MainViewModel()
        {
            FileProvider.Default.FilelistChangedHandler += (sender, args) =>
            {
                this.OnPropertyChanged(() => this.FileList);
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
                }
            }
        }

        public string ApplicationName
        {
            get { return this._package.ApplicationName; }
            set
            {
                if (this._package.ApplicationName != value)
                    {
                        this._package.ApplicationName = value;
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
                }
            }
        }

        public string Filename
        {
            get { return this._package.Filename; }
            set
            {
                if (this._package.Filename != value)
                {
                    this._package.Filename = value;
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
                }
            }
        }

        public string Version => this._package.Version;

        public string Url
        {
            get { return this._package.Url; }
            set
            {
                if (this._package.Url != value)
                {
                    this._package.Url = value;
                }
            }
        }

        public string PackageFilename => this._package.PackageFilenameZip;

        public string UploadPath { get; set; }

        private bool IsPackageValid => !string.IsNullOrEmpty(this.ApplicationName)
            && !string.IsNullOrEmpty(this.PackageName)
            && !string.IsNullOrEmpty(this.Url);

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