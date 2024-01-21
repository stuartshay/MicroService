using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum.Attributes;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using System.Collections.Generic;

namespace MicroService.Service.Interfaces
{
    public interface IShapeService<out T> where T : ShapeBase
    {
        IReadOnlyCollection<Feature> GetFeatures();

        T GetFeatureLookup(double x, double y, Datum datum);

        IEnumerable<T> GetFeatureLookup(List<KeyValuePair<string, object>> attributes);

        FeatureCollection? GetFeatureCollection(List<KeyValuePair<string, object>>? attributes);

        IEnumerable<T> GetFeatureList();

        ShapefileHeader GetShapeProperties();

        DbaseFileHeader GetShapeDatabaseProperties();
    }
}
