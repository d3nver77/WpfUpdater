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
        #region Instance

        private static FileProvider _instance;
        public static FileProvider Instance
        {
            get { return _instance ?? (_instance = new FileProvider()); }
        }

        #endregion Instance

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
                return this._excludeMaskList
                    .Select(m => string.Format("^{0}$", Regex.Escape(m).Replace(@"\*", ".*").Replace(@"\?", ".{1}")))
                    .ToList();
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
                file.IsSelected = !Regex.IsMatch(file.Filename, pattern);
            }
            return fileList;
        }
    }
}