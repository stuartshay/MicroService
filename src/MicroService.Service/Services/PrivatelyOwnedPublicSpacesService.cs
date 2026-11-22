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
    public class PrivatelyOwnedPublicSpacesService : AbstractPointShapeService<PrivatelyOwnedPublicSpaceShape>, IShapeService<PrivatelyOwnedPublicSpaceShape>
    {
        public PrivatelyOwnedPublicSpacesService(ShapefileDataReaderResolver shapefileDataReaderResolver, ILogger<PrivatelyOwnedPublicSpacesService> logger)
            : base(shapefileDataReaderResolver, logger, PrivatelyOwnedPublicSpaceShapeProfile.Map)
        {
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            foreach (var feature in GetFeatureLookup(attributes))
            {
                var featureAttributes = PrivatelyOwnedPublicSpaceShapeProfile.ToDictionary(feature);
                featureAttributes.Add("ShapeColor", nameof(Color.Yellow).ToLower());
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }
    }
}
