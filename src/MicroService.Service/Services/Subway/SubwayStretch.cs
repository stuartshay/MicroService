using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Operation.Linemerge;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MicroService.Service.Services.Subway
{
    /// <summary>
    /// A map's stations, which trim each service line to the stretch between them: a station is on
    /// a line when it lies within <see cref="RadiusMetres"/> of it, and each line keeps only the part
    /// from its first to its last station. A line with one station keeps a stub of
    /// <see cref="StubMetres"/> each side of it.
    /// </summary>
    public sealed class SubwayStretch
    {
        public const double DefaultRadiusMetres = 150;

        public const double MinRadiusMetres = 10;

        public const double MaxRadiusMetres = 500;

        public const double DefaultStubMetres = 400;

        public const double MaxStubMetres = 2000;

        public const int MaxPoints = SubwayStationLookupService.MaxPoints;

        // Stations closer than this along a line are one spot, which keeps a stub
        private const double SameSpotMetres = 1;

        // Ends of two parts closer than this meet: the source's pieces miss each other by up to about 7 m
        private const double JoinMetres = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubwayStretch"/> class.
        /// </summary>
        /// <param name="points">The stations as WGS84 latitude and longitude, 1 to <see cref="MaxPoints"/>. Their order doesn't matter.</param>
        /// <param name="radiusMetres">How near a line a station must be to count for it, <see cref="MinRadiusMetres"/> to <see cref="MaxRadiusMetres"/>.</param>
        /// <param name="stubMetres">The line kept each side of a lone station, 0 (none) to <see cref="MaxStubMetres"/>.</param>
        public SubwayStretch(IReadOnlyCollection<(double Latitude, double Longitude)> points, double radiusMetres, double stubMetres)
        {
            ArgumentNullException.ThrowIfNull(points);
            ArgumentOutOfRangeException.ThrowIfZero(points.Count, nameof(points));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(points.Count, MaxPoints, nameof(points));
            if (!double.IsFinite(radiusMetres) || radiusMetres < MinRadiusMetres || radiusMetres > MaxRadiusMetres)
            {
                throw new ArgumentOutOfRangeException(nameof(radiusMetres), radiusMetres, $"The radius is {MinRadiusMetres} to {MaxRadiusMetres} metres.");
            }

            if (!double.IsFinite(stubMetres) || stubMetres < 0 || stubMetres > MaxStubMetres)
            {
                throw new ArgumentOutOfRangeException(nameof(stubMetres), stubMetres, $"The stub is 0 to {MaxStubMetres} metres.");
            }

            // Sorted, so that the same stations in another order are the same request
            Points = points.OrderBy(p => p.Latitude).ThenBy(p => p.Longitude).ToList();
            RadiusMetres = radiusMetres;
            StubMetres = stubMetres;
            Key = BuildKey(Points, radiusMetres, stubMetres);
        }

        /// <summary>
        /// The stations, sorted by latitude then longitude.
        /// </summary>
        public IReadOnlyList<(double Latitude, double Longitude)> Points { get; }

        public double RadiusMetres { get; }

        public double StubMetres { get; }

        /// <summary>
        /// The same for the same stations (in any order), radius and stub.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The stretch of <paramref name="lines"/> (one service, WGS84) between the stations on it.
        /// </summary>
        /// <remarks>
        /// The lines are merged into parts, which meet at junctions and where the source's pieces
        /// nearly touch (ends within 10 m), or not at all where a clip cut them. Each part keeps the line between the outermost of its stations, projected onto it, and
        /// runs on to an end where it joins a part that has stations of its own, so that a stretch
        /// carries on across a junction. A part whose stations are all at one spot and that joins
        /// no part with stations keeps the stub; a part without stations is kept only where it
        /// joins parts with stations at both ends, and is otherwise dropped. Distances are
        /// metres on a plane centred on the lines (latitude scaled by the cosine at the centre).
        /// </remarks>
        /// <returns>The kept lines and how many of the stations lie within the radius of the lines.</returns>
        public (MultiLineString Lines, int Stations) Trim(Geometry lines)
        {
            ArgumentNullException.ThrowIfNull(lines);

            var factory = lines.Factory;
            var merger = new LineMerger();
            merger.Add(lines);
            var parts = merger.GetMergedLineStrings().OfType<LineString>().ToList();
            if (parts.Count == 0)
            {
                return (factory.CreateMultiLineString(), 0);
            }

            var frame = new LocalFrame(lines.EnvelopeInternal.Centre);
            var localParts = parts.Select(p => frame.ToLocal(p, factory)).ToList();
            var localPoints = Points.Select(p => factory.CreatePoint(frame.ToLocal(p.Longitude, p.Latitude))).ToList();

            // The stations within the radius of each part
            var near = localParts
                .Select(part => Enumerable.Range(0, localPoints.Count).Where(i => part.IsWithinDistance(localPoints[i], RadiusMetres)).ToList())
                .ToList();
            var stations = near.SelectMany(n => n).Distinct().Count();

            var graph = new PartGraph(localParts, near);
            var kept = new List<LineString>();

            for (var i = 0; i < localParts.Count; i++)
            {
                var part = localParts[i];
                var index = new LengthIndexedLine(part);
                var toStart = graph.HasStationsBeyond(i, fromStart: true);
                var toEnd = graph.HasStationsBeyond(i, fromStart: false);

                if (near[i].Count == 0)
                {
                    // Kept whole only between two parts with stations
                    if (toStart && toEnd)
                    {
                        kept.Add(frame.ToWgs84(part, factory));
                    }

                    continue;
                }

                var positions = near[i].Select(p => index.Project(localPoints[p].Coordinate)).ToList();
                var from = toStart ? index.StartIndex : positions.Min();
                var to = toEnd ? index.EndIndex : positions.Max();

                if (to - from < SameSpotMetres)
                {
                    // A station where the part joins another with stations is on that one's stretch
                    if (toStart || toEnd || StubMetres <= 0)
                    {
                        continue;
                    }

                    var spot = (from + to) / 2;
                    from = Math.Max(index.StartIndex, spot - StubMetres);
                    to = Math.Min(index.EndIndex, spot + StubMetres);
                }

                if (index.ExtractLine(from, to) is LineString piece && piece.Length > 0)
                {
                    kept.Add(frame.ToWgs84(piece, factory));
                }
            }

            return (factory.CreateMultiLineString(kept.ToArray()), stations);
        }

        private static string BuildKey(IReadOnlyList<(double Latitude, double Longitude)> points, double radiusMetres, double stubMetres)
        {
            var text = new StringBuilder();
            foreach (var (latitude, longitude) in points)
            {
                text.Append(latitude.ToString("R", CultureInfo.InvariantCulture))
                    .Append(',')
                    .Append(longitude.ToString("R", CultureInfo.InvariantCulture))
                    .Append(';');
            }

            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString())));
            return string.Join('|',
                hash,
                radiusMetres.ToString("R", CultureInfo.InvariantCulture),
                stubMetres.ToString("R", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// A plane in metres around a centre: x east, y north. Over the city, the scale of a degree of
        /// longitude changes by less than 1%.
        /// </summary>
        private sealed class LocalFrame
        {
            private readonly double _longitude;
            private readonly double _latitude;
            private readonly double _metresPerDegreeLongitude;

            public LocalFrame(Coordinate centre)
            {
                _longitude = centre.X;
                _latitude = centre.Y;
                _metresPerDegreeLongitude = SubwayRouteService.MetresPerDegree * Math.Cos(double.DegreesToRadians(centre.Y));
            }

            public Coordinate ToLocal(double longitude, double latitude) => new(
                (longitude - _longitude) * _metresPerDegreeLongitude,
                (latitude - _latitude) * SubwayRouteService.MetresPerDegree);

            public LineString ToLocal(LineString line, GeometryFactory factory) =>
                factory.CreateLineString(line.Coordinates.Select(c => ToLocal(c.X, c.Y)).ToArray());

            public LineString ToWgs84(LineString line, GeometryFactory factory) =>
                factory.CreateLineString(line.Coordinates.Select(c => new Coordinate(
                    (c.X / _metresPerDegreeLongitude) + _longitude,
                    (c.Y / SubwayRouteService.MetresPerDegree) + _latitude)).ToArray());
        }

        /// <summary>
        /// The parts of a service and the ends they share. The source draws a service in pieces whose
        /// ends can miss each other by a few metres, so ends within <see cref="JoinMetres"/> are one.
        /// </summary>
        private sealed class PartGraph
        {
            private readonly IReadOnlyList<List<int>> _near;
            private readonly List<Coordinate> _ends = [];
            private readonly List<List<int>> _partsAtEnd = [];
            private readonly int[] _start;
            private readonly int[] _end;

            public PartGraph(IReadOnlyList<LineString> parts, IReadOnlyList<List<int>> near)
            {
                _near = near;
                _start = new int[parts.Count];
                _end = new int[parts.Count];

                for (var i = 0; i < parts.Count; i++)
                {
                    _start[i] = AddEnd(parts[i].StartPoint.Coordinate, i);
                    _end[i] = AddEnd(parts[i].EndPoint.Coordinate, i);
                }
            }

            /// <summary>
            /// Whether a part with stations can be reached from the start (or end) of part
            /// <paramref name="part"/> without going along that part. A loop, whose ends are one,
            /// is never carried on from.
            /// </summary>
            public bool HasStationsBeyond(int part, bool fromStart)
            {
                if (_start[part] == _end[part])
                {
                    return false;
                }

                var visited = new HashSet<int> { part };
                var ends = new Queue<int>();
                ends.Enqueue(fromStart ? _start[part] : _end[part]);

                while (ends.Count > 0)
                {
                    foreach (var next in _partsAtEnd[ends.Dequeue()])
                    {
                        if (!visited.Add(next))
                        {
                            continue;
                        }

                        if (_near[next].Count > 0)
                        {
                            return true;
                        }

                        ends.Enqueue(_start[next]);
                        ends.Enqueue(_end[next]);
                    }
                }

                return false;
            }

            private int AddEnd(Coordinate coordinate, int part)
            {
                var end = _ends.FindIndex(e => e.Distance(coordinate) <= JoinMetres);
                if (end < 0)
                {
                    end = _ends.Count;
                    _ends.Add(coordinate);
                    _partsAtEnd.Add([]);
                }

                if (!_partsAtEnd[end].Contains(part))
                {
                    _partsAtEnd[end].Add(part);
                }

                return end;
            }
        }
    }
}
