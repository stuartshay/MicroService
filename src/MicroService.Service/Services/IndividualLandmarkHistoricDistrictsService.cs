using AutoMapper;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using MicroService.Service.Services.Base;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;

namespace MicroService.Service.Services
{
    public class IndividualLandmarkHistoricDistrictsService : AbstractShapeService<IndividualLandmarkHistoricDistrictsShape,
            IndividualLandmarkHistoricDistrictsShapeProfile>, IShapeService<IndividualLandmarkHistoricDistrictsShape>
    {
        private readonly IndividualLandmarkSiteService _individualLandmarkSiteService;

        public IndividualLandmarkHistoricDistrictsService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IndividualLandmarkSiteService individualLandmarkSiteService,
            IMapper mapper,
            ILogger<IndividualLandmarkSiteService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.IndividualLandmarkHistoricDistricts));
            _individualLandmarkSiteService = individualLandmarkSiteService;
        }

        public override IEnumerable<IndividualLandmarkHistoricDistrictsShape>? GetFeatureLookup(List<KeyValuePair<string, object>>? attributes)
        {
            attributes = MapLandmarkAttributes(attributes);
            return base.GetFeatureLookup(attributes);
        }

        public FeatureCollection? GetFeatureCollection(List<KeyValuePair<string, object>>? attributes)
        {
            attributes = MapLandmarkAttributes(attributes);
            if (attributes == null)
            {
                return null;
            }

            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                // Map attributes
                var featureAttributes = Mapper.Map<IDictionary<string, object>>(feature);
                featureAttributes.Add("ShapeColor", Color.Blue.ToString().ToLower());

                // Transform geometry
                var transformedGeometry = GeoTransformationHelper.TransformGeometry(feature.Geometry, Datum.Nad83, Datum.Wgs84);

                // Add feature to collection
                featureCollection.Add(new Feature(transformedGeometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        private List<KeyValuePair<string, object>>? MapLandmarkAttributes(List<KeyValuePair<string, object>>? attributes)
        {
            if (attributes.Any(a => a.Key.Equals("LPNumber", StringComparison.InvariantCultureIgnoreCase)))
            {
                var lpNumberValue = attributes.First(a => a.Key.Equals("LPNumber", StringComparison.InvariantCultureIgnoreCase)).Value.ToString();
                var propertyAttributes = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("LPNumber", lpNumberValue)
                };
                var properties = _individualLandmarkSiteService.GetFeatureCollection(propertyAttributes);
                if (!properties.Any())
                {
                    //TODO: Check for Historic Districts
                    return null;
                }

                var bbl = properties.First().Attributes["BBL"];

                attributes.Clear();

                // Create a new Attribute with the BBL
                attributes.Add(new KeyValuePair<string, object>("Bbl", bbl));
            }

            return attributes;
        }

    }
}
