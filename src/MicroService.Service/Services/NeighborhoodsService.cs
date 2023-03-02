using AutoMapper;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class NeighborhoodsService : AbstractShapeService<NeighborhoodShape>, IShapeService<NeighborhoodShape>
    {
        public NeighborhoodsService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<NeighborhoodsService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.Neighborhoods));
        }


        public override NeighborhoodShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var feature = GetFeatures().FirstOrDefault(f => f.Geometry.Contains(point));
            if (feature == null)
            {
                return null;
            }

            return new NeighborhoodShape
            {
                BoroCode = int.Parse(feature.Attributes["BoroCode"].ToString()),
                BoroName = feature.Attributes["BoroName"].ToString(),
                CountyFIPS = feature.Attributes["CountyFIPS"].ToString(),
                NTACode = feature.Attributes["NTACode"].ToString(),
                NTAName = feature.Attributes["NTAName"].ToString(),
                ShapeArea = double.Parse(feature.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["Shape_Leng"].ToString()),
            };
        }


        public override IEnumerable<NeighborhoodShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
        {
            attributes = ValidateFeatureKey(attributes);

            var results = GetFeatures()
                .Where(f => attributes.All(pair =>
                {
                    var value = f.Attributes[pair.Key];
                    var expectedValue = pair.Value;
                    var matchedValue = MatchAttributeValue(value, expectedValue);
                    return matchedValue != null;
                }))
                .Select(f => new NeighborhoodShape
                {
                    BoroCode = int.Parse(f.Attributes["BoroCode"].ToString()),
                    BoroName = f.Attributes["BoroName"].ToString(),
                    CountyFIPS = f.Attributes["CountyFIPS"].ToString(),
                    NTACode = f.Attributes["NTACode"].ToString(),
                    NTAName = f.Attributes["NTAName"].ToString(),
                    ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                    ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
                });

            return results;
        }

        public IEnumerable<NeighborhoodShape> GetFeatureList()
        {
            var features = GetFeatures();

            return features.Select(f => new NeighborhoodShape
            {
                BoroCode = int.Parse(f.Attributes["BoroCode"].ToString()),
                BoroName = f.Attributes["BoroName"].ToString(),
                CountyFIPS = f.Attributes["CountyFIPS"].ToString(),
                NTACode = f.Attributes["NTACode"].ToString(),
                NTAName = f.Attributes["NTAName"].ToString(),
                ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
            }).OrderBy(x => x.BoroCode);
        }

    }
}
