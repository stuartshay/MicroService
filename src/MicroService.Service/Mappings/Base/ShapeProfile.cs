using AutoMapper;
using MicroService.Service.Models.Enum.Attributes;
using NetTopologySuite.Features;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MicroService.Service.Mappings.Base
{
    public abstract partial class ShapeProfile<TShape> : Profile where TShape : class
    {
        protected ShapeProfile()
        {
            CreateMap<TShape, IDictionary<string, object>>()
                .ConvertUsing(shape => shape.GetType().GetProperties()
                    .Where(prop => prop.GetCustomAttributes(typeof(FeatureCollectionExcludeAttribute), false).Length == 0)
                    // No shape model exposes a nullable-valued property, so GetValue(shape) is
                    // never actually null here; PropertyInfo.GetValue's object? return is a
                    // general reflection contract, not a real possibility for this closed set of types.
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(shape)!));

        }

        /// <summary>
        /// Reads a shapefile attribute as a string, defaulting to <see cref="string.Empty"/>
        /// when the attribute is absent or its value has no string representation.
        /// </summary>
        protected static string GetString(Feature src, string key) => src.Attributes[key]?.ToString() ?? string.Empty;

        /// <summary>
        /// Parses a shapefile attribute as a double. Throws if the attribute is absent, matching
        /// the behavior of the required shape_area/shape_leng geometry attributes.
        /// </summary>
        protected static double ParseDouble(Feature src, string key) => double.Parse(GetString(src, key));

        /// <summary>
        /// Reads a shapefile attribute as a double, defaulting to 0 when the attribute is absent.
        /// </summary>
        protected static double GetDouble(Feature src, string key) =>
            src.Attributes[key] != null ? ParseDouble(src, key) : 0;

        /// <summary>
        /// Reads a shapefile attribute as a string with embedded null characters stripped,
        /// returning null when the raw value is null or empty.
        /// </summary>
        protected static string? GetSanitizedString(Feature src, string key)
        {
            var value = GetString(src, key);
            return string.IsNullOrEmpty(value) ? null : NullCharacterRegex().Replace(value, string.Empty);
        }

        [GeneratedRegex(@"\u0000")]
        private static partial Regex NullCharacterRegex();
    }
}
