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
    /// <summary>
    /// The MTA's subway service lines as a shape. /api/v1/Subway serves them merged, clipped
    /// and simplified for maps (<see cref="Subway.SubwayRouteService"/>).
    /// </summary>
    public class SubwayServiceLinesService : AbstractShapeService<SubwayServiceLineShape>, IShapeService<SubwayServiceLineShape>
    {
        public SubwayServiceLinesService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            ILogger<SubwayServiceLinesService> logger)
            : base(shapefileDataReaderResolver, logger, SubwayServiceLineShapeProfile.Map)
        {
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                var featureAttributes = SubwayServiceLineShapeProfile.ToDictionary(feature);
                featureAttributes.Add("ShapeColor", nameof(Color.Blue).ToLower());
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }
    }
}
