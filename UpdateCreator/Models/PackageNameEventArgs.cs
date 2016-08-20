using System;

namespace UpdateCreator.Models
{
    public class PackageNameEventArgs : EventArgs
    {
        public Package Package { get; }
        public double Percent { get; }

        public PackageNameEventArgs(Package package)
        {
            this.Package = package;
        }
    }
}