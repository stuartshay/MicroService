using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace MicroService.Service.Services
{
    public class IndividualLandmarkSiteService : AbstractShapeService<IndividualLandmarkSiteShape>, IShapeService<IndividualLandmarkSiteShape>
    {
        public IndividualLandmarkSiteService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.IndividualLandmarkSite));
        }

        public IEnumerable<IndividualLandmarkSiteShape> GetFeatureAttributes()
        {
            var features = GetFeatures();
            var results = new List<IndividualLandmarkSiteShape>(features.Count);

            foreach (var f in features)
            {
                var borough = f.Attributes["borough"].ToString();
                var model = new IndividualLandmarkSiteShape
                {
                    //LPNumber = f.Attributes["LP_NUMBER"].ToString(),
                    //AreaName = f.Attributes["AREA_NAME"].ToString(),
                    BoroName = borough,
                    BoroCode = (int)Enum.Parse(typeof(Borough), borough),
                };

                results.Add(model);
            }

            return results;
        }

        public override IndividualLandmarkSiteShape GetFeatureLookup(double x, double y)
        {
            // Convert Nad83 to Wgs 
            var result = GeoTransformationHelper.ConvertNad83ToWgs84(x, y);
            var wgs84Point = new { X = result.Item1, Y = result.Item2 };

            // Validate Point is in Range
            var point = new Point(wgs84Point.X.Value, wgs84Point.Y.Value);

            var model = new IndividualLandmarkSiteShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    var borough = f.Attributes["borough"].ToString();
                    model = new IndividualLandmarkSiteShape
                    {
                        LPNumber = f.Attributes["lpc_name"].ToString(),
                        AreaName = f.Attributes["scen_lm_na"].ToString(),
                        BoroName = borough,
                        BoroCode = (int)Enum.Parse(typeof(Borough), borough),
                        ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                        ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
                    };
                }
            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;
        }

        public override IEnumerable<IndividualLandmarkSiteShape> GetFeatureLookup(List<KeyValuePair<string, string>> features)
        {
            throw new System.NotImplementedException();
        }
    }
}
