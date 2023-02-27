using MicroService.Service.Models.Base;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using System.Collections.Generic;

namespace MicroService.Service.Interfaces
{
    public interface IShapeService<out T> where T : ShapeBase
    {
        IReadOnlyCollection<Feature> GetFeatures();

        T GetFeatureLookup(double x, double y);

        IEnumerable<T> GetFeatureLookup(List<KeyValuePair<string, object>> attributes);

        IEnumerable<T> GetFeatureAttributes();

        ShapefileHeader GetShapeProperties();

        DbaseFileHeader GetShapeDatabaseProperties();
    }
}
