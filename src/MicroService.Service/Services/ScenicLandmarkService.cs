using AutoMapper;
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
    public class ScenicLandmarkService : AbstractShapeService<ScenicLandmarkShape, FeatureToScenicLandmarkShapeProfile>, IShapeService<ScenicLandmarkShape>
    {
        public ScenicLandmarkService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<ScenicLandmarkService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.ScenicLandmarks));
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                var featureAttributes = Mapper.Map<IDictionary<string, object>>(feature);
                featureAttributes.Add("ShapeColor", Color.Blue.ToString().ToLower());
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }
    }
}
