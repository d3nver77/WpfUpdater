using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UpdateCreator.Models
{
    public class CheckedFile : INotifyPropertyChanged, IDragable
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get { return this._isSelected; }
            set
            {
                if (this._isSelected == value)
                {
                    return;
                }
                this._isSelected = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsDragable => FileProvider.Default.IsDragable(this);

        public string Filename { get; set; }
        
        public CheckedFile(string filename)
        {
            this.Filename = filename;
            this.IsSelected = false;
        }

        public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}