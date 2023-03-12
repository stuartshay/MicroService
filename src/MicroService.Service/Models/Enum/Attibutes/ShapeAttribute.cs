namespace MicroService.Service.Models.Enum.Attibutes
{
    public class ShapeAttribute : System.Attribute
    {
        public ShapeAttribute(string directory, string fileName, Datum datum)
        {
            Directory = directory;
            FileName = fileName;
            Datum = datum;
        }

        public string Directory { get; set; }

        public string FileName { get; set; }

        public Datum Datum { get; set; }

    }

    public enum Datum
    {
        Wgs84,
        Nad83,
    }
}
