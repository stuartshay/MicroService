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
                Population = int.Parse(src.Attributes["POPULATION"]?.ToString() ?? string.Empty),
                State = src.Attributes["STATE"]?.ToString() ?? string.Empty,
                County = src.Attributes["COUNTY"]?.ToString() ?? string.Empty,
                Area = double.Parse(src.Attributes["AREA"]?.ToString() ?? string.Empty),
                StateFibs = src.Attributes["ST_FIPS"]?.ToString() ?? string.Empty,
                CityFibs = src.Attributes["CTY_FIPS"]?.ToString() ?? string.Empty,
                Url = src.Attributes["URL"]?.ToString() ?? string.Empty,
                ShapeArea = double.Parse(src.Attributes["SHAPE_AREA"]?.ToString() ?? string.Empty),
                ShapeLength = double.Parse(src.Attributes["SHAPE_LEN"]?.ToString() ?? string.Empty),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
