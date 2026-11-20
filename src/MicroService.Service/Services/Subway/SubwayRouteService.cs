using MicroService.Data.Enum;
using MicroService.Service.Helpers;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using MicroService.Service.Models.Subway;
using MicroService.Service.Services.Base;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO.Converters;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Operation.OverlayNG;
using NetTopologySuite.Simplify;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace MicroService.Service.Services.Subway
{
    /// <summary>
    /// The subway services and their lines, shaped for a map: one feature per service, optionally
    /// clipped to a borough and simplified, in WGS84 with coordinates rounded to 6 decimals (about 0.1 m).
    /// </summary>
    public class SubwayRouteService
    {
        public const double DefaultSimplifyMetres = 5;

        public const double MaxSimplifyMetres = 50;

        // Metres in a degree of latitude. A degree of longitude at New York's latitude (40.7° N)
        // is about 84,400 m, so a tolerance in degrees from this scale never removes a detail
        // larger than the requested metres in either direction.
        internal const double MetresPerDegree = 111_320;

        private const int CoordinateDecimals = 6;

        // As long as the shapes stay in the shared cache (CachedShapefileDataReader)
        private static readonly TimeSpan Lifetime = TimeSpan.FromHours(3);

        private static readonly JsonSerializerOptions GeoJsonOptions = new()
        {
            Converters = { new GeoJsonConverterFactory() },
        };

        private readonly SubwayServiceLinesService _serviceLines;
        private readonly BoroughBoundariesService _boroughBoundaries;
        private readonly MemoryCache _cache;

        public SubwayRouteService(
            SubwayServiceLinesService serviceLines,
            BoroughBoundariesService boroughBoundaries,
            SubwayLineCache cache)
        {
            _serviceLines = serviceLines ?? throw new ArgumentNullException(nameof(serviceLines));
            _boroughBoundaries = boroughBoundaries ?? throw new ArgumentNullException(nameof(boroughBoundaries));
            ArgumentNullException.ThrowIfNull(cache);
            _cache = cache.Cache;
        }

        /// <summary>
        /// Every service in the service lines, 1-7 then letters, with its colors.
        /// </summary>
        public IReadOnlyList<SubwayRoute> GetServices() =>
            _cache.GetOrCreate("services", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Lifetime;
                entry.Size = 4096;

                return (IReadOnlyList<SubwayRoute>)GetServiceFeatures()
                    .GroupBy(f => f.Service, StringComparer.Ordinal)
                    .Select(g => new SubwayRoute(g.Key, g.First().Name, SubwayRoutes.GetColor(g.Key), SubwayRoutes.GetTextColor(g.Key)))
                    .OrderBy(r => r.Service, SubwayRoutes.Order)
                    .ToList();
            })!;

        /// <summary>
        /// The service codes the request names, as the service lines write them. Codes match
        /// regardless of case, and S, which station lists use for every shuttle, means SF, SR and
        /// ST. No codes means every service.
        /// </summary>
        /// <returns>The services in route order, and the codes that match none.</returns>
        public (IReadOnlyList<string> Services, IReadOnlyList<string> Unknown) ResolveServices(IEnumerable<string>? codes)
        {
            var known = GetServices().ToDictionary(r => r.Service, r => r.Service, StringComparer.OrdinalIgnoreCase);
            var requested = codes?.Select(c => c.Trim()).Where(c => c.Length > 0).ToList() ?? [];

            if (requested.Count == 0)
            {
                return (known.Values.OrderBy(s => s, SubwayRoutes.Order).ToList(), []);
            }

            var services = new HashSet<string>(StringComparer.Ordinal);
            var unknown = new List<string>();

            foreach (var code in requested)
            {
                if (known.TryGetValue(code, out var service))
                {
                    services.Add(service);
                }
                else if (string.Equals(code, SubwayRoutes.Shuttle, StringComparison.OrdinalIgnoreCase))
                {
                    services.UnionWith(SubwayRoutes.Shuttles.Where(known.ContainsKey));
                }
                else if (!unknown.Contains(code, StringComparer.OrdinalIgnoreCase))
                {
                    unknown.Add(code);
                }
            }

            return (services.OrderBy(s => s, SubwayRoutes.Order).ToList(), unknown);
        }

        /// <summary>
        /// A GeoJSON FeatureCollection with one MultiLineString feature per service (properties
        /// service, name, color and textColor), cached per request.
        /// </summary>
        /// <param name="services">Service codes from <see cref="ResolveServices"/>.</param>
        /// <param name="borough">Clip every line to this borough's outline; a service with nothing left is left out.</param>
        /// <param name="simplifyMetres">Douglas-Peucker tolerance in metres, 0 for none.</param>
        public string GetLinesGeoJson(IReadOnlyCollection<string> services, Borough? borough, double simplifyMetres) =>
            GetLinesGeoJson(services, borough, simplifyMetres, null);

        /// <summary>
        /// A GeoJSON FeatureCollection with one MultiLineString feature per service (properties
        /// service, name, color and textColor), cached per request. With a <paramref name="stretch"/>,
        /// each line is trimmed to the stretch between its stations after clipping and before
        /// simplifying, a service with nothing left is left out, and each feature also has
        /// stations, how many of the stretch's stations are on it.
        /// </summary>
        /// <param name="services">Service codes from <see cref="ResolveServices"/>.</param>
        /// <param name="borough">Clip every line to this borough's outline; a service with nothing left is left out.</param>
        /// <param name="simplifyMetres">Douglas-Peucker tolerance in metres, 0 for none.</param>
        /// <param name="stretch">The map's stations to trim the lines to, or null for whole lines.</param>
        public string GetLinesGeoJson(IReadOnlyCollection<string> services, Borough? borough, double simplifyMetres, SubwayStretch? stretch)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentOutOfRangeException.ThrowIfNegative(simplifyMetres);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(simplifyMetres, MaxSimplifyMetres);

            var parts = new List<string>
            {
                "lines",
                string.Join(',', services),
                borough?.ToString() ?? "all",
                simplifyMetres.ToString("R", CultureInfo.InvariantCulture),
            };

            if (stretch is not null)
            {
                parts.Add("stretch");
                parts.Add(stretch.Key);
            }

            var key = string.Join('|', parts);

            return _cache.GetOrCreate(key, entry =>
            {
                var json = JsonSerializer.Serialize(BuildLines(services, borough, simplifyMetres, stretch), GeoJsonOptions);

                entry.AbsoluteExpirationRelativeToNow = Lifetime;
                entry.Size = json.Length * sizeof(char);
                return json;
            })!;
        }

        private FeatureCollection BuildLines(IReadOnlyCollection<string> services, Borough? borough, double simplifyMetres, SubwayStretch? stretch)
        {
            var routes = GetServices().ToDictionary(r => r.Service, StringComparer.Ordinal);
            var lines = GetServiceFeatures().ToLookup(f => f.Service, StringComparer.Ordinal);
            var outline = borough is null ? null : GetBoroughOutline(borough.Value);
            var tolerance = simplifyMetres / MetresPerDegree;

            var collection = new FeatureCollection();

            foreach (var service in services)
            {
                if (!routes.TryGetValue(service, out var route))
                {
                    continue;
                }

                var factory = lines[service].First().Geometry.Factory;
                Geometry geometry = MergeLines(lines[service].Select(l => l.Geometry), factory);

                var stations = 0;
                if (stretch is null)
                {
                    geometry = Simplify(geometry, tolerance);
                    geometry = Clip(geometry, outline);
                }
                else
                {
                    // Trim the clipped line at full detail, then simplify what is left
                    (geometry, stations) = stretch.Trim(Clip(geometry, outline));
                    geometry = Simplify(geometry, tolerance);
                }

                var rounded = RoundLines(geometry, factory);
                if (rounded.IsEmpty)
                {
                    continue;
                }

                var attributes = new AttributesTable
                {
                    { "service", route.Service },
                    { "name", route.Name },
                    { "color", route.Color },
                    { "textColor", route.TextColor },
                };

                if (stretch is not null)
                {
                    attributes.Add("stations", stations);
                }

                collection.Add(new Feature(rounded, attributes));
            }

            return collection;
        }

        private static Geometry Simplify(Geometry geometry, double tolerance) =>
            tolerance > 0 ? DouglasPeuckerSimplifier.Simplify(geometry, tolerance) : geometry;

        private static Geometry Clip(Geometry geometry, Geometry? outline) =>
            outline is null ? geometry : OverlayNGRobust.Overlay(geometry, outline, OverlayNG.INTERSECTION);

        private IEnumerable<(string Service, string Name, Geometry Geometry)> GetServiceFeatures() =>
            _serviceLines.GetFeatures().Select(f => (
                Convert.ToString(f.Attributes["service"], CultureInfo.InvariantCulture)?.Trim() ?? string.Empty,
                Convert.ToString(f.Attributes["service_na"], CultureInfo.InvariantCulture)?.Trim() ?? string.Empty,
                f.Geometry));

        /// <summary>
        /// The borough's outline in WGS84. Borough Boundaries is NAD83 State Plane, so it is converted.
        /// </summary>
        private Geometry GetBoroughOutline(Borough borough) =>
            _cache.GetOrCreate($"borough|{borough}", entry =>
            {
                var feature = _boroughBoundaries.GetFeatures().Single(f =>
                    Convert.ToInt32(f.Attributes["BoroCode"], CultureInfo.InvariantCulture) == (int)borough);

                var datum = ShapeRegistry.GetByKey(ShapeProperties.BoroughBoundaries).Datum;
                var outline = datum == Datum.Nad83
                    ? GeoTransformationHelper.CopyNad83ToWgs84(feature.Geometry)
                    : feature.Geometry.Copy();

                entry.AbsoluteExpirationRelativeToNow = Lifetime;
                entry.Size = outline.NumPoints * 40L;
                return outline;
            })!;

        /// <summary>
        /// Joins a service's pieces where they meet end to end, which leaves fewer, longer lines
        /// to simplify and fewer repeated end points.
        /// </summary>
        private static MultiLineString MergeLines(IEnumerable<Geometry> geometries, GeometryFactory factory)
        {
            var merger = new LineMerger();
            foreach (var geometry in geometries)
            {
                merger.Add(geometry);
            }

            return factory.CreateMultiLineString(merger.GetMergedLineStrings().OfType<LineString>().ToArray());
        }

        /// <summary>
        /// The lines of <paramref name="geometry"/> (a clip can also return points where a line
        /// touches the outline) with coordinates rounded, repeated points removed and lines that
        /// shrink to a point dropped.
        /// </summary>
        private static MultiLineString RoundLines(Geometry geometry, GeometryFactory factory)
        {
            var lines = new List<LineString>();

            foreach (var line in LineStringExtracter.GetLines(geometry).OfType<LineString>())
            {
                var coordinates = new List<Coordinate>(line.NumPoints);
                foreach (var coordinate in line.Coordinates)
                {
                    var rounded = new Coordinate(
                        Math.Round(coordinate.X, CoordinateDecimals),
                        Math.Round(coordinate.Y, CoordinateDecimals));

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
