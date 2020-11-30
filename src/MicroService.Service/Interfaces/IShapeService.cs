using System.Collections.Generic;
using MicroService.Service.Models.Base;
using NetTopologySuite.Features;
using NetTopologySuite.IO;

namespace MicroService.Service.Interfaces
{
    public interface IShapeService<out T> where T : ShapeBase
    {
        IReadOnlyCollection<Feature> GetFeatures();

        T GetFeatureLookup(double x, double y);

        ShapefileHeader GetShapeProperties();

        DbaseFileHeader GetShapeDatabaseProperties();
    }
}
