using MicroService.Service.Interfaces;
using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum.Attributes;
using MicroService.Service.Services.Base;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using System.Collections.Generic;

namespace MicroService.Service.Services
{
    public class GreenstreetsService : AbstractShapeService<GreenstreetShape>, IShapeService<GreenstreetShape>
    {
        public GreenstreetsService(ShapefileDataReaderResolver shapefileDataReaderResolver, ILogger<GreenstreetsService> logger)
            : base(shapefileDataReaderResolver, logger, GreenstreetShapeProfile.Map)
        {
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            foreach (var feature in GetFeatureLookup(attributes))
            {
                var featureAttributes = GreenstreetShapeProfile.ToDictionary(feature);
                featureAttributes.Add("ShapeColor", nameof(Color.Green).ToLower());
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }
    }
}
