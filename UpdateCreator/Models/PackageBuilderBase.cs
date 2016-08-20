using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace UpdateCreator.Models
{
    public abstract class PackageBuilderBase
    {
        protected Package Package { get; }
        protected IEnumerable<string> FileList { get; } = FileProvider.Default.GetFileList().Where(f => f.IsSelected).Select(f => f.Filename);

        protected PackageBuilderBase(Package package)
        {
            this.Package = package;
        }

        public static EventHandler<ProgressEventArgs> PackProgressChanged = (sender, args) => { };
        protected static void OnPackProgressChanged(object sender, ProgressEventArgs args)
        {
            PackProgressChanged(sender, args);
        }

        public static EventHandler<ProgressEventArgs> PackCompleted = (sender, args) => { };
        protected static void OnPackCompleted(object sender, ProgressEventArgs args)
        {
            PackCompleted(sender, args);
        }

        public void Create()
        {
            if (this.Package == null)
            {
                throw new Exception("Package can't be null!");
            }
            this.OnCreatePackage();
        }

        public void Cancel()
        {
            this.OnCancel();
        }

        private void OnCreatePackage()
        {


            try
            {
                this.RemovePackageFiles();
                this.Package.Hash = string.Empty;
                this.CreatePackageFileZip();
                this.Package.Hash = this.GetPackageZipHash();
                this.CreatePackageFileXml();
            }
            catch (Exception)
            {
                this.RemovePackageFiles();
                throw;
            }
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

        protected abstract void CreatePackageFileZip();
        protected abstract void OnCancel();

        protected void OnCanceled()
        {
            this.RemovePackageFiles();
            this.Package.Hash = string.Empty;
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