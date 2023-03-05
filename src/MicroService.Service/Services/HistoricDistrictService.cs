﻿using AutoMapper;
using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class HistoricDistrictService : AbstractShapeService<HistoricDistrictShape>, IShapeService<HistoricDistrictShape>
    {
        public HistoricDistrictService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<HistoricDistrictService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.HistoricDistricts));
        }

        public virtual HistoricDistrictShape GetFeatureLookup(double x, double y)
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

                AreaName = feature.Attributes["AREA_NAME"].ToString(),
                BoroName = feature.Attributes["BOROUGH"].ToString(),
                BoroCode = (int)Enum.Parse(typeof(Borough), feature.Attributes["BOROUGH"].ToString()),
                ShapeArea = double.Parse(feature.Attributes["Shape_area"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["Shape_len"].ToString()),
            };
        }

        public IEnumerable<HistoricDistrictShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
        {
            attributes = ValidateFeatureKey(attributes);

            var results = from f in GetFeatures()
                          where attributes.All(pair =>
                          {
                              var value = f.Attributes[pair.Key];
                              var expectedValue = pair.Value;
                              var matchedValue = MatchAttributeValue(value, expectedValue);
                              return matchedValue != null;
                          })
                          select new HistoricDistrictShape
                          {
                              LPNumber = f.Attributes["lp_number"].ToString(),
                              AreaName = f.Attributes["area_name"].ToString(),
                              BoroName = f.Attributes["borough"].ToString(),
                              BoroCode = (int)Enum.Parse(typeof(Borough), f.Attributes["borough"].ToString()),
                              ShapeArea = double.Parse(f.Attributes["shape_area"].ToString()),
                              ShapeLength = double.Parse(f.Attributes["shape_leng"].ToString()),
                          };

            return results;
        }

        public FeatureCollection GetFeatureCollection(List<KeyValuePair<string, object>> attributes)
        {
            attributes = ValidateFeatureKey(attributes);
            var featureCollection = new FeatureCollection();

            var features = GetFeatures()
                .Where(f => attributes.All(pair =>
                {
                    var value = f.Attributes[pair.Key];
                    var expectedValue = pair.Value;
                    var matchedValue = MatchAttributeValue(value, expectedValue);
                    return matchedValue != null;
                }))
                .Select(f => new HistoricDistrictShape
                {
                    LPNumber = f.Attributes["LP_NUMBER"].ToString(),
                    AreaName = f.Attributes["AREA_NAME"].ToString(),
                    BoroName = f.Attributes["BOROUGH"].ToString(),
                    BoroCode = (int)Enum.Parse(typeof(Borough), f.Attributes["BOROUGH"].ToString()),
                    ShapeArea = double.Parse(f.Attributes["Shape_area"].ToString()),
                    ShapeLength = double.Parse(f.Attributes["Shape_len"].ToString()),
                    Geometry = f.Geometry,
                });

            foreach (var feature in features)
            {
                var featureProperties = EnumHelper.GetPropertiesWithoutExcludedAttribute<HistoricDistrictShape, FeatureCollectionExcludeAttribute>();
                var featureAttributes = featureProperties
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(feature, null));

                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public IEnumerable<HistoricDistrictShape> GetFeatureList()
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
