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
            var filelist = FileProvider.Instance.GetFilterFileList();
            this.FileList = new ObservableCollection<CheckedFile>(filelist);


            //FileList.Add(new CheckedFile("file1.txt"));
            //FileList.Add(new CheckedFile("file2.txt"));
            //FileList.Add(new CheckedFile("file3.txt", false) );
            //FileList.Add(new CheckedFile("file4.txt"));
            //FileList.Add(new CheckedFile("file5.txt", false));
        }

        #region Properties

        public string ExludeMasks
        {
            get
            {
                var mask = string.Join("; ", FileProvider.Instance.ExcludeMaskList);
                return mask;
            }
            set
            {
                var mask = value ?? string.Empty;
                var maskSplitted = Regex.Split(mask, @"\s*;\s*");
                FileProvider.Instance.ExcludeMaskList.Clear();
                FileProvider.Instance.ExcludeMaskList.AddRange(maskSplitted);
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<CheckedFile> FileList { get; }

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
                  ?? (this._uploadOnServerCommand = new CommandViewModel("Upload on server", new RelayCommand(this.UploadOnServer)));
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
            ExludeMasks += "; *.exe";
        }
    }
}