using System.Text;
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
        protected override void CreatePackageFileZip()
        {
            var count = 0;
            this.Progress.Report(new ProgressEventArgs(this.CurrentFileName, this.Percentage, ProgressStatus.Started));
            using (var zipFile = new ZipFile(this.Package.PackageFilenameZip, Encoding.UTF8))
            {
                // add content to zip here 
                zipFile.AddFiles(this.FileList);

                zipFile.SaveProgress +=
                    (o, args) =>
                    {
                        this.Percentage = (int) (1.0d/args.TotalBytesToTransfer*args.BytesTransferred*100.0d);
                        // report your progress
                        this.CurrentFileName = args.CurrentEntry != null ? args.CurrentEntry.FileName : string.Empty;
                        this.Progress.Report(new ProgressEventArgs(this.CurrentFileName, this.Percentage, ProgressStatus.Started));
                        //Thread.Sleep(200);
                    };
                zipFile.Save();
            }
        }
    }
}