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
        public event EventHandler<SelectedFileEventArgs> SelectedFileChangedHandler = (sender, args) => { };

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

        private CheckedFile _selectedFile;
        public CheckedFile SelectedFile
        {
            get { return this._selectedFile; }
            set
            {
                if (this._selectedFile != value)
                {
                    this._selectedFile = value;
                    this.SelectedFileChangedHandler(this, new SelectedFileEventArgs(this._selectedFile));
                }
            }
        }

        private List<CheckedFile> _fileList;
        public List<CheckedFile> GetFileList(bool isCreateNewFileList = false)
        {
            if (this._fileList != null && !isCreateNewFileList)
            {
                return this._fileList;
            }
            this._fileList = this.CreateFileList().ToList();
            var pattern = string.Join("|", this.ExcludeRegexMaskList);
            foreach (var file in this._fileList)
            {
                file.IsSelected = !Regex.IsMatch(file.Filename, pattern, RegexOptions.IgnoreCase);
            }
            if (this.SelectedFile != null)
            {
                this.SelectedFile = this._fileList.FirstOrDefault(f => string.Equals(f.Filename, this.SelectedFile.Filename, StringComparison.InvariantCultureIgnoreCase) && f.IsSelected);
            }
            return this._fileList;
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
            if (file == null)
                return;
            if (!file.IsSelected)
            {
                if (this.SelectedFile == file)
                {
                    this.SelectedFile = null;
                }
                return;
            }

            var pattern = string.Join("|", this.ExcludeRegexFileList);
            file.IsSelected = !Regex.IsMatch(file.Filename, pattern, RegexOptions.IgnoreCase);
        }

        public bool IsDragable(CheckedFile file)
        {
            if (file == null)
                return false;
            var pattern = string.Join("|", this.ExcludeRegexFileList);
            var isDragable = !Regex.IsMatch(file.Filename, pattern, RegexOptions.IgnoreCase);
            return isDragable;
        }
    }
}