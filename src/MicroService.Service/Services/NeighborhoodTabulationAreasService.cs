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
    public class NeighborhoodTabulationAreasService : AbstractShapeService<NeighborhoodTabulationAreaShape>, IShapeService<NeighborhoodTabulationAreaShape>
    {
        public NeighborhoodTabulationAreasService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            ILogger<NeighborhoodTabulationAreasService> logger)
            : base(logger, NeighborhoodTabulationAreaShapeProfile.Map)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.NeighborhoodTabulationAreas));
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                var featureAttributes = NeighborhoodTabulationAreaShapeProfile.ToDictionary(feature);
                featureAttributes.Add("ShapeColor", Color.Blue.ToString().ToLower());
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

    }
}
