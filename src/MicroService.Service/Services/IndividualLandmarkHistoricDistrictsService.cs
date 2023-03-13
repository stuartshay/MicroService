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
using System.Collections.Generic;

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

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var propertyAttributes = new List<KeyValuePair<string, object>>
            {
               new KeyValuePair<string, object>("LPNumber","LP-00001")
            };
            var properties = _individualLandmarkSiteService.GetFeatureCollection(propertyAttributes);

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
    }
}
