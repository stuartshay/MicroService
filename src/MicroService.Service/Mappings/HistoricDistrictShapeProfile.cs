using MicroService.Data.Enum;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class HistoricDistrictShapeProfile : ShapeProfile<HistoricDistrictShape>
    {
        protected HistoricDistrictShapeProfile()
        {
        }

        public static HistoricDistrictShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new HistoricDistrictShape
            {
                LPNumber = GetString(src, "lp_number"),
                AreaName = GetString(src, "area_name"),
                BoroName = GetString(src, "borough"),
                BoundaryName = GetSanitizedString(src, "boundary_n")!,
                CalendarDate = GetSanitizedString(src, "caldate")!,
                Current = GetString(src, "current_"),
                DesignationDate = GetSanitizedString(src, "desdate")!,
                Extension = GetSanitizedString(src, "extension")!,
                LastAction = GetString(src, "last_actio"),
                OtherHearing = GetSanitizedString(src, "other_hear")!,
                PublicHearing = GetString(src, "public_hea"),
                Status = GetString(src, "status_of_"),
                BoroCode = (int)Enum.Parse<Borough>(GetString(src, "borough")),
                ShapeArea = ParseDouble(src, "shape_area"),
                ShapeLength = ParseDouble(src, "shape_leng"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
