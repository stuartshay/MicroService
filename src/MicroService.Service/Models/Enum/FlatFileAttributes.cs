namespace MicroService.Service.Models.Enum
{
    public class FlatFileAttributes : System.Attribute
    {
        public FlatFileAttributes(string directory, string fileName, string modelName, FileTypes fileType)
        {
            Directory = directory;
            FileName = fileName;
            FileType = fileType;
            ModelName = modelName;
        }

        public string Directory { get; set; }

        public string FileName { get; set; }

        public string ModelName { get; set; }

        public FileTypes FileType { get; set; }
    }


    public enum FileTypes
    {
        Csv,
        Xml,
        Json
    }


}
