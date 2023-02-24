using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

namespace MicroService.Service.Services
{
    public class CommunityDistrictsService : AbstractShapeService<CommunityDistrictShape>, IShapeService<CommunityDistrictShape>
    {
        public CommunityDistrictsService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.CommunityDistricts));
        }

        public override CommunityDistrictShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new CommunityDistrictShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    model = new CommunityDistrictShape
                    {
                        Cd = int.Parse(f.Attributes["BoroCD"].ToString()),
                        BoroCd = int.Parse(f.Attributes["BoroCD"].ToString().Substring(1, 2)),
                        BoroCode = int.Parse(f.Attributes["BoroCD"].ToString().Substring(0, 1)),
                        Borough = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().ToString(),
                        BoroName = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().GetEnumDescription(),
                        ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                        ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
                        Coordinates = new List<Coordinate>()
                    };
                }
            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;

        }

        public override IEnumerable<CommunityDistrictShape> GetFeatureLookup(
            List<KeyValuePair<string, string>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<CommunityDistrictShape> GetFeatureAttributes()
        {
            var features = GetFeatures();
            var results = new List<CommunityDistrictShape>(features.Count);

            foreach (var f in features)
            {
                var model = new CommunityDistrictShape
                {
                    Cd = int.Parse(f.Attributes["BoroCD"].ToString()),
                    BoroCd = int.Parse(f.Attributes["BoroCD"].ToString().Substring(1, 2)),
                    BoroCode = int.Parse(f.Attributes["BoroCD"].ToString().Substring(0, 1)),
                    Borough = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().ToString(),
                    BoroName = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().GetEnumDescription(),
                    ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                    ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
                };

                results.Add(model);
            }

            return results.OrderBy(x => x.Cd);
        }
    }
}
