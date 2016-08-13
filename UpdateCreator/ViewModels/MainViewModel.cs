using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using UpdateCreator.Models;
using UpdateCreator.ViewModels.Commands;

namespace UpdateCreator.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            this.PackageName = "update";
        }

        #region Properties

        public string ExludeMasks
        {
            get
            {
                return FileProvider.Instance.Filter;
            }
            set
            {
                FileProvider.Instance.Filter = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(()=> this.FileList);
            }
        }

        public List<CheckedFile> FileList
        {
            get
            {
                return FileProvider.Instance.GetFilterFileList();
            }
        }

        private string _packageName;
        public string PackageName
        {
            get { return this._packageName; }
            set
            {
                if (this._packageName != value)
                {
                    this._packageName = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(() => this.PackageFilename);
                }
            }
        }

        public string ApplicationName { get; set; }
        public string Description { get; set; }
        public string Filename { get; set; }
        public string LaunchArguments { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }

        public string PackageFilename
        {
            get
            {
                return string.IsNullOrEmpty(this.PackageName) ? string.Empty : string.Format("{0}.zip", this.PackageName);
            }
        }

        public string UploadPath { get; set; }

        public bool IsPackageValid
        {
            get
            {
                return !string.IsNullOrEmpty(this.ApplicationName) &&
                       !string.IsNullOrEmpty(this.PackageName) &&
                       !string.IsNullOrEmpty(this.Url);
            }
        }

        #endregion Properties

        #region Commands

        private CommandViewModel _createUpdatePackageCommand = null;
        public CommandViewModel CreateUpdatePackageCommand
        {
            get { return this._createUpdatePackageCommand 
                    ?? (this._createUpdatePackageCommand = new CommandViewModel("Create update package", new RelayCommand(this.CreateUpdatePackage, p => this.IsPackageValid))); }
        }

        private CommandViewModel _uploadOnServerCommand = null;
        public CommandViewModel UploadOnServerCommand
        {
            get
            {
                return this._uploadOnServerCommand
                  ?? (this._uploadOnServerCommand = new CommandViewModel("Upload on server", new RelayCommand(this.UploadOnServer,
                      p => !string.IsNullOrEmpty(this.UploadPath))));
            }
        }

        private CommandViewModel _closeCommand = null;
        public CommandViewModel CloseCommand
        {
            get
            {
                return this._closeCommand
                  ?? (this._closeCommand = new CommandViewModel("Close", new RelayCommand(this.Close)));
            }
        }

        private CommandViewModel _updateFilelistCommand = null;
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
            throw new System.NotImplementedException();
        }

        private void UploadOnServer(object obj)
        {
            throw new System.NotImplementedException();
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