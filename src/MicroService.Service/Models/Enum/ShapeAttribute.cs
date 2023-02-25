namespace MicroService.Service.Models.Enum
{
    public class ShapeAttribute : System.Attribute
    {
        public ShapeAttribute(string directory, string fileName)
        {
            Directory = directory;
            FileName = fileName;
        }

        public string Directory { get; set; }

        public string FileName { get; set; }

    }
}
