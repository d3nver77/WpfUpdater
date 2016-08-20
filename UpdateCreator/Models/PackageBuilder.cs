using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using IonicZip = Ionic.Zip;

namespace UpdateCreator.Models
{
    public class PackageBuilder : PackageBuilderBase
    {
        private BackgroundWorker _backgroundWorker;

        public PackageBuilder(Package package) : base(package)
        {
            this._backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            this._backgroundWorker.DoWork += this.PackFileZipDoWork;
            this._backgroundWorker.ProgressChanged += this.PackFileZipProgressChanged;
            this._backgroundWorker.RunWorkerCompleted += this.PackFileZipComplete;
        }

        protected override void CreatePackageFileZip()
        {
            this._backgroundWorker.RunWorkerAsync();
        }

        protected override void OnCancel()
        {
            this._backgroundWorker.CancelAsync();
        }

        private void PackFileZipDoWork(object sender, DoWorkEventArgs args)
        {
            var filesNumber = this.FileList.Count();
            var count = 0;
            this._backgroundWorker.ReportProgress(0, string.Empty);
            using (var archive = ZipFile.Open(this.Package.PackageFilenameZip, ZipArchiveMode.Create))
            {
                foreach (var fileName in this.FileList)
                {
                    if (this._backgroundWorker.CancellationPending)
                    {
                        args.Cancel = true;
                        break;
                    }
                    archive.CreateEntryFromFile(fileName, fileName, CompressionLevel.Optimal);
                    var percent = (int)Math.Round((double)++count / filesNumber * 100.0, 0);
                    this._backgroundWorker.ReportProgress(percent, fileName);
                    Thread.Sleep(500);
                }
            }
        }
        
        private void PackFileZipProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            var filename = args.UserState.ToString();
            if (args.ProgressPercentage == 0 && string.IsNullOrEmpty(filename))
            {
                OnPackProgressChanged(this, new ProgressEventArgs(filename, args.ProgressPercentage, ProgressStatus.Started));
            }
            OnPackProgressChanged(this, new ProgressEventArgs(filename, args.ProgressPercentage, ProgressStatus.Running));
        }

        private void PackFileZipComplete(object sender, RunWorkerCompletedEventArgs args)
        {
            if (args.Cancelled)
            {
                this.OnCanceled();
                OnPackCompleted(this, new ProgressEventArgs(string.Empty, 0, ProgressStatus.Canceled));
                return;
            }
            if (args.Error != null)
            {
                this.OnCanceled();
                OnPackCompleted(this, new ProgressEventArgs(args.Error.Message, 0, ProgressStatus.Error));
                return;
            }
            OnPackCompleted(this, new ProgressEventArgs(string.Empty, 100, ProgressStatus.Completed));
        }
    }
}