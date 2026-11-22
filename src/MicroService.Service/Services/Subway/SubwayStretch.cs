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

        public const double MaxMarginMetres = 2000;

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
            : this(points, radiusMetres, stubMetres, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubwayStretch"/> class that also keeps
        /// <paramref name="marginMetres"/> of line beyond the outermost stations, where the line goes on
        /// (street lines run a little past the first and last place).
        /// </summary>
        /// <param name="points">The stations as WGS84 latitude and longitude, 1 to <see cref="MaxPoints"/>. Their order doesn't matter.</param>
        /// <param name="radiusMetres">How near a line a station must be to count for it, <see cref="MinRadiusMetres"/> to <see cref="MaxRadiusMetres"/>.</param>
        /// <param name="stubMetres">The line kept each side of a lone station, 0 (none) to <see cref="MaxStubMetres"/>.</param>
        /// <param name="marginMetres">The line kept beyond the outermost stations, 0 (none) to <see cref="MaxMarginMetres"/>.</param>
        public SubwayStretch(IReadOnlyCollection<(double Latitude, double Longitude)> points, double radiusMetres, double stubMetres, double marginMetres)
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

            if (!double.IsFinite(marginMetres) || marginMetres < 0 || marginMetres > MaxMarginMetres)
            {
                throw new ArgumentOutOfRangeException(nameof(marginMetres), marginMetres, $"The margin is 0 to {MaxMarginMetres} metres.");
            }

            // Sorted, so that the same stations in another order are the same request
            Points = points.OrderBy(p => p.Latitude).ThenBy(p => p.Longitude).ToList();
            RadiusMetres = radiusMetres;
            StubMetres = stubMetres;
            MarginMetres = marginMetres;
            // A margin changes the key only when there is one, so subway keys stay as they were
            Key = BuildKey(Points, radiusMetres, stubMetres)
                + (marginMetres > 0 ? "|m" + marginMetres.ToString("R", CultureInfo.InvariantCulture) : string.Empty);
        }

        /// <summary>
        /// The stations, sorted by latitude then longitude.
        /// </summary>
        public IReadOnlyList<(double Latitude, double Longitude)> Points { get; }

        public double RadiusMetres { get; }

        public double StubMetres { get; }

        /// <summary>
        /// The line kept beyond the outermost stations, 0 for none.
        /// </summary>
        public double MarginMetres { get; }

        /// <summary>
        /// The same for the same stations (in any order), radius and stub.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The stretch of <paramref name="lines"/> (one service, WGS84) between the stations on it.
        /// </summary>
        /// <remarks>
        /// The lines are merged into parts, which meet at junctions and where the source's pieces
        /// nearly touch (ends within 10 m), and not where a clip cut them. A station within the
        /// radius belongs to the part nearest it. In each set of connected parts, a part keeps the
        /// line between the outermost of its stations, projected onto it, and runs on to an end
        /// that leads to another part with stations, so that a stretch carries on across a
        /// junction; a part without stations is kept only between parts with stations, and a loop
        /// with stations that leads to more is kept whole. Where all
        /// of a set's stations are at one spot, the set keeps the stub each side of it instead,
        /// carried on across joins. Distances are metres on a plane centred on the lines, with
        /// longitude scaled by the cosine of the centre's latitude.
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
            var indexes = localParts.Select(p => new LengthIndexedLine(p)).ToList();
            var positions = Assign(localParts, indexes, Points.Select(p => factory.CreatePoint(frame.ToLocal(p.Longitude, p.Latitude))));
            var graph = new PartGraph(localParts, positions);
            var ranges = new Dictionary<int, (double From, double To)>();

            foreach (var component in graph.Components())
            {
                var withStations = component.Where(p => positions[p].Count > 0).ToList();
                if (withStations.Count == 0)
                {
                    continue;
                }

                if (IsOneSpot(withStations, positions, indexes))
                {
                    AddStub(graph, indexes, withStations[0], positions[withStations[0]][0], ranges);
                    continue;
                }

                var stretches = component
                    .Select(part => (Part: part, Range: StretchRange(graph, indexes[part], positions[part], part, MarginMetres)))
                    .Where(s => s.Range.HasValue);

                foreach (var (part, range) in stretches)
                {
                    ranges[part] = range!.Value;
                }
            }

            var kept = ranges
                .OrderBy(r => r.Key)
                .Select(r => frame.ToWgs84((LineString)indexes[r.Key].ExtractLine(r.Value.From, r.Value.To), factory))
                .ToArray();

            return (factory.CreateMultiLineString(kept), positions.Sum(p => p.Count));
        }

        /// <summary>
        /// Each station within the radius goes to the part nearest it, so that a station where
        /// parts meet counts once. Ties go to the first part.
        /// </summary>
        /// <returns>For each part, its stations' positions along it.</returns>
        private List<List<double>> Assign(List<LineString> parts, List<LengthIndexedLine> indexes, IEnumerable<Point> stations)
        {
            var positions = parts.Select(_ => new List<double>()).ToList();

            foreach (var station in stations)
            {
                var (part, distance) = parts.Select((p, i) => (Part: i, Distance: p.Distance(station))).MinBy(p => p.Distance);
                if (distance <= RadiusMetres)
                {
                    positions[part].Add(indexes[part].Project(station.Coordinate));
                }
            }

            return positions;
        }

        private static bool IsOneSpot(List<int> parts, List<List<double>> positions, List<LengthIndexedLine> indexes)
        {
            var spots = parts.SelectMany(p => positions[p].Select(position => indexes[p].ExtractPoint(position))).ToList();
            return spots.TrueForAll(s => s.Distance(spots[0]) < SameSpotMetres);
        }

        /// <summary>
        /// The part of a line between its stations, running on to an end that leads to more
        /// stations, or null for nothing.
        /// </summary>
        private static (double From, double To)? StretchRange(PartGraph graph, LengthIndexedLine index, List<double> positions, int part, double marginMetres)
        {
            var toStart = graph.HasStationsBeyond(part, fromStart: true);
            var toEnd = graph.HasStationsBeyond(part, fromStart: false);

            if (graph.IsLoop(part))
            {
                // A loop with stations is kept whole when it leads to more, and is otherwise
                // between its first and last (more than one spot, or it would be a stub)
                return positions.Count == 0 ? null
                    : toStart ? (index.StartIndex, index.EndIndex)
                    : (positions.Min(), positions.Max());
            }

            if (positions.Count == 0)
            {
                return toStart && toEnd ? (index.StartIndex, index.EndIndex) : null;
            }

            var from = toStart ? index.StartIndex : Math.Max(index.StartIndex, positions.Min() - marginMetres);
            var to = toEnd ? index.EndIndex : Math.Min(index.EndIndex, positions.Max() + marginMetres);

            // Stations at the end where the part joins the stretch add nothing
            return to - from < SameSpotMetres ? null : (from, to);
        }

        /// <summary>
        /// The stub each side of <paramref name="spot"/> on <paramref name="part"/>, carried on
        /// into the parts that join it when the part ends first.
        /// </summary>
        private void AddStub(PartGraph graph, List<LengthIndexedLine> indexes, int part, double spot, Dictionary<int, (double From, double To)> ranges)
        {
            if (StubMetres <= 0)
            {
                return;
            }

            var index = indexes[part];
            ranges[part] = (Math.Max(index.StartIndex, spot - StubMetres), Math.Min(index.EndIndex, spot + StubMetres));

            if (graph.IsLoop(part))
            {
                return;
            }

            var visited = new HashSet<int> { part };
            CarryOn(graph, indexes, graph.Start(part), StubMetres - (spot - index.StartIndex), visited, ranges);
            CarryOn(graph, indexes, graph.End(part), StubMetres - (index.EndIndex - spot), visited, ranges);
        }

        private static void CarryOn(PartGraph graph, List<LengthIndexedLine> indexes, int end, double metres, HashSet<int> visited, Dictionary<int, (double From, double To)> ranges)
        {
            if (metres <= 0)
            {
                return;
            }

            foreach (var next in graph.PartsAt(end).Where(visited.Add).ToList())
            {
                var index = indexes[next];
                var length = index.EndIndex - index.StartIndex;

                if (graph.Start(next) == end)
                {
                    ranges[next] = (index.StartIndex, Math.Min(index.EndIndex, index.StartIndex + metres));
                    CarryOn(graph, indexes, graph.End(next), metres - length, visited, ranges);
                }
                else
                {
                    ranges[next] = (Math.Max(index.StartIndex, index.EndIndex - metres), index.EndIndex);
                    CarryOn(graph, indexes, graph.Start(next), metres - length, visited, ranges);
                }
            }
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
            private readonly IReadOnlyList<List<double>> _stations;
            private readonly List<Coordinate> _ends = [];
            private readonly List<List<int>> _partsAtEnd = [];
            private readonly int[] _start;
            private readonly int[] _end;

            public PartGraph(IReadOnlyList<LineString> parts, IReadOnlyList<List<double>> stations)
            {
                _stations = stations;
                _start = new int[parts.Count];
                _end = new int[parts.Count];

                for (var i = 0; i < parts.Count; i++)
                {
                    _start[i] = AddEnd(parts[i].StartPoint.Coordinate, i);
                    _end[i] = AddEnd(parts[i].EndPoint.Coordinate, i);
                }
            }

            public int Start(int part) => _start[part];

            public int End(int part) => _end[part];

            public IReadOnlyList<int> PartsAt(int end) => _partsAtEnd[end];

            /// <summary>
            /// A part whose ends are one.
            /// </summary>
            public bool IsLoop(int part) => _start[part] == _end[part];

            /// <summary>
            /// The sets of connected parts.
            /// </summary>
            public IEnumerable<List<int>> Components()
            {
                var seen = new bool[_start.Length];

                for (var first = 0; first < _start.Length; first++)
                {
                    if (seen[first])
                    {
                        continue;
                    }

                    seen[first] = true;
                    var component = new List<int> { first };
                    for (var i = 0; i < component.Count; i++)
                    {
                        var part = component[i];
                        foreach (var next in _partsAtEnd[_start[part]].Concat(_partsAtEnd[_end[part]]).Where(p => !seen[p]))
                        {
                            seen[next] = true;
                            component.Add(next);
                        }
                    }

                    yield return component;
                }
            }

            /// <summary>
            /// Whether a part with stations can be reached from the start (or end) of part
            /// <paramref name="part"/> without going along that part.
            /// </summary>
            public bool HasStationsBeyond(int part, bool fromStart)
            {
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

                        if (_stations[next].Count > 0)
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
