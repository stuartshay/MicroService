using System.Collections.Generic;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using NetTopologySuite.IO;

namespace MicroService.Service.Interfaces
{
    public interface INypdPolicePrecinctService
    {
        List<Feature> GetFeatures();

        NypdPrecinctShape GetFeatureLookup(double x, double y);

        ShapefileHeader GetShapeProperties();

        DbaseFileHeader GetShapeDatabaseProperties();

    }
}
