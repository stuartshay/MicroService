using MicroService.Service.Models;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum.Attributes;
using NetTopologySuite.Features;
using System.Collections.Generic;

namespace MicroService.Service.Interfaces
{
    public interface IShapeService<out T> where T : ShapeBase
    {
        IReadOnlyCollection<Feature> GetFeatures();

        T? GetFeatureLookup(double x, double y, Datum datum);

        IEnumerable<T> GetFeatureLookup(List<KeyValuePair<string, object>>? attributes);

        FeatureCollection? GetFeatureCollection(List<KeyValuePair<string, object>> attributes);

        IEnumerable<T> GetFeatureList();

        ShapefileProperties GetShapeProperties();

        ShapeDatabaseProperties GetShapeDatabaseProperties();
    }
}
