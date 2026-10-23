using MicroService.Data.Enum;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class FeatureToScenicLandmarkShapeProfile : ShapeProfile<ScenicLandmarkShape>
    {
        protected FeatureToScenicLandmarkShapeProfile()
        {
        }

        public static ScenicLandmarkShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new ScenicLandmarkShape
            {
                LPNumber = src.Attributes["lp_number"]?.ToString() ?? string.Empty,
                AreaName = src.Attributes["scen_lm_na"]?.ToString() ?? string.Empty,
                BoroName = src.Attributes["borough"]?.ToString() ?? string.Empty,
                BoroCode = (int)Enum.Parse<Borough>(src.Attributes["borough"]?.ToString() ?? string.Empty),
                ShapeArea = ParseDouble(src, "shape_area"),
                ShapeLength = ParseDouble(src, "shape_leng"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
