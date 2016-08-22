using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace UpdateCreator.Models
{
    public class PackageBuilder
    {
        protected readonly CancellationTokenSource CancellationTokenSource;
        protected readonly IProgress<ProgressEventArgs> Progress;
        protected int Percentage;
        protected string CurrentFileName = string.Empty;
        protected static readonly ManualResetEvent ManualResetEvent = new ManualResetEvent(true);


        protected Package Package { get; }
        protected IEnumerable<string> FileList { get; } = FileProvider.Default.GetFileList().Where(f => f.IsSelected).Select(f => f.Filename);

        public PackageBuilder(Package package)
        {
            this.Package = package;
            this.CancellationTokenSource = new CancellationTokenSource();
            this.Progress = new Progress<ProgressEventArgs>();
            ((Progress<ProgressEventArgs>) this.Progress).ProgressChanged += PackProgressChanged;
        }

        public static EventHandler<ProgressEventArgs> PackProgressChanged = (sender, args) => { };

        public static EventHandler<ProgressEventArgs> PackCompleted = (sender, args) => { };

        public void Create()
        {
            this.Percentage = 0;
            this.RemovePackageFiles();
            this.OnCreatePackage();
        }

        public void Cancel()
        {
            this.CancellationTokenSource?.Cancel();
        }

        public bool IsPause
        {
            get { return !ManualResetEvent.WaitOne(0); }
            set
            {
                if (value)
                {
                    ManualResetEvent.Reset();
                }
                else
                {
                    ManualResetEvent.Set();
                }
            }
        }
        
        private async void OnCreatePackage()
        {
            try
            {
                await Task.Run(() =>
                {
                    this.CreatePackageFileZip();
                });
            }
            catch (OperationCanceledException ex)
            {
                this.RemovePackageFiles();
                PackCompleted(this, new ProgressEventArgs(this.CurrentFileName, this.Percentage, ProgressStatus.Canceled, ex.Message));
                return;
            }
            catch (Exception ex)
            {
                this.RemovePackageFiles();
                PackCompleted(this, new ProgressEventArgs(this.CurrentFileName, this.Percentage, ProgressStatus.Error, ex.Message));
                return;
            }
            this.CurrentFileName = string.Empty;
            this.Package.Hash = this.GetPackageZipHash();
            this.CreatePackageFileXml();
            PackCompleted(this, new ProgressEventArgs(this.CurrentFileName, this.Percentage, ProgressStatus.Completed));
        }

        private void RemovePackageFiles()
        {
            if (File.Exists(this.Package.PackageFilenameZip))
            {
                File.Delete(this.Package.PackageFilenameZip);
            }
            if (File.Exists(this.Package.PackageFilenameXml))
            {
                File.Delete(this.Package.PackageFilenameXml);
            }
        }

        protected virtual void CreatePackageFileZip()
        {
            var count = 0;
            this.Progress.Report(new ProgressEventArgs(this.CurrentFileName, this.Percentage, ProgressStatus.Started));
            using (var archive = ZipFile.Open(this.Package.PackageFilenameZip, ZipArchiveMode.Create))
            {
                foreach (var fileName in this.FileList)
                {
                    this.CurrentFileName = fileName;
                    ManualResetEvent.WaitOne();
                    this.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    archive.CreateEntryFromFile(this.CurrentFileName, this.CurrentFileName, CompressionLevel.Optimal);
                    this.Percentage = (int)Math.Round((double)++count / this.FileList.Count() * 100.0, 0);
                    this.Progress.Report(new ProgressEventArgs(this.CurrentFileName, this.Percentage));
                    Thread.Sleep(200);
                }
            }
        }

        private void CreatePackageFileXml()
        {
            string packageXml;

            var serializer = new XmlSerializer(typeof(Package));

            var settings = new XmlWriterSettings
            {
                Encoding = new UnicodeEncoding(false, false),
                Indent = true,
                OmitXmlDeclaration = false
            };

            using (var textWriter = new Utf8StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    var namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    serializer.Serialize(xmlWriter, this.Package, namespaces);
                }
                packageXml = textWriter.ToString();
            }

            File.WriteAllText(this.Package.PackageFilenameXml, packageXml);
        }

        private string GetPackageZipHash()
        {
            var sb = new StringBuilder();
            if (!File.Exists(this.Package.PackageFilenameZip))
            {
                return sb.ToString();
            }
            using (var stream = new FileStream(this.Package.PackageFilenameZip, FileMode.Open))
            {
                var hash = MD5.Create().ComputeHash(stream);
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2").ToLower());
                }
            }
            return sb.ToString();
        }

        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => new UTF8Encoding();
        }
    }
}