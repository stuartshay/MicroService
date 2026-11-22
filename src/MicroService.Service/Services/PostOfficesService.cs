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
    public class PostOfficesService : AbstractPointShapeService<PostOfficeShape>, IShapeService<PostOfficeShape>
    {
        public PostOfficesService(ShapefileDataReaderResolver shapefileDataReaderResolver, ILogger<PostOfficesService> logger)
            : base(shapefileDataReaderResolver, logger, PostOfficeShapeProfile.Map)
        {
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            foreach (var feature in GetFeatureLookup(attributes))
            {
                var featureAttributes = PostOfficeShapeProfile.ToDictionary(feature);
                featureAttributes.Add("ShapeColor", nameof(Color.Red).ToLower());
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }
    }
}
