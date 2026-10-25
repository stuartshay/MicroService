using MicroService.Service.Interfaces;
using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using MicroService.Service.Services.Base;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class DsnyDistrictsService : AbstractShapeService<DsnyDistrictsShape>, IShapeService<DsnyDistrictsShape>
    {
        public DsnyDistrictsService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            ILogger<DsnyDistrictsService> logger)
            : base(shapefileDataReaderResolver, logger, DsnyDistrictsShapeProfile.Map)
        {
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                var featureAttributes = DsnyDistrictsShapeProfile.ToDictionary(feature);
                featureAttributes.Add("ShapeColor", nameof(Color.Blue).ToLower());
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public override IEnumerable<DsnyDistrictsShape> GetFeatureList()
        {
            var features = GetFeatures();

            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("FeatureCount {FeatureCount}", features.Count);
            }

            var results = features.Select(MapFeature).OrderBy(x => x.District);
            return results;
        }
    }
}
