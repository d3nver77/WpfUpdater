using System;

namespace UpdateCreator.Models
{

    public class PackageEventArgs : EventArgs
    {
        public Package Package { get; set; }

        public PackageEventArgs(Package package)
        {
            this.Package = package;
        }
    }

    public class Package
    {
        public event EventHandler<PackageEventArgs> PackageNameChanged;

        private string _packageName;

        public string PackageName
        {
            get { return this._packageName; }
            set
            {
                this._packageName = value;
                if (this.PackageNameChanged != null)
                {
                    this.PackageNameChanged(this, new PackageEventArgs(this));
                }
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