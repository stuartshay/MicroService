using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

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
                        ShapeArea = double.Parse(f.Attributes["Shape_area"].ToString()),
                        ShapeLength = double.Parse(f.Attributes["Shape_len"].ToString()),
                        Coordinates = new List<Coordinate>(),
                    };
                }

            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;
        }

        public override IEnumerable<HistoricDistrictShape> GetFeatureLookup(List<KeyValuePair<string, string>> attributes)
        {
            var list = new List<HistoricDistrictShape>();

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
                    var borough = f.Attributes["BOROUGH"].ToString();
                    var coordinates = new List<Coordinate>();
                    foreach (var c in f.Geometry.Coordinates)
                    {
                        coordinates.Add(new Coordinate
                        {
                            X = c.X,
                            Y = c.Y,
                        });
                    }

                    var model = new HistoricDistrictShape
                    {
                        LPNumber = f.Attributes["LP_NUMBER"].ToString(),
                        AreaName = f.Attributes["AREA_NAME"].ToString(),
                        BoroName = borough,
                        BoroCode = (int)Enum.Parse(typeof(Borough), borough),
                        ShapeArea = double.Parse(f.Attributes["Shape_area"].ToString()),
                        ShapeLength = double.Parse(f.Attributes["Shape_len"].ToString()),
                        Coordinates = coordinates,
                    };
                    list.Add(model);
                }
            }

            return list;
        }

        public IEnumerable<HistoricDistrictShape> GetFeatureAttributes()
        {
            var features = GetFeatures();

            return features.Select(f => new HistoricDistrictShape
            {
                LPNumber = f.Attributes["LP_NUMBER"].ToString(),
                AreaName = f.Attributes["AREA_NAME"].ToString(),
                BoroName = f.Attributes["BOROUGH"].ToString(),
                BoroCode = (int)Enum.Parse(typeof(Borough), f.Attributes["BOROUGH"].ToString()),
                ShapeArea = double.Parse(f.Attributes["Shape_area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["Shape_len"].ToString()),
            });
        }

    }
}
