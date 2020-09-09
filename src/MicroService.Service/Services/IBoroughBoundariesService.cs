using System;
using System.Collections.Generic;
using System.Text;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace MicroService.Service.Services
{
    public interface IBoroughBoundariesService
    {
        List<Feature> GetFeatures();

        BoroughBoundaries GetFeatureLookup(double x, double y);

        ShapefileHeader GetShapeProperties();

        DbaseFileHeader GetShapeDatabaseProperties();
    }
}
