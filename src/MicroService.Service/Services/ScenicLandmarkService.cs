using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class ScenicLandmarkService : AbstractShapeService<ScenicLandmarkShape>, IShapeService<ScenicLandmarkShape>
    {
        public ScenicLandmarkService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.ScenicLandmarks));
        }

        public override ScenicLandmarkShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new ScenicLandmarkShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    var borough = f.Attributes["BOROUGH"].ToString();
                    model = new ScenicLandmarkShape
                    {
                        LPNumber = f.Attributes["lp_number"].ToString(),
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

        public override IEnumerable<ScenicLandmarkShape> GetFeatureLookup(List<KeyValuePair<string, string>> attributes)
        {
            var list = new List<ScenicLandmarkShape>();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var found = true;
                foreach (var pair in attributes)
                {
                    if (f.Attributes[pair.Key] as string != pair.Value)
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    var borough = f.Attributes["borough"].ToString();
                    var model = new ScenicLandmarkShape
                    {
                        LPNumber = f.Attributes["lp_number"].ToString(),
                        AreaName = f.Attributes["scen_lm_na"].ToString(),
                        BoroName = borough,
                        BoroCode = (int)Enum.Parse(typeof(Borough), borough),
                        ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                        ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
                    };
                    list.Add(model);
                }
            }

            return list;
        }

        public IEnumerable<ScenicLandmarkShape> GetFeatureAttributes()
        {
            var features = GetFeatures();

            return features.Select(f => new ScenicLandmarkShape
            {
                LPNumber = f.Attributes["lp_number"].ToString(),
                AreaName = f.Attributes["scen_lm_na"].ToString(),
                BoroName = f.Attributes["borough"].ToString(),
                BoroCode = (int)Enum.Parse(typeof(Borough), f.Attributes["borough"].ToString()),
                ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
            });
        }
    }
}
