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
    public class ZipCodeService : AbstractShapeService<ZipCodeShape>, IShapeService<ZipCodeShape>
    {
        public ZipCodeService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<ZipCodeService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.ZipCodes));
        }

        public virtual ZipCodeShape GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var features = GetFeatures();
            var feature = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return Mapper.Map<ZipCodeShape>(feature);
        }

        public IEnumerable<ZipCodeShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
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
                          select Mapper.Map<ZipCodeShape>(f);

            return results;
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            attributes = ValidateFeatureKey(attributes);
            var featureCollection = new FeatureCollection();

            var features = from f in GetFeatures()
                           where attributes.All(pair =>
                           {
                               var value = f.Attributes[pair.Key];
                               var expectedValue = pair.Value;
                               var matchedValue = MatchAttributeValue(value, expectedValue);
                               return matchedValue != null;
                           })
                           select Mapper.Map<ZipCodeShape>(f);

            foreach (var feature in features)
            {
                var featureProperties = EnumHelper.GetPropertiesWithoutExcludedAttribute<ZipCodeShape, FeatureCollectionExcludeAttribute>();
                var featureAttributes = featureProperties
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(feature, null));

                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public IEnumerable<ZipCodeShape> GetFeatureList()
        {
            var features = GetFeatures();
            return Mapper.Map<IEnumerable<ZipCodeShape>>(features);
        }
    }
}