using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class ZipCodeMappingsProfile : ShapeProfile<ZipCodeShape>
    {
        protected ZipCodeMappingsProfile()
        {
        }

        public static ZipCodeShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new ZipCodeShape
            {
                ZipCode = src.Attributes["ZIPCODE"]?.ToString() ?? string.Empty,
                BldgZip = src.Attributes["BLDGZIP"]?.ToString() ?? string.Empty,
                PostOfficeName = src.Attributes["PO_NAME"]?.ToString() ?? string.Empty,
                Population = ParseInt(src, "POPULATION"),
                State = src.Attributes["STATE"]?.ToString() ?? string.Empty,
                County = src.Attributes["COUNTY"]?.ToString() ?? string.Empty,
                Area = ParseDouble(src, "AREA"),
                StateFibs = src.Attributes["ST_FIPS"]?.ToString() ?? string.Empty,
                CityFibs = src.Attributes["CTY_FIPS"]?.ToString() ?? string.Empty,
                Url = src.Attributes["URL"]?.ToString() ?? string.Empty,
                ShapeArea = ParseDouble(src, "SHAPE_AREA"),
                ShapeLength = ParseDouble(src, "SHAPE_LEN"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
