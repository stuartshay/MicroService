using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;

namespace MicroService.Service.Mappings
{
    public class NeighborhoodTabulationAreaShapeProfile : ShapeProfile<NeighborhoodTabulationAreaShape>
    {
        protected NeighborhoodTabulationAreaShapeProfile()
        {
        }

        public static NeighborhoodTabulationAreaShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new NeighborhoodTabulationAreaShape
            {
                BoroCode = ParseInt(src, "BoroCode"),
                BoroName = src.Attributes["BoroName"]?.ToString() ?? string.Empty,
                CountyFIPS = src.Attributes["CountyFIPS"]?.ToString() ?? string.Empty,
                NTA2020 = src.Attributes["NTA2020"]?.ToString() ?? string.Empty,
                NTAName = src.Attributes["NTAName"]?.ToString() ?? string.Empty,
                NTAAbbrev = src.Attributes["NTAAbbrev"]?.ToString() ?? string.Empty,
                NTAType = ParseInt(src, "NTAType"),
                CDTA2020 = src.Attributes["CDTA2020"]?.ToString() ?? string.Empty,
                CDTAName = src.Attributes["CDTAName"]?.ToString() ?? string.Empty,
                ShapeArea = ParseDouble(src, "Shape_Area"),
                ShapeLength = ParseDouble(src, "Shape_Leng"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
