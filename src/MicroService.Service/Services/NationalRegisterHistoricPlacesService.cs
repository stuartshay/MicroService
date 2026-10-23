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
    public class NationalRegisterHistoricPlacesService : AbstractShapeService<NationalRegisterHistoricPlacesShape>, IShapeService<NationalRegisterHistoricPlacesShape>
    {

        public NationalRegisterHistoricPlacesService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            ILogger<NationalRegisterHistoricPlacesService> logger)
            : base(logger, NationalRegisterHistoricPlacesShapeProfile.Map)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.IndividualLandmarkSite));
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            var featureCollection = new FeatureCollection();
            var features = GetFeatureLookup(attributes);

            foreach (var feature in features)
            {
                var featureAttributes = NationalRegisterHistoricPlacesShapeProfile.ToDictionary(feature);
                featureAttributes.Add("ShapeColor", nameof(Color.Green).ToLower());
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

    }
}
