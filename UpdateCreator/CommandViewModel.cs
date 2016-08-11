using System;
using System.Windows.Input;

namespace UpdateCreator
{
    public class CommandViewModel : ViewModelBase
    {
        private string _displayName = string.Empty;
        public string DisplayName
        {
            get { return this._displayName; }
            set
            {
                if (this._displayName != value)
                {
                    this._displayName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private ICommand _command;
        public ICommand Command
        {
            //if (this._command == null)
            //    throw new ArgumentNullException("command");
            get { return this._command; }
            set
            {
                if (this._command != value)
                {
                    this._command = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _hitText;
        public string HitText
        {
            get { return string.IsNullOrEmpty(this._hitText) ? this.DisplayName : this._hitText; }
            set
            {
                if (this._hitText != value)
                {
                    this._hitText = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public CommandViewModel()
        {

        }

        public CommandViewModel(string displayName, ICommand command)
        {
            this.DisplayName = displayName ?? string.Empty;
            this.Command = command;
        }

        public CommandViewModel(string displayName, ICommand command, string hitText) : this(displayName, command)
        {
            this.HitText = hitText;
        }
    }
}