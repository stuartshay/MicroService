using MicroService.Data.Enum;
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
            var point = new Point(x, y);

            var features = GetFeatures();
            var feature = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return new HistoricDistrictShape
            {
                LPNumber = feature.Attributes["LP_NUMBER"].ToString(),
                AreaName = feature.Attributes["AREA_NAME"].ToString(),
                BoroName = feature.Attributes["BOROUGH"].ToString(),
                BoroCode = (int)Enum.Parse(typeof(Borough), feature.Attributes["BOROUGH"].ToString()),
                ShapeArea = double.Parse(feature.Attributes["Shape_area"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["Shape_len"].ToString()),
                Coordinates = new List<Coordinate>(),
            };
        }

        public override IEnumerable<HistoricDistrictShape> GetFeatureLookup(List<KeyValuePair<string, string>> attributes)
        {
            // Get Shape Feature Names
            for (int i = 0; i < attributes.Count; i++)
            {
                var key = attributes[i].Key;
                var featureName = GetFeatureName(key);
                attributes[i] = new KeyValuePair<string, string>(featureName, attributes[i].Value);
            }

            var results = from f in GetFeatures()
                          where attributes.All(pair => f.Attributes[pair.Key] as string == pair.Value)
                          select new HistoricDistrictShape
                          {
                              LPNumber = f.Attributes["LP_NUMBER"].ToString(),
                              AreaName = f.Attributes["AREA_NAME"].ToString(),
                              BoroName = f.Attributes["BOROUGH"].ToString(),
                              BoroCode = (int)Enum.Parse(typeof(Borough), f.Attributes["BOROUGH"].ToString()),
                              ShapeArea = double.Parse(f.Attributes["Shape_area"].ToString()),
                              ShapeLength = double.Parse(f.Attributes["Shape_len"].ToString()),
                              Coordinates = new List<Coordinate>(),
                          };

            return results;
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
