using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using UpdateCreator.Models;
using UpdateCreator.ViewModels.Commands;

namespace UpdateCreator.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            
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

        public string UploadPath { get; set; }
        #endregion Properties

        #region Commands

        private CommandViewModel _createUpdatePackageCommand = null;
        public CommandViewModel CreateUpdatePackageCommand
        {
            get { return this._createUpdatePackageCommand 
                    ?? (this._createUpdatePackageCommand = new CommandViewModel("Create update package", new RelayCommand(this.CreateUpdatePackage))); }
        }

        private CommandViewModel _uploadOnServerCommand = null;
        public CommandViewModel UploadOnServerCommand
        {
            get
            {
                return this._uploadOnServerCommand
                  ?? (this._uploadOnServerCommand = new CommandViewModel("Upload on server", new RelayCommand(this.UploadOnServer,
                      delegate
                      {
                          return !string.IsNullOrEmpty(this.UploadPath);
                      })));
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
            throw new System.NotImplementedException();
        }

        private void UpdateFileList(object obj)
        {
            this.OnPropertyChanged(()=> this.FileList);
        }
    }
}