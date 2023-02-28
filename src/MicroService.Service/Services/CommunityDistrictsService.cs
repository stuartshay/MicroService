using AutoMapper;
using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

namespace MicroService.Service.Services
{
    public class CommunityDistrictsService : AbstractShapeService<CommunityDistrictShape>, IShapeService<CommunityDistrictShape>
    {
        private readonly IMapper _mapper;

        public CommunityDistrictsService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<CommunityDistrictsService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.CommunityDistricts));
            _mapper = mapper;
        }

        public override CommunityDistrictShape GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var feature = GetFeatures().FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return new CommunityDistrictShape
            {
                Cd = int.Parse(feature.Attributes["BoroCD"].ToString()),
                BoroCd = int.Parse(feature.Attributes["BoroCD"].ToString().Substring(1, 2)),
                BoroCode = int.Parse(feature.Attributes["BoroCD"].ToString().Substring(0, 1)),
                Borough = feature.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().ToString(),
                BoroName = feature.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().GetEnumDescription(),
                ShapeArea = double.Parse(feature.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["Shape_Leng"].ToString()),
                Coordinates = new List<Coordinate>()
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
                              Cd = int.Parse(f.Attributes["BoroCD"].ToString()),
                              BoroCd = int.Parse(f.Attributes["BoroCD"].ToString().Substring(1, 2)),
                              BoroCode = int.Parse(f.Attributes["BoroCD"].ToString().Substring(0, 1)),
                              Borough = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().ToString(),
                              BoroName = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().GetEnumDescription(),
                              ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                              ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
                              Coordinates = new List<Coordinate>()
                          };

            return results;
        }

        public IEnumerable<CommunityDistrictShape> GetFeatureAttributes()
        {
            var features = GetFeatures();

            return features.Select(f => new CommunityDistrictShape
            {
                Cd = int.Parse(f.Attributes["BoroCD"].ToString()),
                BoroCd = int.Parse(f.Attributes["BoroCD"].ToString().Substring(1, 2)),
                BoroCode = int.Parse(f.Attributes["BoroCD"].ToString().Substring(0, 1)),
                Borough = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().ToString(),
                BoroName = f.Attributes["BoroCD"].ToString().Substring(0, 1).ParseEnum<Borough>().GetEnumDescription(),
                ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
            }).OrderBy(x => x.Cd);
        }

    }
}
