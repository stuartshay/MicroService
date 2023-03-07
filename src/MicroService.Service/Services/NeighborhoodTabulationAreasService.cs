using AutoMapper;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class NeighborhoodTabulationAreasService : AbstractShapeService<NeighborhoodTabulationAreaShape, FeatureToNeighborhoodTabulationAreaShapeProfile>, IShapeService<NeighborhoodTabulationAreaShape>
    {
        public NeighborhoodTabulationAreasService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<NeighborhoodTabulationAreasService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.NeighborhoodTabulationAreas));
        }

        public virtual NeighborhoodTabulationAreaShape GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var features = GetFeatures();
            var f = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (f == null)
            {
                return null;
            }

            return Mapper.Map<NeighborhoodTabulationAreaShape>(f);
        }

        public IEnumerable<NeighborhoodTabulationAreaShape> GetFeatureLookup(
            List<KeyValuePair<string, object>> attributes)
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
                          select Mapper.Map<NeighborhoodTabulationAreaShape>(f);

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
                           select Mapper.Map<NeighborhoodTabulationAreaShape>(f);

            foreach (var feature in features)
            {
                var featureProperties = EnumHelper.GetPropertiesWithoutExcludedAttribute<NeighborhoodTabulationAreaShape, FeatureCollectionExcludeAttribute>();
                var featureAttributes = featureProperties
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(feature, null));

                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public IEnumerable<NeighborhoodTabulationAreaShape> GetFeatureList()
        {
            var features = GetFeatures();
            return Mapper.Map<IEnumerable<NeighborhoodTabulationAreaShape>>(features);
        }

    }
}
