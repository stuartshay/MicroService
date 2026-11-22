using MicroService.Data.Enum;
using MicroService.Service.Services.Subway;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO.Converters;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Simplify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MicroService.Service.Services.Streets
{
    /// <summary>
    /// Street lines for a map: each named street's centerline in a borough, merged into lines and,
    /// with the map's places, trimmed to the stretch along them (<see cref="SubwayStretch"/> with a margin).
    /// </summary>
    public class StreetLineService
    {
        public const double DefaultSimplifyMetres = 2;

        public const double MaxSimplifyMetres = 50;

        /// <summary>
        /// The most streets one request draws.
        /// </summary>
        public const int MaxStreets = 10;

        private const double MetresPerDegree = 111_320;

        private const int CoordinateDecimals = 6;

        private static readonly JsonSerializerOptions GeoJsonOptions = new()
        {
            Converters = { new GeoJsonConverterFactory() },
        };

        private readonly StreetCenterlineClient _centerline;

        public StreetLineService(StreetCenterlineClient centerline)
        {
            _centerline = centerline ?? throw new ArgumentNullException(nameof(centerline));
        }

        /// <summary>
        /// One MultiLineString feature per street that has a line, with the properties name, borough and,
        /// with a stretch, places (how many of the places are along it).
        /// </summary>
        public async Task<string> GetLinesGeoJsonAsync(
            IReadOnlyList<string> names,
            Borough borough,
            double simplifyMetres,
            SubwayStretch? stretch,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(names);
            ArgumentOutOfRangeException.ThrowIfNegative(simplifyMetres);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(simplifyMetres, MaxSimplifyMetres);

            var tolerance = simplifyMetres / MetresPerDegree;
            var collection = new FeatureCollection();

            foreach (var name in names)
            {
                var segments = await _centerline.GetSegmentsAsync(name, borough, cancellationToken).ConfigureAwait(false);
                if (segments.Count == 0)
                {
                    continue;
                }

                var factory = segments[0].Factory;
                Geometry geometry = Merge(segments, factory);
                var places = 0;
                if (stretch is not null)
                {
                    // Trim at full detail, then simplify what is left
                    (geometry, places) = stretch.Trim(geometry);
                }

                geometry = tolerance > 0 ? DouglasPeuckerSimplifier.Simplify(geometry, tolerance) : geometry;
                var rounded = Round(geometry, factory);
                if (rounded.IsEmpty)
                {
                    continue;
                }

                var attributes = new AttributesTable
                {
                    { "name", name },
                    { "borough", borough.ToString() },
                };

                if (stretch is not null)
                {
                    attributes.Add("places", places);
                }

                collection.Add(new Feature(rounded, attributes));
            }

            return JsonSerializer.Serialize(collection, GeoJsonOptions);
        }

        /// <summary>
        /// The segments merged into as few lines as possible, so a stretch runs along the street.
        /// </summary>
        private static MultiLineString Merge(IEnumerable<Geometry> segments, GeometryFactory factory)
        {
            var merger = new LineMerger();
            foreach (var segment in segments)
            {
                merger.Add(segment);
            }

            return factory.CreateMultiLineString(merger.GetMergedLineStrings().OfType<LineString>().ToArray());
        }

        private static MultiLineString Round(Geometry geometry, GeometryFactory factory)
        {
            var lines = new List<LineString>();
            foreach (var line in LineStringExtracter.GetLines(geometry).OfType<LineString>())
            {
                var coordinates = new List<Coordinate>(line.NumPoints);
                foreach (var coordinate in line.Coordinates)
                {
                    var rounded = new Coordinate(Math.Round(coordinate.X, CoordinateDecimals), Math.Round(coordinate.Y, CoordinateDecimals));
                    if (coordinates.Count == 0 || !coordinates[^1].Equals2D(rounded))
                    {
                        coordinates.Add(rounded);
                    }
                }

                if (coordinates.Count >= 2)
                {
                    lines.Add(factory.CreateLineString(coordinates.ToArray()));
                }
            }

            return factory.CreateMultiLineString(lines.ToArray());
        }
    }
}
