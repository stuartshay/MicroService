using AutoMapper;
using MicroService.Service.Interfaces;
using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using MicroService.Service.Services.Base;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class NeighborhoodsService : AbstractShapeService<NeighborhoodShape, NeighborhoodShapeMappingsProfile>, IShapeService<NeighborhoodShape>
    {
        public NeighborhoodsService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<NeighborhoodsService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.Neighborhoods));
        }


        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                var featureAttributes = Mapper.Map<IDictionary<string, object>>(feature);
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public override IEnumerable<NeighborhoodShape> GetFeatureList()
        {
            var features = GetFeatures();

            var results = Mapper.Map<IEnumerable<NeighborhoodShape>>(features).OrderBy(x => x.BoroCode);
            return results;
        }

    }
}
