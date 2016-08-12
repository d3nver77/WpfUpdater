using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace UpdateCreator.Models
{
    public class Generator
    {
        private static readonly string[] ExludeMaskArray = new[] {".pdb", ".vshost.", ".config"};

        public List<string> ExludeMaskList { get; private set; }

        private List<string> _fileList;

        public List<string> IncludeFilelist { get; set; }
        private string CurrentDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); }
        }
        private string EntryAssemblyName
        {
            get { return Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location); }
        }

        private const string UpdateFilenameZip = @"update.zip";
        private const string UpdateFilenameXml = @"update.xml";

        public Generator()
        {
            this.ExludeMaskList = new List<string>();
            this.ExludeMaskList.AddRange(ExludeMaskArray);
            this.IncludeFilelist = new List<string>();
            this.IncludeFilelist.Add("SomeAppWithUpdate.exe.config");
        }

        public void GetFileList()
        {
            this._fileList = Directory
                .GetFiles(this.CurrentDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Where(name => !this.ExludeMaskList.Any(mask => name.IndexOf(mask, StringComparison.InvariantCultureIgnoreCase) >=0))
                .Select(f=>f.Replace(this.CurrentDirectory, string.Empty)
                .TrimStart('\\'))
                .Where(name => !string.Equals(name, UpdateFilenameZip, StringComparison.InvariantCultureIgnoreCase))
                .Where(name => !string.Equals(name, UpdateFilenameXml, StringComparison.InvariantCultureIgnoreCase))
                .Where(name => name.IndexOf(this.EntryAssemblyName, StringComparison.InvariantCultureIgnoreCase) < 0)
                .ToList();
            this._fileList.AddRange(this.IncludeFilelist);
        }

        public void CreateUpdateZip()
        {
            if (File.Exists(UpdateFilenameZip))
            {
                File.Delete(UpdateFilenameZip);
            }

            using (ZipArchive archive = ZipFile.Open(UpdateFilenameZip, ZipArchiveMode.Create))
            {
                foreach (var fileName in this._fileList)
                {
                    archive.CreateEntryFromFile(fileName, fileName, CompressionLevel.Optimal);
                }
            }

            
        }

        public void CreateUpdateXml()
        {
            var updateXml = new UpdateXml()
            {
                ApplicationName = "SomeAppWithUpdate",
                Url = "www.test.com/" + UpdateFilenameZip,
                Hash = this.GetHash(),
                Description = "New version.", 
                Filename = "SomeAppWithUpdate.exe",
                LaunchArguments = string.Empty
            };
            updateXml.Version = FileVersionInfo.GetVersionInfo(updateXml.Filename).ProductVersion;
        }

        private string GetHash()
        {
            var sb = new StringBuilder();
            using (var stream = new FileStream(UpdateFilenameZip, FileMode.Open))
            {
                var hash = MD5.Create().ComputeHash(stream);
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2").ToLower());
                }
            }
            return sb.ToString();
        }
    }
}