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
    public class IndividualLandmarkHistoricDistrictsService : AbstractShapeService<IndividualLandmarkHistoricDistrictsShape>, IShapeService<IndividualLandmarkHistoricDistrictsShape>
    {
        private readonly IndividualLandmarkSiteService _individualLandmarkSiteService;

        public IndividualLandmarkHistoricDistrictsService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IndividualLandmarkSiteService individualLandmarkSiteService,
            ILogger<IndividualLandmarkHistoricDistrictsService> logger)
            : base(logger, IndividualLandmarkHistoricDistrictsShapeProfile.Map)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.IndividualLandmarkHistoricDistricts));
            _individualLandmarkSiteService = individualLandmarkSiteService;
        }

        public override IEnumerable<IndividualLandmarkHistoricDistrictsShape> GetFeatureLookup(List<KeyValuePair<string, object>>? attributes)
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
                var featureAttributes = IndividualLandmarkHistoricDistrictsShapeProfile.ToDictionary(feature);
                featureAttributes.Add("ShapeColor", Color.Blue.ToString().ToLower());

                // The building database geometry is already WGS84 (see its .prj), so no transform is needed
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        private List<KeyValuePair<string, object>>? MapLandmarkAttributes(List<KeyValuePair<string, object>>? attributes)
        {
            if (attributes is null)
            {
                return null;
            }

            if (attributes.Any(a => a.Key.Equals("LPNumber", StringComparison.InvariantCultureIgnoreCase)))
            {
                var lpNumberValue = attributes.First(a => a.Key.Equals("LPNumber", StringComparison.InvariantCultureIgnoreCase)).Value.ToString() ?? string.Empty;
                var propertyAttributes = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("LPNumber", lpNumberValue)
                };
                var properties = _individualLandmarkSiteService.GetFeatureCollection(propertyAttributes);
                if (!properties.Any())
                {
                    // Falls through to null when no matching individual landmark site is found;
                    // Historic Districts lookup by LPNumber is not yet implemented (2026-08-05). NOSONAR
                    return null;
                }

                var bbl = properties[0].Attributes["BBL"];

                attributes.Clear();

                // Create a new Attribute with the BBL
                attributes.Add(new KeyValuePair<string, object>("Bbl", bbl));
            }

            return attributes;
        }

    }
}
