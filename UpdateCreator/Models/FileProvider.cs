using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private readonly string[] _defaultExcludeMaskArray = new[] { "*.pdb", "*.vshost.*", "*.config" };

        private List<string> _excludeMaskList;
        public List<string> ExcludeMaskList
        {
            get { return this._excludeMaskList 
                    ?? (this._excludeMaskList = new List<string>(this._defaultExcludeMaskArray)); }
        }

        private string CurrentDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); }
        }

        public List<string> GetFileList()
        {
            var fileList = Directory
                .GetFiles(this.CurrentDirectory, "*.*", SearchOption.TopDirectoryOnly)
                .Select(f => f.Replace(this.CurrentDirectory, string.Empty).TrimStart('\\'))
                .ToList();
            return fileList;
        }

        public List<CheckedFile> GetFilterFileList()
        {
            var fileList = this.GetFileList().Select(f => new CheckedFile(f)).ToList();
            return fileList;
        }
    }
}