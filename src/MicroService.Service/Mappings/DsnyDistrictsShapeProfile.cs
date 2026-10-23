using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class DsnyDistrictsShapeProfile : ShapeProfile<DsnyDistrictsShape>
    {
        protected DsnyDistrictsShapeProfile()
        {
        }

        public static DsnyDistrictsShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new DsnyDistrictsShape
            {
                District = src.Attributes["district"]?.ToString() ?? string.Empty,
                DistrictCode = ParseInt(src, "districtco"),
                Fid = src.Attributes["fid"]?.ToString() ?? string.Empty,
                GlobalId = src.Attributes["globalid"]?.ToString() ?? string.Empty,
                OperationZone = (src.Attributes["district"]?.ToString() ?? string.Empty).RemoveIntegers(),
                OperationZoneName = (src.Attributes["district"]?.ToString() ?? string.Empty).RemoveIntegers().ParseEnum<DsnyOperationZone>().GetEnumDescription(),
                ShapeArea = ParseDouble(src, "shape_area"),
                ShapeLength = ParseDouble(src, "shape_leng"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
