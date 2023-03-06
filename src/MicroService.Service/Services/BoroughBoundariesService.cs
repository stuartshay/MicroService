using AutoMapper;
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
    public class BoroughBoundariesService : AbstractShapeService<BoroughBoundaryShape>, IShapeService<BoroughBoundaryShape>
    {
        public BoroughBoundariesService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<BoroughBoundariesService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.BoroughBoundaries));
            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        public virtual BoroughBoundaryShape GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var features = GetFeatures();

            var feature = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return Mapper.Map<BoroughBoundaryShape>(feature);
        }

        public IEnumerable<BoroughBoundaryShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
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
                            .Select(f => Mapper.Map<BoroughBoundaryShape>(f));

            return results;
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            attributes = ValidateFeatureKey(attributes);
            var featureCollection = new FeatureCollection();

            var features = GetFeatures()
                .Where(f => attributes.All(pair =>
                {
                    var value = f.Attributes[pair.Key];
                    var expectedValue = pair.Value;
                    var matchedValue = MatchAttributeValue(value, expectedValue);
                    return matchedValue != null;
                }))
                .Select(f => new BoroughBoundaryShape
                {
                    BoroCode = int.Parse(f.Attributes["BoroCode"].ToString()),
                    BoroName = f.Attributes["BoroName"].ToString(),
                    ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                    ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
                    Geometry = f.Geometry,
                });

            foreach (var feature in features)
            {
                var featureProperties = EnumHelper.GetPropertiesWithoutExcludedAttribute<BoroughBoundaryShape, FeatureCollectionExcludeAttribute>();
                var featureAttributes = featureProperties
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(feature, null));

                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public IEnumerable<BoroughBoundaryShape> GetFeatureList()
        {
            var features = GetFeatures();
            Logger.LogInformation("FeatureCount {FeatureCount}", features.Count);

            var results = Mapper.Map<IEnumerable<BoroughBoundaryShape>>(features).OrderBy(x => x.BoroCode);
            return results;
        }
    }
}
