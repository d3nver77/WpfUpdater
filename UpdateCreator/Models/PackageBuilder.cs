using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace UpdateCreator.Models
{
    public class PackageBuilder
    {
        private readonly Package _package;

        private List<string> _fileList
        {
            get
            {
                return FileProvider.Default.GetFilterFileList()
                    .Where(f => f.IsSelected)
                    .Select(f => f.Filename)
                    .ToList();
            }
        }

        public PackageBuilder(Package package)
        {
            this._package = package;
        }

        public bool Create()
        {
            if (this._package == null)
            {
                throw new System.Exception("Package can't be null!");
            }
            
            return this.CreatePackage();
        }

        private bool CreatePackage()
        {
            this.RemovePackage();

            try
            {
                this.CreatePackageZip();
            }
            catch (Exception)
            {
                this.RemovePackage();
                return false;
            }
            return true;
        }

        private void RemovePackage()
        {
            if (File.Exists(this._package.PackageFilenameZip))
            {
                File.Delete(this._package.PackageFilenameZip);
            }
            if (File.Exists(this._package.PackageFilenameXml))
            {
                File.Delete(this._package.PackageFilenameXml);
            }
        }

        void CreatePackageZip()
        {
            using (ZipArchive archive = ZipFile.Open(this._package.PackageFilenameZip, ZipArchiveMode.Create))
            {
                foreach (var fileName in this._fileList)
                {
                    archive.CreateEntryFromFile(fileName, fileName, CompressionLevel.Optimal);
                }
            }
        }
    }
}