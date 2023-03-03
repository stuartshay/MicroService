using AutoMapper;
using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class CommunityDistrictsService : AbstractShapeService<CommunityDistrictShape>, IShapeService<CommunityDistrictShape>
    {
        public CommunityDistrictsService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<CommunityDistrictsService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.CommunityDistricts));
        }

        public virtual CommunityDistrictShape GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var feature = GetFeatures().FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return new CommunityDistrictShape
            {
                Cd = int.Parse(feature.Attributes["BoroCD"].ToString().Substring(1, 2)),
                BoroCd = int.Parse(feature.Attributes["BoroCD"].ToString()),
                BoroCode = int.Parse(feature.Attributes["BoroCD"].ToString().Substring(0, 1)),
                Borough = feature.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().ToString(),
                BoroName = feature.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().GetEnumDescription(),
                ShapeArea = double.Parse(feature.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["Shape_Leng"].ToString()),
            };
        }

        public override IEnumerable<CommunityDistrictShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
        {
            attributes = ValidateFeatureKey(attributes);

            var results = from f in GetFeatures()
                          where attributes.All(pair =>
                          {
                              var value = f.Attributes[pair.Key];
                              var expectedValue = pair.Value;
                              var matchedValue = MatchAttributeValue(value, expectedValue);
                              return matchedValue != null;
                          })
                          select new CommunityDistrictShape
                          {
                              Cd = int.Parse(f.Attributes["BoroCD"].ToString().Substring(1, 2)),
                              BoroCd = int.Parse(f.Attributes["BoroCD"].ToString()),
                              BoroCode = int.Parse(f.Attributes["BoroCD"].ToString().Substring(0, 1)),
                              Borough = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().ToString(),
                              BoroName = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().GetEnumDescription(),
                              ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                              ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
                          };

            return results;
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<CommunityDistrictShape> GetFeatureList()
        {
            var features = GetFeatures();

            return features.Select(f => new CommunityDistrictShape
            {
                Cd = int.Parse(f.Attributes["BoroCD"].ToString().Substring(1, 2)),
                BoroCd = int.Parse(f.Attributes["BoroCD"].ToString()),
                BoroCode = int.Parse(f.Attributes["BoroCD"].ToString().Substring(0, 1)),
                Borough = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().ToString(),
                BoroName = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().GetEnumDescription(),
                ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
            }).OrderBy(x => x.Cd);
        }

    }
}
