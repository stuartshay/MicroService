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
using System.Linq;

namespace MicroService.Service.Services
{
    public class BoroughBoundariesService : AbstractShapeService<BoroughBoundaryShape, BoroughBoundaryShapeProfile>, IShapeService<BoroughBoundaryShape>
    {
        public BoroughBoundariesService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<BoroughBoundariesService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.BoroughBoundaries));
            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }


        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                var featureAttributes = Mapper.Map<IDictionary<string, object>>(feature);
                featureAttributes.Add("ShapeColor", Color.Green.ToString().ToLower());

                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public override IEnumerable<BoroughBoundaryShape> GetFeatureList()
        {
            var features = GetFeatures();
            Logger.LogInformation("FeatureCount {FeatureCount}", features.Count);

            var results = Mapper.Map<IEnumerable<BoroughBoundaryShape>>(features).OrderBy(x => x.BoroCode);
            return results;
        }
    }
}
