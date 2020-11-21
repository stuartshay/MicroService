using System.Collections.Generic;
using MicroService.Service.Models;
using MicroService.Service.Models.Base;
using NetTopologySuite.Features;
using NetTopologySuite.IO;

namespace MicroService.Service.Interfaces
{
    public interface IShapeService<T> where T : ShapeBase
    {
        List<Feature> GetFeatures();

        T GetFeatureLookup(double x, double y);

        ShapefileHeader GetShapeProperties();

        DbaseFileHeader GetShapeDatabaseProperties();

    }
}
