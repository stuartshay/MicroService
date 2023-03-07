using AutoMapper;
using MicroService.Service.Interfaces;
using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class CommunityDistrictsService : AbstractShapeService<CommunityDistrictShape, FeatureToCommunityDistrictShapeProfile>, IShapeService<CommunityDistrictShape>
    {
        public CommunityDistrictsService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<CommunityDistrictsService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.CommunityDistricts));
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

        public override IEnumerable<CommunityDistrictShape> GetFeatureList()
        {
            var features = GetFeatures();

            var results = Mapper.Map<IEnumerable<CommunityDistrictShape>>(features).OrderBy(x => x.Cd);
            return results;
        }

    }
}
