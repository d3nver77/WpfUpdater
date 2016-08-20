using System;

namespace UpdateCreator.Models
{
    public enum ProgressStatus
    {
        Started,
        Running,
        Completed,
        Canceled,
        Error
    }
    public class ProgressEventArgs : EventArgs
    {
        public string FileName { get; }
        public int Percent { get; }
        public ProgressStatus Status { get; }
        public string Message { get; }

        public ProgressEventArgs(string filename, int percent, ProgressStatus status = ProgressStatus.Running) : this(filename, percent, status, string.Empty)
        {
        }

        public ProgressEventArgs(string filename, int percent, ProgressStatus status, string message)
        {
            this.FileName = filename;
            this.Percent = percent;
            this.Status = status;
            this.Message = message;
        }
    }
}