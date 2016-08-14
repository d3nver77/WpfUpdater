using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace UpdateCreator.Models
{
    public class PackageBuilder
    {
        private readonly Package _package;

        public PackageBuilder(Package package)
        {
            this._package = package;
        }

        private IEnumerable<string> FileList { get; } = FileProvider.Default.GetFileList().Where(f => f.IsSelected).Select(f => f.Filename);

        public void Create()
        {
            if (this._package == null)
            {
                throw new Exception("Package can't be null!");
            }
            this.CreatePackage();
        }

        private void CreatePackage()
        {
            try
            {
                this.RemovePackageFiles();
                this.CreatePackageFileZip();
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
            if (File.Exists(this._package.PackageFilenameZip))
            {
                File.Delete(this._package.PackageFilenameZip);
            }
            this._package.Hash = string.Empty;
            if (File.Exists(this._package.PackageFilenameXml))
            {
                File.Delete(this._package.PackageFilenameXml);
            }
        }

        private void CreatePackageFileZip()
        {
            using (var archive = ZipFile.Open(this._package.PackageFilenameZip, ZipArchiveMode.Create))
            {
                foreach (var fileName in this.FileList)
                {
                    archive.CreateEntryFromFile(fileName, fileName, CompressionLevel.Optimal);
                }
            }
            this._package.Hash = this.GetHashPackageZip();
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
                    serializer.Serialize(xmlWriter, this._package, namespaces);
                }
                packageXml = textWriter.ToString();
            }

            File.WriteAllText(this._package.PackageFilenameXml, packageXml);
        }

        private string GetHashPackageZip()
        {
            var sb = new StringBuilder();
            if (!File.Exists(this._package.PackageFilenameZip))
            {
                return sb.ToString();
            }
            using (var stream = new FileStream(this._package.PackageFilenameZip, FileMode.Open))
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