namespace UpdateCreator.Models
{
    public class CheckedFile
    {
        public bool IsSelected { get; set; }
        public string Filename { get; set; }
        
        public CheckedFile(string filename, bool isSelected = true)
        {
            this.Filename = filename;
            this.IsSelected = isSelected;
        }
    }
}