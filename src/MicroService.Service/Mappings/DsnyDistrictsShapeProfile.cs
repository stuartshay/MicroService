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
                DistrictCode = int.Parse(src.Attributes["districtco"]?.ToString() ?? string.Empty),
                Fid = src.Attributes["fid"]?.ToString() ?? string.Empty,
                GlobalId = src.Attributes["globalid"]?.ToString() ?? string.Empty,
                OperationZone = (src.Attributes["district"]?.ToString() ?? string.Empty).RemoveIntegers(),
                OperationZoneName = (src.Attributes["district"]?.ToString() ?? string.Empty).RemoveIntegers().ParseEnum<DsnyOperationZone>().GetEnumDescription(),
                ShapeArea = double.Parse(src.Attributes["shape_area"]?.ToString() ?? string.Empty),
                ShapeLength = double.Parse(src.Attributes["shape_leng"]?.ToString() ?? string.Empty),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
