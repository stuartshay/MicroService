using AutoMapper;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using MicroService.Service.Services.Base;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using System.Collections.Generic;

namespace MicroService.Service.Services
{
    public class IndividualLandmarkHistoricDistrictsService : AbstractShapeService<IndividualLandmarkHistoricDistrictsShape,
            IndividualLandmarkHistoricDistrictsShapeProfile>, IShapeService<IndividualLandmarkHistoricDistrictsShape>
    {
        public IndividualLandmarkHistoricDistrictsService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<IndividualLandmarkSiteService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.IndividualLandmarkHistoricDistricts));
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                // Map attributes
                var featureAttributes = Mapper.Map<IDictionary<string, object>>(feature);
                var transformedGeometry = GeoTransformationHelper.TransformGeometry(feature.Geometry, false);


                // Add feature to collection
                featureCollection.Add(new Feature(transformedGeometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }
    }
}
