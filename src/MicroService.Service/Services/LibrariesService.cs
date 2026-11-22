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
    public class LibrariesService : AbstractPointShapeService<LibraryShape>, IShapeService<LibraryShape>
    {
        public LibrariesService(ShapefileDataReaderResolver shapefileDataReaderResolver, ILogger<LibrariesService> logger)
            : base(shapefileDataReaderResolver, logger, LibraryShapeProfile.Map)
        {
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            foreach (var feature in GetFeatureLookup(attributes))
            {
                var featureAttributes = LibraryShapeProfile.ToDictionary(feature);
                featureAttributes.Add("ShapeColor", nameof(Color.Blue).ToLower());
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }
    }
}
