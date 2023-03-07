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
    public class NationalRegisterHistoricPlacesService : AbstractShapeService<NationalRegisterHistoricPlacesShape, FeatureToNationalRegisterHistoricPlacesShapeProfile>, IShapeService<NationalRegisterHistoricPlacesShape>
    {

        public NationalRegisterHistoricPlacesService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<NationalRegisterHistoricPlacesService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.IndividualLandmarkSite));
        }

        public NationalRegisterHistoricPlacesShape GetFeatureLookup(double x, double y)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NationalRegisterHistoricPlacesShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NationalRegisterHistoricPlacesShape> GetFeatureList()
        {
            var features = GetFeatures();
            return Mapper.Map<IEnumerable<NationalRegisterHistoricPlacesShape>>(features).Take(100);
        }
    }
}
