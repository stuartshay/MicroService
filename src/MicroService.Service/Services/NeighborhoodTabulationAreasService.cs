using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace MicroService.Service.Services
{
    public class NeighborhoodTabulationAreasService : AbstractShapeService<NeighborhoodTabulationAreaShape>, IShapeService<NeighborhoodTabulationAreaShape>
    {
        public NeighborhoodTabulationAreasService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.NeighborhoodTabulationAreas));
        }

        public override NeighborhoodTabulationAreaShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new NeighborhoodTabulationAreaShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    model = new NeighborhoodTabulationAreaShape
                    {
                        BoroCode = int.Parse(f.Attributes["BoroCode"].ToString()),
                        BoroName = f.Attributes["BoroName"].ToString(),

                        CountyFIPS = f.Attributes["CountyFIPS"].ToString(),
                        NTA2020 = f.Attributes["NTA2020"].ToString(),
                        NTAName = f.Attributes["BoroCode"].ToString(),
                        NTAAbbrev = f.Attributes["NTAName"].ToString(),

                        NTAType = int.Parse(f.Attributes["NTAType"].ToString()),



                        CDTA2020 = f.Attributes["CDTA2020"].ToString(),
                        CDTAName = f.Attributes["CDTAName"].ToString(),
                    };
                }
            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;
        }

        public override IEnumerable<NeighborhoodTabulationAreaShape> GetFeatureLookup(
            List<KeyValuePair<string, string>> features)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NeighborhoodTabulationAreaShape> GetFeatureAttributes()
        {
            var features = GetFeatures();
            var results = new List<NeighborhoodTabulationAreaShape>(features.Count);

            foreach (var f in features)
            {
                var model = new NeighborhoodTabulationAreaShape
                {
                    BoroCode = int.Parse(f.Attributes["BoroCode"].ToString()),
                    BoroName = f.Attributes["BoroName"].ToString(),

                    CountyFIPS = f.Attributes["CountyFIPS"].ToString(),
                    NTA2020 = f.Attributes["NTA2020"].ToString(),
                    NTAName = f.Attributes["BoroCode"].ToString(),
                    NTAAbbrev = f.Attributes["NTAName"].ToString(),

                    NTAType = int.Parse(f.Attributes["NTAType"].ToString()),

                    CDTA2020 = f.Attributes["CDTA2020"].ToString(),
                    CDTAName = f.Attributes["CDTAName"].ToString(),

                };

                results.Add(model);
            }

            return results;
        }

    }
}
