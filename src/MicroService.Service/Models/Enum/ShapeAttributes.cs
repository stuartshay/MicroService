namespace MicroService.Service.Models.Enum
{
    public class ShapeAttributes : System.Attribute
    {
        public ShapeAttributes(string directory, string fileName)
        {
            Directory = directory;
            FileName = fileName;
        }

        public string Directory { get; set; }

        public string FileName { get; set; }

    }
}
