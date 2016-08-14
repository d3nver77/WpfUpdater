using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace UpdateCreator.Models
{
    public class FileProvider
    {
        #region Instance Default

        private static FileProvider _instance;
        public static FileProvider Default => _instance ?? (_instance = new FileProvider());

        #endregion Instance Default

        public event EventHandler FilelistChangedHandler = (sender, args) => { };

        private readonly List<string> _excludeMaskList = new List<string>()
        {
            "*.pdb",
            "*.vshost.*",
            "*.config"
        };

        private IEnumerable<string> ExcludeRegexMaskList
        {
            get
            {
                var excludeRegexMaskList = this._excludeMaskList
                    .Select(m => $"^{Regex.Escape(m).Replace(@"\*", ".*").Replace(@"\?", ".{1}")}$");
                return excludeRegexMaskList;
            }
        }

        private List<string> ExcludeRegexFileList
        {
            get
            {
                var excludeRegexFileList = new List<string>
                {
                    $"^{Regex.Escape(this.CurrentAssemblyMaskFilename).Replace(@"\*", ".*").Replace(@"\?", ".{1}")}$"
                };
                if (this._package != null)
                {
                    excludeRegexFileList.Add($"^{Regex.Escape(this._package.PackageFilenameZip).Replace(@"\*", ".*").Replace(@"\?", ".{1}")}$");
                    excludeRegexFileList.Add($"^{Regex.Escape(this._package.PackageFilenameXml).Replace(@"\*", ".*").Replace(@"\?", ".{1}")}$");
                }
                return excludeRegexFileList;
            }
        }

        private string CurrentAssemblyMaskFilename => Assembly.GetEntryAssembly().GetName().Name + ".*";

        private Package _package;
        public void PackageChanged(Package package)
        {
            this._package = package;
            this.FilelistChangedHandler(this, new EventArgs());
        }

        private string CurrentDirectory => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        public string Filter
        {
            get { return string.Join("; ", this._excludeMaskList); }
            set
            {
                var mask = value ?? string.Empty;
                var maskSplitted = Regex.Split(mask.Trim(), @"\s*;\s*");
                this._excludeMaskList.Clear();
                this._excludeMaskList.AddRange(maskSplitted);
                this.FilelistChangedHandler(this, new EventArgs());
            }
        }
        public List<CheckedFile> GetFileList()
        {
            var fileList = this.CreateFileList().ToList();
            var pattern = string.Join("|", this.ExcludeRegexMaskList);
            foreach (var file in fileList)
            {
                file.IsSelected = !Regex.IsMatch(file.Filename, pattern, RegexOptions.IgnoreCase);
            }
            return fileList;
        }

        private IEnumerable<CheckedFile> CreateFileList()
        {
            var fileList = Directory
                .GetFiles(this.CurrentDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Select(f => f.Replace(this.CurrentDirectory, string.Empty).TrimStart('\\'))
                .Select(f =>
                {
                    var checkedFile = new CheckedFile(f);
                    checkedFile.PropertyChanged += this.CheckedFileOnPropertyChanged;
                    return checkedFile;
                });
            return fileList;
        }

        private void CheckedFileOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var file = sender as CheckedFile;
            if (file == null || !file.IsSelected)
                return;
            var pattern = string.Join("|", this.ExcludeRegexFileList);
            file.IsSelected = !Regex.IsMatch(file.Filename, pattern, RegexOptions.IgnoreCase);
        }
    }
}