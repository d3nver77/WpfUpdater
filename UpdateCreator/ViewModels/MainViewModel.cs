using System.Collections.ObjectModel;
using UpdateCreator.ViewModels.Commands;

namespace UpdateCreator.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            this.FileList = new ObservableCollection<CheckedFile>();
            FileList.Add(new CheckedFile() { Filename = "file1.txt", IsSelected = true});
            FileList.Add(new CheckedFile() { Filename = "file2.txt", IsSelected = true});
            FileList.Add(new CheckedFile() { Filename = "file3.txt", IsSelected = false});
            FileList.Add(new CheckedFile() { Filename = "file4.txt", IsSelected = true});
            FileList.Add(new CheckedFile() { Filename = "file5.txt", IsSelected = false});
        }

        public ObservableCollection<CheckedFile> FileList { get; private set; }


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
    }
}