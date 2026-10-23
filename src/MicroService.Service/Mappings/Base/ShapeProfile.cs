using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum.Attributes;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MicroService.Service.Mappings.Base
{
    internal static class ShapePropertyCache
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Cache = new();

        public static PropertyInfo[] GetIncludedProperties(Type type) =>
            Cache.GetOrAdd(
                type,
                static t => t.GetProperties()
                    .Where(prop => prop.GetCustomAttributes(typeof(FeatureCollectionExcludeAttribute), false).Length == 0)
                    .ToArray());
    }

    public abstract partial class ShapeProfile<TShape> where TShape : class
    {
        protected ShapeProfile()
        {
        }

        /// <summary>
        /// Projects all public properties of <paramref name="shape"/> except those marked with
        /// <see cref="FeatureCollectionExcludeAttribute"/> into a mutable dictionary keyed by property name.
        /// </summary>
        public static IDictionary<string, object> ToDictionary(TShape shape)
        {
            ArgumentNullException.ThrowIfNull(shape);

            // No shape model exposes a nullable-valued property, so GetValue(shape) is
            // never actually null here; PropertyInfo.GetValue's object? return is a
            // general reflection contract, not a real possibility for this closed set of types.
            return ShapePropertyCache.GetIncludedProperties(shape.GetType())
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(shape)!);
        }

        /// <summary>
        /// Maps a NetTopologySuite <see cref="Envelope"/> to a <see cref="BoundingBox"/>.
        /// </summary>
        protected static BoundingBox MapBoundingBox(Envelope? envelope)
        {
            if (envelope is null)
            {
                return null!;
            }

            return new BoundingBox
            {
                Area = envelope.Area,
                Centre = envelope.Centre is null
                    ? null!
                    : new CentrePoint
                    {
                        X = envelope.Centre.X,
                        Y = envelope.Centre.Y,
                    },
                Diameter = envelope.Diameter,
                MinX = envelope.MinX,
                MaxX = envelope.MaxX,
                MinY = envelope.MinY,
                MaxY = envelope.MaxY,
                MinExtent = envelope.MinExtent,
                MaxExtent = envelope.MaxExtent,
            };
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
        protected static double ParseDouble(Feature src, string key) => double.Parse(GetString(src, key), CultureInfo.InvariantCulture);

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
