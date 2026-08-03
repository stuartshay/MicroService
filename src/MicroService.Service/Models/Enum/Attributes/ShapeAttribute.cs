namespace MicroService.Service.Models.Enum.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
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
