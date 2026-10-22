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
    public class BoroughBoundariesService : AbstractShapeService<BoroughBoundaryShape>, IShapeService<BoroughBoundaryShape>
    {
        public BoroughBoundariesService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            ILogger<BoroughBoundariesService> logger)
            : base(logger, BoroughBoundaryShapeProfile.Map)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.BoroughBoundaries));
        }
        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                var featureAttributes = BoroughBoundaryShapeProfile.ToDictionary(feature);
                featureAttributes.Add("ShapeColor", Color.Green.ToString().ToLower());

                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public override IEnumerable<BoroughBoundaryShape> GetFeatureList()
        {
            var features = GetFeatures();
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("FeatureCount {FeatureCount}", features.Count);
            }

            var results = features.Select(MapFeature).OrderBy(x => x.BoroCode);
            return results;
        }
    }
}
