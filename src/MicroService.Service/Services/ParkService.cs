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
    public class ParkService : AbstractShapeService<ParkShape>, IShapeService<ParkShape>
    {
        public ParkService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            ILogger<ParkService> logger)
            : base(logger, ParkShapeProfile.Map)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.Parks));
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                var featureAttributes = ParkShapeProfile.ToDictionary(feature);
                featureAttributes.Add("ShapeColor", Color.Blue.ToString().ToLower());
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public override IEnumerable<ParkShape> GetFeatureList()
        {
            var features = GetFeatures();

            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("Feature Count|{Count}", features.Count);
            }
            return features.Select(MapFeature).Take(100);
        }
    }
}
