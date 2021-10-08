using System.Collections.Generic;
using System.Linq;
using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;

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
                        BoroName = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().GetEnumDescription()
                    };
                }
            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;

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
                    BoroName = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().GetEnumDescription()
                };

                results.Add(model);
            }

            return results.OrderBy(x => x.Cd);
        }
    }
}
