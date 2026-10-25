using MicroService.Service.Interfaces;
using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using MicroService.Service.Services.Base;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using System.Collections.Generic;

namespace MicroService.Service.Services
{
    public class ZipCodeService : AbstractShapeService<ZipCodeShape>, IShapeService<ZipCodeShape>
    {
        public ZipCodeService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            ILogger<ZipCodeService> logger)
            : base(shapefileDataReaderResolver, logger, ZipCodeMappingsProfile.Map)
        {
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                var featureAttributes = ZipCodeMappingsProfile.ToDictionary(feature);
                featureAttributes.Add("ShapeColor", nameof(Color.Blue).ToLower());
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }
    }
}