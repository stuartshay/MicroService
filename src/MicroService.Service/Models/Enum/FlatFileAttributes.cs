namespace MicroService.Service.Models.Enum
{
    public class FlatFileAttributes : System.Attribute
    {
        public FlatFileAttributes(string directory, string fileName, FileTypes fileType)
        {
            Directory = directory;
            FileName = fileName;
            FileType = fileType;
        }

        public string Directory { get; set; }

        public string FileName { get; set; }

        public FileTypes FileType { get; set; }
    }


    public enum FileTypes
    {
        Csv,
        Xml,
        Json
    }


}
