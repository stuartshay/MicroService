using System;
using System.Collections.Generic;
using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;

namespace MicroService.Service.Services
{
    public class HistoricDistrictService : AbstractShapeService<HistoricDistrictShape>, IShapeService<HistoricDistrictShape>
    {
        public HistoricDistrictService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.HistoricDistricts));
        }

        public override HistoricDistrictShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new HistoricDistrictShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    var borough = f.Attributes["BOROUGH"].ToString();
                    model = new HistoricDistrictShape
                    {
                        LPNumber = f.Attributes["LP_NUMBER"].ToString(),
                        AreaName = f.Attributes["AREA_NAME"].ToString(),
                        BoroName = borough,
                        BoroCode = (int)Enum.Parse(typeof(Borough), borough),
                    };
                }

            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;
        }

        public IEnumerable<HistoricDistrictShape> GetFeatureAttributes()
        {
            var features = GetFeatures();
            var results = new List<HistoricDistrictShape>(features.Count);

            foreach (var f in features)
            {
                var borough = f.Attributes["BOROUGH"].ToString();
                var model = new HistoricDistrictShape
                {
                    LPNumber = f.Attributes["LP_NUMBER"].ToString(),
                    AreaName = f.Attributes["AREA_NAME"].ToString(),
                    BoroName = borough,
                    BoroCode = (int)Enum.Parse(typeof(Borough), borough),
                };

                results.Add(model);
            }

            return results;
        }

    }
}
