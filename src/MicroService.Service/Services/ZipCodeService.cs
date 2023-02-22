﻿using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace MicroService.Service.Services
{
    public class ZipCodeService<T> : AbstractShapeService<ZipCodeShape>, IShapeService<ZipCodeShape>
    {
        public ZipCodeService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.ZipCodes));
        }

        public override ZipCodeShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new ZipCodeShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    model = new ZipCodeShape
                    {
                        ZipCode = f.Attributes["ZIPCODE"].ToString(),
                        BldgZip = f.Attributes["BLDGZIP"].ToString(),
                        PostOfficeName = f.Attributes["PO_NAME"].ToString(),
                        Population = int.Parse(f.Attributes["POPULATION"].ToString()),
                        Area = double.Parse(f.Attributes["AREA"].ToString()),
                        State = f.Attributes["STATE"].ToString(),
                        County = f.Attributes["COUNTY"].ToString(),
                    };
                }
            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;
        }

        public override IEnumerable<ZipCodeShape> GetFeatureLookup(List<KeyValuePair<string, string>> features)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ZipCodeShape> GetFeatureAttributes()
        {
            var features = GetFeatures();
            var results = new List<ZipCodeShape>(features.Count);

            foreach (var f in features)
            {
                var model = new ZipCodeShape
                {
                    ZipCode = f.Attributes["ZIPCODE"].ToString(),
                    BldgZip = f.Attributes["BLDGZIP"].ToString(),
                    PostOfficeName = f.Attributes["PO_NAME"].ToString(),
                    Population = int.Parse(f.Attributes["POPULATION"].ToString()),
                    Area = double.Parse(f.Attributes["AREA"].ToString()),
                    State = f.Attributes["STATE"].ToString(),
                    County = f.Attributes["COUNTY"].ToString(),
                };

                results.Add(model);
            }

            return results;
        }
    }
}
