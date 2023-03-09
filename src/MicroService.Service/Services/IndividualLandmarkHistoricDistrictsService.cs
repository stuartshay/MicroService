using AutoMapper;
using MicroService.Service.Interfaces;
using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using MicroService.Service.Services.Base;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System.Collections.Generic;

namespace MicroService.Service.Services
{
    public class IndividualLandmarkHistoricDistrictsService : AbstractShapeService<IndividualLandmarkHistoricDistrictsShape,
            IndividualLandmarkHistoricDistrictsShapeProfile>, IShapeService<IndividualLandmarkHistoricDistrictsShape>
    {
        const string Epsg2263EsriWkt = "PROJCS[\"NAD83 / New York Long Island (ftUS)\",GEOGCS[\"GCS_North_American_1983\",DATUM[\"D_North_American_1983\",SPHEROID[\"GRS_1980\",6378137,298.257222101]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Lambert_Conformal_Conic\"],PARAMETER[\"standard_parallel_1\",41.03333333333333],PARAMETER[\"standard_parallel_2\",40.66666666666666],PARAMETER[\"latitude_of_origin\",40.16666666666666],PARAMETER[\"central_meridian\",-74],PARAMETER[\"false_easting\",984250.0000000002],PARAMETER[\"false_northing\",0],UNIT[\"Foot_US\",0.30480060960121924]]";



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

            // Define coordinate systems
            var csWgs84 = GeographicCoordinateSystem.WGS84;
            var csFact = new CoordinateSystemFactory();
            var utmNad83 = csFact.CreateFromWkt(Epsg2263EsriWkt);

            var ctFactory = new CoordinateTransformationFactory();
            var trans = ctFactory.CreateFromCoordinateSystems(utmNad83, csWgs84);


            foreach (var feature in features)
            {
                ////// Convert geometry to WGS84
                //var geometry = feature.Geometry;
                //var wgs84Geom = geometry.Copy();
                //wgs84Geom.Apply(new MathTransformFilter(trans.MathTransform));




                // Map attributes
                var featureAttributes = Mapper.Map<IDictionary<string, object>>(feature);

                // Add feature to collection
                //featureCollection.Add(new Feature(wgs84Geom, new AttributesTable(featureAttributes)));
                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }
    }
}
