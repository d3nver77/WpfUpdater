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
}