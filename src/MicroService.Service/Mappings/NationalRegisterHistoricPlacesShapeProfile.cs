using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;
using System.Globalization;

namespace MicroService.Service.Mappings
{
    public abstract class NationalRegisterHistoricPlacesShapeProfile : ShapeProfile<NationalRegisterHistoricPlacesShape>
    {
        public static NationalRegisterHistoricPlacesShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);

            return new NationalRegisterHistoricPlacesShape
            {
                ResourceName = GetString(src, "resource_n"),
                County = GetString(src, "county"),
                DateDesignated = DateTime.TryParse(GetString(src, "date_natio"), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateDesignated)
                    ? dateDesignated
                    : default,
                TimeDesignated = GetSanitizedString(src, "time_natio") ?? string.Empty,
                SphinxNumber = GetString(src, "sphinx_num"),
                X = ParseDouble(src, "x"),
                Y = ParseDouble(src, "y"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
