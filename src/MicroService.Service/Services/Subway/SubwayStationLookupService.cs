using MicroService.Service.Models.FlatFileModels;
using MicroService.Service.Models.Subway;
using MicroService.Service.Services.FlatFileService;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MicroService.Service.Services.Subway
{
    /// <summary>
    /// The MTA stations (Stations.csv) near a set of points, and the routes that serve them.
    /// </summary>
    public class SubwayStationLookupService
    {
        public const double DefaultRadiusMetres = 100;

        public const double MinRadiusMetres = 10;

        public const double MaxRadiusMetres = 500;

        public const int MaxPoints = 300;

        // Mean Earth radius
        private const double EarthRadiusMetres = 6_371_008.8;

        private const string CacheKey = "FlatFile:SubwayStationLocations";

        private static readonly TimeSpan Lifetime = TimeSpan.FromHours(3);

        private readonly StationFlatFileService _stations;
        private readonly IMemoryCache _cache;

        public SubwayStationLookupService(StationFlatFileService stations, IMemoryCache cache)
        {
            _stations = stations ?? throw new ArgumentNullException(nameof(stations));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// For each point, the stations within <paramref name="radiusMetres"/>, nearest first, and the
        /// union of their daytime routes.
        /// </summary>
        public SubwayStationsNear FindNear(IReadOnlyList<(double Latitude, double Longitude)> points, double radiusMetres)
        {
            ArgumentNullException.ThrowIfNull(points);

            var stations = GetStations();
            var services = new HashSet<string>(StringComparer.Ordinal);
            var results = new List<SubwayPointStations>(points.Count);

            foreach (var (latitude, longitude) in points)
            {
                var near = stations
                    .Select(s => (Station: s, Distance: Distance(latitude, longitude, s.GTFSLatitude, s.GTFSLongitude)))
                    .Where(s => s.Distance <= radiusMetres)
                    .OrderBy(s => s.Distance)
                    .ThenBy(s => s.Station.StationId)
                    .Select(s => ToStationDistance(s.Station, s.Distance))
                    .ToList();

                foreach (var station in near)
                {
                    services.UnionWith(station.Routes);
                }

                results.Add(new SubwayPointStations(latitude, longitude, near));
            }

            return new SubwayStationsNear(results, services.OrderBy(s => s, SubwayRoutes.Order).ToList());
        }

        /// <summary>
        /// Parses <c>lat,lon;lat,lon</c> (decimal degrees, invariant culture).
        /// </summary>
        /// <returns>False with a message saying what is wrong.</returns>
        public static bool TryParsePoints(string? near, out List<(double Latitude, double Longitude)> points, out string error) =>
            TryParsePoints(near, nameof(near), out points, out error);

        /// <summary>
        /// Parses <c>lat,lon;lat,lon</c> (decimal degrees, invariant culture), with messages that
        /// name the <paramref name="parameter"/> the points came in.
        /// </summary>
        /// <returns>False with a message saying what is wrong.</returns>
        public static bool TryParsePoints(string? value, string parameter, out List<(double Latitude, double Longitude)> points, out string error)
        {
            points = [];
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(value))
            {
                error = $"{parameter} is required: one or more lat,lon points separated by ';', e.g. {parameter}=40.7527,-73.9772;40.7066,-74.0110.";
                return false;
            }

            var parts = value.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > MaxPoints)
            {
                error = $"{parameter} has {parts.Length} points; the most is {MaxPoints}.";
                return false;
            }

            foreach (var part in parts)
            {
                var values = part.Split(',', StringSplitOptions.TrimEntries);
                if (values.Length != 2
                    || !double.TryParse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
                    || !double.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude)
                    || !double.IsFinite(latitude)
                    || !double.IsFinite(longitude))
                {
                    error = $"{parameter} point '{part}' is not lat,lon in decimal degrees.";
                    return false;
                }

                if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
                {
                    error = $"{parameter} point '{part}' is out of range: latitude is -90 to 90 and longitude -180 to 180.";
                    return false;
                }

                points.Add((latitude, longitude));
            }

            if (points.Count == 0)
            {
                error = $"{parameter} is required: one or more lat,lon points separated by ';'.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// The daytime routes of a station, 1-7 then letters.
        /// </summary>
        public static IReadOnlyList<string> ParseRoutes(string? daytimeRoutes) =>
            (daytimeRoutes ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(r => r, SubwayRoutes.Order)
                .ToList();

        /// <summary>
        /// Great-circle (haversine) distance in metres.
        /// </summary>
        public static double Distance(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            var phi1 = double.DegreesToRadians(latitude1);
            var phi2 = double.DegreesToRadians(latitude2);
            var deltaPhi = double.DegreesToRadians(latitude2 - latitude1);
            var deltaLambda = double.DegreesToRadians(longitude2 - longitude1);

            var a = Math.Pow(Math.Sin(deltaPhi / 2), 2)
                + (Math.Cos(phi1) * Math.Cos(phi2) * Math.Pow(Math.Sin(deltaLambda / 2), 2));

            return 2 * EarthRadiusMetres * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        }

        private List<StationFlatFile> GetStations() =>
            _cache.GetOrCreate(CacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Lifetime;
                return _stations.GetAll().Cast<StationFlatFile>().ToList();
            })!;

        private static SubwayStationDistance ToStationDistance(StationFlatFile station, double distance) => new(
            station.StationId,
            station.ComplexId,
            station.GtfsStopId,
            station.StopName,
            station.Line,
            station.Borough,
            ParseRoutes(station.DaytimeRoutes),
            station.Structure,
            station.GTFSLatitude,
            station.GTFSLongitude,
            Math.Round(distance, 1));
    }
}
