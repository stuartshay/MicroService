﻿using AutoMapper;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services
{
    public class ZipCodeService : AbstractShapeService<ZipCodeShape>, IShapeService<ZipCodeShape>
    {
        public ZipCodeService(ShapefileDataReaderResolver shapefileDataReaderResolver,
            IMapper mapper,
            ILogger<SubwayService> logger)
            : base(logger, mapper)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.ZipCodes));
        }

        public virtual ZipCodeShape GetFeatureLookup(double x, double y)
        {
            var point = new Point(x, y);

            var features = GetFeatures();
            var feature = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (feature == null)
            {
                return null;
            }

            return new ZipCodeShape
            {
                ZipCode = feature.Attributes["ZIPCODE"].ToString(),
                BldgZip = feature.Attributes["BLDGZIP"].ToString(),
                PostOfficeName = feature.Attributes["PO_NAME"].ToString(),
                Population = int.Parse(feature.Attributes["POPULATION"].ToString()),
                Area = double.Parse(feature.Attributes["AREA"].ToString()),
                State = feature.Attributes["STATE"].ToString(),
                County = feature.Attributes["COUNTY"].ToString(),
                StateFibs = feature.Attributes["ST_FIPS"].ToString(),
                CityFibs = feature.Attributes["CTY_FIPS"].ToString(),
                Url = feature.Attributes["URL"].ToString(),
                ShapeArea = double.Parse(feature.Attributes["SHAPE_AREA"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["SHAPE_LEN"].ToString()),
            };
        }

        public IEnumerable<ZipCodeShape> GetFeatureLookup(List<KeyValuePair<string, object>> attributes)
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
                          select new ZipCodeShape
                          {
                              ZipCode = f.Attributes["ZIPCODE"].ToString(),
                              BldgZip = f.Attributes["BLDGZIP"].ToString(),
                              PostOfficeName = f.Attributes["PO_NAME"].ToString(),
                              Population = int.Parse(f.Attributes["POPULATION"].ToString()),
                              Area = double.Parse(f.Attributes["AREA"].ToString()),
                              State = f.Attributes["STATE"].ToString(),
                              County = f.Attributes["COUNTY"].ToString(),
                              StateFibs = f.Attributes["ST_FIPS"].ToString(),
                              CityFibs = f.Attributes["CTY_FIPS"].ToString(),
                              Url = f.Attributes["URL"].ToString(),
                              ShapeArea = double.Parse(f.Attributes["SHAPE_AREA"].ToString()),
                              ShapeLength = double.Parse(f.Attributes["SHAPE_LEN"].ToString()),
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
                .Select(f => new ZipCodeShape
                {
                    ZipCode = f.Attributes["ZIPCODE"].ToString(),
                    BldgZip = f.Attributes["BLDGZIP"].ToString(),
                    PostOfficeName = f.Attributes["PO_NAME"].ToString(),
                    Population = int.Parse(f.Attributes["POPULATION"].ToString()),
                    Area = double.Parse(f.Attributes["AREA"].ToString()),
                    State = f.Attributes["STATE"].ToString(),
                    County = f.Attributes["COUNTY"].ToString(),
                    StateFibs = f.Attributes["ST_FIPS"].ToString(),
                    CityFibs = f.Attributes["CTY_FIPS"].ToString(),
                    Url = f.Attributes["URL"].ToString(),
                    ShapeArea = double.Parse(f.Attributes["SHAPE_AREA"].ToString()),
                    ShapeLength = double.Parse(f.Attributes["SHAPE_LEN"].ToString()),
                    Geometry = f.Geometry,
                });

            foreach (var feature in features)
            {
                var featureProperties = EnumHelper.GetPropertiesWithoutExcludedAttribute<ZipCodeShape, FeatureCollectionExcludeAttribute>();
                var featureAttributes = featureProperties
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(feature, null));

                featureCollection.Add(new Feature(feature.Geometry, new AttributesTable(featureAttributes)));
            }

            return featureCollection;
        }

        public IEnumerable<ZipCodeShape> GetFeatureList()
        {
            var features = GetFeatures();

            return features.Select(f => new ZipCodeShape
            {
                ZipCode = f.Attributes["ZIPCODE"].ToString(),
                BldgZip = f.Attributes["BLDGZIP"].ToString(),
                PostOfficeName = f.Attributes["PO_NAME"].ToString(),
                Population = int.Parse(f.Attributes["POPULATION"].ToString()),
                State = f.Attributes["STATE"].ToString(),
                County = f.Attributes["COUNTY"].ToString(),
                StateFibs = f.Attributes["ST_FIPS"].ToString(),
                CityFibs = f.Attributes["CTY_FIPS"].ToString(),
                Url = f.Attributes["URL"].ToString(),
                ShapeArea = double.Parse(f.Attributes["SHAPE_AREA"].ToString()),
                ShapeLength = double.Parse(f.Attributes["SHAPE_LEN"].ToString()),
            });
        }

    }
}