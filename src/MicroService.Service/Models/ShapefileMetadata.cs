using NetTopologySuite.Geometries;

namespace MicroService.Service.Models
{
    public sealed record ShapefileProperties
    {
        public Envelope Bounds { get; init; } = new();
        public string ShapeType { get; init; } = string.Empty;
    }

    public sealed record ShapeDatabaseProperties
    {
        public int NumRecords { get; init; }
        public int NumFields => Fields.Length;
        public DateTime LastUpdateDate { get; init; }
        public ShapeField[] Fields { get; init; } = [];
    }

    public sealed record ShapeField(string Name, Type Type, char DbaseType, int Length, int DecimalCount);
}
