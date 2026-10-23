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
    public class CommunityDistrictsService : AbstractShapeService<CommunityDistrictShape>, IShapeService<CommunityDistrictShape>
    {
        public CommunityDistrictsService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            ILogger<CommunityDistrictsService> logger)
            : base(logger, CommunityDistrictShapeProfile.Map)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.CommunityDistricts));
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                var featureAttributes = CommunityDistrictShapeProfile.ToDictionary(feature);
                featureAttributes.Add("ShapeColor", Color.Blue.ToString().ToLower());
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public override IEnumerable<CommunityDistrictShape> GetFeatureList()
        {
            var features = GetFeatures();

            var results = features.Select(MapFeature).OrderBy(x => x.Cd);
            return results;
        }

    }
}
