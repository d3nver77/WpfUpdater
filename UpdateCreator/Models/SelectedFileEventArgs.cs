using System;

namespace UpdateCreator.Models
{
    public class SelectedFileEventArgs : EventArgs
    {
        public CheckedFile File { get; set; }

        public SelectedFileEventArgs(CheckedFile file)
        {
            this.File = file;
        }
    }
}