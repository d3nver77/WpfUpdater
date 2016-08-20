using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;

namespace UpdateCreator.Models
{
    public class PackageBuilderIonic : PackageBuilder
    {
        public PackageBuilderIonic(Package package) : base(package)
        {
            
        }
        protected override async void CreatePackageFileZip()
        {
            await Task.Run(() =>
            {
                using (var zipFile = new ZipFile(this.Package.PackageFilenameZip))
                {
                    // add content to zip here 
                    
                    //zipFile.AddProgress += (sender, args) =>
                    //{
                    //    var percentage = (int)(1.0d / args.TotalBytesToTransfer * args.BytesTransferred * 100.0d);
                    //    // report your progress
                    //    PackProgressChanged(this, new ProgressEventArgs(args.CurrentEntry.FileName, percentage));
                    //    Thread.Sleep(500);
                    //};
                    zipFile.AddFiles(this.FileList);
                    zipFile.SaveProgress +=
                        (o, args) =>
                        {
                            var percentage = (int)(1.0d / args.TotalBytesToTransfer * args.BytesTransferred * 100.0d);
                            // report your progress
                            var name = args.CurrentEntry != null ? args.CurrentEntry.FileName : string.Empty;
                            //OnPackProgressChanged(this, new ProgressEventArgs(name, percentage, ProgressStatus.Running));
                            //Thread.Sleep(500);
                        };
                    zipFile.Save();
                    //OnPackProgressChanged(this, new ProgressEventArgs(string.Empty, 100, ProgressStatus.Completed));
                }
            });
        }
    }
}