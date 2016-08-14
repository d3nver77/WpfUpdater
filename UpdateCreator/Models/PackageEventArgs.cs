using System;

namespace UpdateCreator.Models
{
    public class PackageEventArgs : EventArgs
    {
        public Package Package { get; }

        public PackageEventArgs(Package package)
        {
            this.Package = package;
        }
    }
}