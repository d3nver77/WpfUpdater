﻿using System;
using System.IO;
using System.Xml.Serialization;

namespace UpdateCreator.Models
{
    [Serializable]
    [XmlRoot("Update")]
    public class Package
    {
        public event EventHandler<PackageNameEventArgs> PackageNameChanged = (sender, args) => { };

        private string _packageName;
        private string _url = string.Empty;

        [XmlIgnore]
        public string PackageName
        {
            get { return this._packageName; }
            set
            {
                this._packageName = value;
                this.PackageNameChanged(this, new PackageNameEventArgs(this));
            }
        }

        [XmlAttribute("Product")]
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LaunchFile { get; set; } = string.Empty;
        public string LaunchArguments { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;

        public string Url
        {
            get { return this._url; }
            set { this._url = Path.Combine(value, this.PackageFilenameZip); }
        }

        public string Hash { get; set; } = string.Empty;

        public string PackageFilenameZip => string.IsNullOrEmpty(this.PackageName) ? string.Empty : string.Format("{0}.zip", this.PackageName);
        public string PackageFilenameXml => string.IsNullOrEmpty(this.PackageName) ? string.Empty : string.Format("{0}.xml", this.PackageName);
    }
}