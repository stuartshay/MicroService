﻿using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

namespace MicroService.Service.Services
{
    public class NeighborhoodsService : AbstractShapeService<NeighborhoodShape>, IShapeService<NeighborhoodShape>
    {
        public NeighborhoodsService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.Neighborhoods));
        }

        public override NeighborhoodShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var feature = GetFeatures().FirstOrDefault(f => f.Geometry.Contains(point));
            if (feature == null)
            {
                return null;
            }

            return new NeighborhoodShape
            {
                BoroCode = int.Parse(feature.Attributes["BoroCode"].ToString()),
                BoroName = feature.Attributes["BoroName"].ToString(),
                CountyFIPS = feature.Attributes["CountyFIPS"].ToString(),
                NTACode = feature.Attributes["NTACode"].ToString(),
                NTAName = feature.Attributes["NTAName"].ToString(),
                ShapeArea = double.Parse(feature.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(feature.Attributes["Shape_Leng"].ToString()),
                Coordinates = new List<Coordinate>(),
            };
        }


        public override IEnumerable<NeighborhoodShape> GetFeatureLookup(List<KeyValuePair<string, string>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<NeighborhoodShape> GetFeatureAttributes()
        {
            var features = GetFeatures();

            return features.Select(f => new NeighborhoodShape
            {
                BoroCode = int.Parse(f.Attributes["BoroCode"].ToString()),
                BoroName = f.Attributes["BoroName"].ToString(),
                CountyFIPS = f.Attributes["CountyFIPS"].ToString(),
                NTACode = f.Attributes["NTACode"].ToString(),
                NTAName = f.Attributes["NTAName"].ToString(),
                ShapeArea = double.Parse(f.Attributes["Shape_Area"].ToString()),
                ShapeLength = double.Parse(f.Attributes["Shape_Leng"].ToString()),
            }).OrderBy(x => x.BoroCode);
        }

    }
}
