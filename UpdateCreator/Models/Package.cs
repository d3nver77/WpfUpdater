using System;

namespace UpdateCreator.Models
{
    public class Package
    {
        public event EventHandler<PackageEventArgs> PackageNameChanged = (sender, args) => { };

        private string _packageName;
        public string PackageName
        {
            get { return this._packageName; }
            set
            {
                this._packageName = value;
                this.PackageNameChanged(this, new PackageEventArgs(this));
            }
        }

        public string ApplicationName { get; set; }
        public string Description { get; set; }
        public string Filename { get; set; }
        public string LaunchArguments { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public string Hash { get; set; }

        public string PackageFilenameZip
        {
            get
            {
                return string.IsNullOrEmpty(this.PackageName) ? string.Empty : string.Format("{0}.zip", this.PackageName);
            }
        }
        public string PackageFilenameXml
        {
            get
            {
                return string.IsNullOrEmpty(this.PackageName) ? string.Empty : string.Format("{0}.xml", this.PackageName);
            }
        }

        public Package()
        {
            
        }
    }
}