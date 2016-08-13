using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using UpdateCreator.ViewModels;

namespace UpdateCreator.Models
{
    public class FileProvider
    {
        #region Instance Default

        private static FileProvider _instance;
        public static FileProvider Default
        {
            get { return _instance ?? (_instance = new FileProvider()); }
        }

        #endregion Instance Default

        public event EventHandler FilelistChangedHandler;

        private readonly List<string> _excludeMaskList = new List<string>()
        {
            "*.pdb",
            "*.vshost.*",
            "*.config"
        };

        private List<string> ExcludeRegexMaskList
        {
            get
            {
                var excludeRegexMaskList = this._excludeMaskList
                    .Select(m => string.Format("^{0}$", Regex.Escape(m).Replace(@"\*", ".*").Replace(@"\?", ".{1}")))
                    .ToList();
                excludeRegexMaskList.Add(string.Format("^{0}$", Regex.Escape(this.CurrentAssemblyFilename).Replace(@"\*", ".*").Replace(@"\?", ".{1}")));
                excludeRegexMaskList.Add(string.Format("^{0}$", Regex.Escape(this.CurrentAssemblyConfigFilename).Replace(@"\*", ".*").Replace(@"\?", ".{1}")));
                if (this._package != null)
                {
                    excludeRegexMaskList.Add(string.Format("^{0}$", Regex.Escape(this._package.PackageFilenameZip).Replace(@"\*", ".*").Replace(@"\?", ".{1}")));
                    excludeRegexMaskList.Add(string.Format("^{0}$", Regex.Escape(this._package.PackageFilenameXml).Replace(@"\*", ".*").Replace(@"\?", ".{1}")));
                }
                return excludeRegexMaskList;
            }
        }

        private string CurrentAssemblyFilename
        {
            get { return Path.GetFileName(Assembly.GetEntryAssembly().Location); }
        }
        private string CurrentAssemblyConfigFilename
        {
            get { return Path.GetFileName(Assembly.GetEntryAssembly().Location) + ".*"; }
        }

        private Package _package;
        public void PackageChanged(Package package)
        {
            this._package = package;
            if (this.FilelistChangedHandler != null)
            {
                this.FilelistChangedHandler(this, new EventArgs());
            }
        }

        private string CurrentDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); }
        }

        public string Filter
        {
            get { return string.Join("; ", this._excludeMaskList); }
            set
            {
                var mask = value ?? string.Empty;
                var maskSplitted = Regex.Split(mask.Trim(), @"\s*;\s*");
                this._excludeMaskList.Clear();
                this._excludeMaskList.AddRange(maskSplitted);
                if (this.FilelistChangedHandler != null)
                {
                    this.FilelistChangedHandler(this, new EventArgs());
                }
            }
        }

        private IEnumerable<CheckedFile> GetFileList()
        {
            var fileList = Directory
                .GetFiles(this.CurrentDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Select(f => f.Replace(this.CurrentDirectory, string.Empty).TrimStart('\\'))
                .Select(f => new CheckedFile(f));
            return fileList;
        }

        public List<CheckedFile> GetFilterFileList()
        {
            var fileList = this.GetFileList().ToList();
            var pattern = string.Join("|", this.ExcludeRegexMaskList);
            foreach (var file in fileList)
            {
                file.IsSelected = !Regex.IsMatch(file.Filename, pattern, RegexOptions.IgnoreCase);
            }
            return fileList;
        }
    }
}