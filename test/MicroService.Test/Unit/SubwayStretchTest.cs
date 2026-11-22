using MicroService.Service.Services.Subway;
using NetTopologySuite.Geometries;
using Xunit;

namespace MicroService.Test.Unit
{
    /// <summary>
    /// Trimming made-up lines near Times Square, laid out in metres east and north of it.
    /// </summary>
    public class SubwayStretchTest
    {
        private const double Latitude = 40.75;
        private const double Longitude = -73.98;
        private const double MetresPerDegree = 111_320;

        private static readonly double MetresPerDegreeLongitude = MetresPerDegree * Math.Cos(double.DegreesToRadians(Latitude));

        private static readonly GeometryFactory Factory = new();

        [Fact(DisplayName = "Subway stretch - a line keeps the part between its first and last station")]
        public void Trim_KeepsTheLineBetweenTheStations()
        {
            var (lines, stations) = Stretch((0, 3000), (0, 1000)).Trim(Line((0, 0), (0, 5000)));

            Assert.Equal(2, stations);
            Assert.Equal(2000, Metres(lines), 0);
            Assert.Equal(1000, North(lines.Coordinates.Min(c => c.Y)), 0);
            Assert.Equal(3000, North(lines.Coordinates.Max(c => c.Y)), 0);
        }

        [Fact(DisplayName = "Subway stretch - a station counts within the radius of the line and not beyond it")]
        public void Trim_CountsStationsWithinTheRadius()
        {
            var line = Line((0, 0), (0, 5000));
            var stations = new[] { At(0, 1000), At(200, 3000) };

            var (narrow, narrowCount) = new SubwayStretch(stations, 150, 0).Trim(line);
            Assert.Equal(1, narrowCount);
            Assert.True(narrow.IsEmpty);

            var (wide, wideCount) = new SubwayStretch(stations, 250, 0).Trim(line);
            Assert.Equal(2, wideCount);
            Assert.Equal(2000, Metres(wide), 0);
        }

        [Fact(DisplayName = "Subway stretch - a lone station keeps a stub each side, clamped to the line's ends")]
        public void Trim_KeepsAStubAroundALoneStation()
        {
            var line = Line((0, 0), (0, 5000));

            Assert.Equal(800, Metres(Stretch((0, 2500)).Trim(line).Lines), 0);
            Assert.Equal(500, Metres(Stretch((0, 100)).Trim(line).Lines), 0);
            Assert.Equal(1200, Metres(new SubwayStretch([At(0, 2500)], 150, 600).Trim(line).Lines), 0);

            // Stations less than a metre apart along the line are one spot
            Assert.Equal(800, Metres(Stretch((0, 2500), (100, 2500.4)).Trim(line).Lines), 0);
        }

        [Fact(DisplayName = "Subway stretch - a margin keeps that much line beyond the first and last station, clamped to the line's ends")]
        public void Trim_KeepsTheMarginBeyondTheStations()
        {
            var line = Line((0, 0), (0, 5000));
            var stations = new[] { At(0, 1000), At(0, 3000) };

            var (lines, count) = new SubwayStretch(stations, 150, 400, 500).Trim(line);
            Assert.Equal(2, count);
            Assert.Equal(3000, Metres(lines), 0);
            Assert.Equal(500, North(lines.Coordinates.Min(c => c.Y)), 0);
            Assert.Equal(3500, North(lines.Coordinates.Max(c => c.Y)), 0);

            // Beyond the start the line stops there
            Assert.Equal(4500, Metres(new SubwayStretch(stations, 150, 400, 1500).Trim(line).Lines), 0);
        }

        [Fact(DisplayName = "Subway stretch - a margin that runs past a junction carries on into the parts that join there")]
        public void Trim_CarriesTheMarginAcrossAJunction()
        {
            // A part ending at a junction at 1000 m, where two parts carry on
            var lines = Lines(
                [(0, 0), (0, 1000)],
                [(0, 1000), (0, 2000)],
                [(0, 1000), (1000, 1000)]);

            var (kept, stations) = new SubwayStretch([At(0, 800), At(0, 950)], 150, 400, 200).Trim(lines);

            // 600-1000 on the first part, then the 150 m left on each part at the junction
            Assert.Equal(2, stations);
            Assert.Equal(700, Metres(kept), 0);
            Assert.Equal(1150, North(kept.Coordinates.Max(c => c.Y)), 0);
            Assert.Equal(150, East(kept.Coordinates.Max(c => c.X)), 0);

            // Without a margin (subway), the stretch ends at the last station as before
            Assert.Equal(150, Metres(new SubwayStretch([At(0, 800), At(0, 950)], 150, 400).Trim(lines).Lines), 0);
        }

        [Fact(DisplayName = "Subway stretch - a margin is 0 to 2000 metres and is part of the key")]
        public void Margin_IsCheckedAndKeyed()
        {
            var stations = new[] { At(0, 1000) };
            Assert.Throws<ArgumentOutOfRangeException>(() => new SubwayStretch(stations, 150, 400, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SubwayStretch(stations, 150, 400, 2001));
            Assert.Equal(new SubwayStretch(stations, 150, 400).Key, new SubwayStretch(stations, 150, 400, 0).Key);
            Assert.NotEqual(new SubwayStretch(stations, 150, 400).Key, new SubwayStretch(stations, 150, 400, 100).Key);
        }

        [Fact(DisplayName = "Subway stretch - with stub 0, a line with a lone station is dropped but the station is counted")]
        public void Trim_StubZeroDropsALoneStation()
        {
            var (lines, stations) = new SubwayStretch([At(0, 2500)], 150, 0).Trim(Line((0, 0), (0, 5000)));

            Assert.True(lines.IsEmpty);
            Assert.Equal(1, stations);
        }

        [Fact(DisplayName = "Subway stretch - a stretch carries on across a junction to a branch with stations; the other branch is dropped")]
        public void Trim_CarriesOnAcrossAJunction()
        {
            var lines = Lines(
                [(0, 0), (0, 2000)],
                [(0, 2000), (0, 4000)],
                [(0, 2000), (2000, 3000)]);

            var (kept, stations) = Stretch((0, 1000), (0, 3000)).Trim(lines);

            Assert.Equal(2, stations);
            Assert.Equal(2000, Metres(kept), 0);
            Assert.All(kept.Coordinates, c => Assert.Equal(0, East(c.X), 0));
        }

        [Fact(DisplayName = "Subway stretch - a part without stations is kept whole between parts with stations")]
        public void Trim_KeepsAPartBetweenPartsWithStations()
        {
            // Spurs at 2000 and 4000 m make junctions there, so the line is three parts
            var lines = Lines(
                [(0, 0), (0, 2000)],
                [(0, 2000), (0, 4000)],
                [(0, 4000), (0, 6000)],
                [(0, 2000), (1000, 2000)],
                [(0, 4000), (1000, 4000)]);

            var (kept, stations) = Stretch((0, 1000), (0, 5000)).Trim(lines);

            Assert.Equal(2, stations);
            Assert.Equal(3, kept.NumGeometries);
            Assert.Equal(4000, Metres(kept), 0);
        }

        [Fact(DisplayName = "Subway stretch - a station at a junction ends the stretch there, without stubs into the other branches")]
        public void Trim_StationAtAJunction()
        {
            var lines = Lines(
                [(0, 0), (0, 2000)],
                [(0, 2000), (0, 4000)],
                [(0, 2000), (2000, 3000)]);

            var (kept, stations) = Stretch((0, 1000), (0, 2000)).Trim(lines);

            Assert.Equal(2, stations);
            Assert.Equal(1000, Metres(kept), 0);
        }

        [Fact(DisplayName = "Subway stretch - a station just inside a branch at a junction adds nothing to the stretch")]
        public void Trim_StationJustInsideABranch()
        {
            var lines = Lines(
                [(0, 0), (0, 2000)],
                [(0, 2000), (0, 4000)],
                [(0, 2000), (2000, 3000)]);

            // Nearest the third branch, under a metre along it
            var (kept, stations) = Stretch((0, 1000), (1, 2000.2)).Trim(lines);

            Assert.Equal(2, stations);
            Assert.Equal(1, kept.NumGeometries);
            Assert.Equal(1000, Metres(kept), 0);
        }

        [Fact(DisplayName = "Subway stretch - a loop without stations is dropped; with stations it is kept")]
        public void Trim_Loops()
        {
            var trunk = new[] { (0d, 0d), (0d, 1000d) };
            var loop = new[] { (0d, 1000d), (500d, 1500d), (0d, 2000d), (-500d, 1500d), (0d, 1000d) };

            // A terminal loop without stations is dropped
            var (kept, _) = Stretch((0, 200), (0, 800)).Trim(Lines(trunk, loop));
            Assert.Equal(600, Metres(kept), 0);

            // Along a loop of its own, from the first station to the last
            var (ring, stations) = Stretch((500, 1500), (0, 2000)).Trim(Line(loop));
            Assert.Equal(2, stations);
            Assert.Equal(Math.Sqrt(2) * 500, Metres(ring), 0);

            // A loop with a station, at the end of a stretch, is kept whole
            Assert.Equal(800 + (4 * Math.Sqrt(2) * 500), Metres(Stretch((0, 200), (0, 2000)).Trim(Lines(trunk, loop)).Lines), 0);

            // A lone station on a loop keeps the stub along it
            Assert.Equal(800, Metres(Stretch((500, 1500)).Trim(Line(loop)).Lines), 0);
        }

        [Fact(DisplayName = "Subway stretch - a lone station where two pieces meet counts once and keeps the whole stub across the join")]
        public void Trim_StubAtAJoin()
        {
            // The pieces miss each other by 10 cm, as the source's do, so they stay two parts
            var lines = Lines(
                [(0, 0), (0, 2000)],
                [(0, 2000.1), (0, 4000)]);

            var (kept, stations) = Stretch((0, 2000)).Trim(lines);

            Assert.Equal(1, stations);
            Assert.Equal(2, kept.NumGeometries);
            Assert.Equal(800, Metres(kept), 0);
            Assert.Equal(1600, North(kept.Coordinates.Min(c => c.Y)), 0);
            Assert.Equal(2400, North(kept.Coordinates.Max(c => c.Y)), 0);
        }

        [Fact(DisplayName = "Subway stretch - a stub carries on through a short piece into the next, whichever way they are drawn")]
        public void Trim_StubCarriesOnThroughPieces()
        {
            // The last piece is drawn north to south
            var lines = Lines(
                [(0, 0), (0, 2000)],
                [(0, 2000.1), (0, 2100)],
                [(0, 5000), (0, 2100.1)]);

            var (kept, _) = Stretch((0, 1900)).Trim(lines);

            Assert.Equal(3, kept.NumGeometries);
            Assert.Equal(800, Metres(kept), 0);
            Assert.Equal(2300, North(kept.Coordinates.Max(c => c.Y)), 0);
        }

        [Fact(DisplayName = "Subway stretch - a stub near a junction carries on into both branches")]
        public void Trim_StubIntoBranches()
        {
            var lines = Lines(
                [(0, 0), (0, 2000)],
                [(0, 2000), (0, 4000)],
                [(0, 2000), (2000, 3000)]);

            var (kept, _) = Stretch((0, 1800)).Trim(lines);

            // 600 m on the trunk, then 200 m along each branch
            Assert.Equal(3, kept.NumGeometries);
            Assert.Equal(1000, Metres(kept), 0);
        }

        [Fact(DisplayName = "Subway stretch - each set of connected parts is trimmed on its own")]
        public void Trim_SeparatePieces()
        {
            // Two lines 1 km apart, as a clip at a river leaves them
            var lines = Lines(
                [(0, 0), (0, 2000)],
                [(0, 3000), (0, 6000)]);

            var (kept, stations) = Stretch((0, 500), (0, 1500), (0, 4500)).Trim(lines);

            Assert.Equal(3, stations);
            Assert.Equal(2, kept.NumGeometries);
            Assert.Equal(1000 + 800, Metres(kept), 0);
        }

        [Fact(DisplayName = "Subway stretch - nothing to trim gives no lines and no stations")]
        public void Trim_NoLines()
        {
            var stretch = Stretch((0, 0));

            var (empty, none) = stretch.Trim(Factory.CreateMultiLineString());
            Assert.True(empty.IsEmpty);
            Assert.Equal(0, none);

            Assert.True(stretch.Trim(Factory.CreatePoint(new Coordinate(Longitude, Latitude))).Lines.IsEmpty);
            Assert.Throws<ArgumentNullException>(() => stretch.Trim(null!));
        }

        [Fact(DisplayName = "Subway stretch - the same stations in another order are the same request")]
        public void Key_IgnoresTheOrderOfTheStations()
        {
            var points = new[] { (40.7, -74.0), (40.8, -73.9), (40.7, -73.95) };
            var stretch = new SubwayStretch(points, 150, 400);

            Assert.Equal(stretch.Key, new SubwayStretch(points.Reverse().ToArray(), 150, 400).Key);
            Assert.Equal([(40.7, -74.0), (40.7, -73.95), (40.8, -73.9)], stretch.Points);
            Assert.NotEqual(stretch.Key, new SubwayStretch(points, 151, 400).Key);
            Assert.NotEqual(stretch.Key, new SubwayStretch(points, 150, 399).Key);
            Assert.NotEqual(stretch.Key, new SubwayStretch(points[..2], 150, 400).Key);
        }

        [Theory(DisplayName = "Subway stretch - a radius outside 10 to 500 metres or a stub outside 0 to 2000 is an error")]
        [InlineData(9.9, 400)]
        [InlineData(500.1, 400)]
        [InlineData(double.NaN, 400)]
        [InlineData(150, -0.1)]
        [InlineData(150, 2000.1)]
        [InlineData(150, double.PositiveInfinity)]
        public void Constructor_RejectsOutOfRangeDistances(double radius, double stub)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SubwayStretch([(40.75, -73.98)], radius, stub));
        }

        [Fact(DisplayName = "Subway stretch - 1 to 300 stations")]
        public void Constructor_NeedsOneTo300Stations()
        {
            Assert.Throws<ArgumentNullException>(() => new SubwayStretch(null!, 150, 400));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SubwayStretch([], 150, 400));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SubwayStretch(Enumerable.Repeat((40.75, -73.98), 301).ToArray(), 150, 400));
            Assert.Equal(300, new SubwayStretch(Enumerable.Repeat((40.75, -73.98), 300).ToArray(), 10, 0).Points.Count);
        }

        private static SubwayStretch Stretch(params (double East, double North)[] stations) =>
            new(stations.Select(s => At(s.East, s.North)).ToArray(), SubwayStretch.DefaultRadiusMetres, SubwayStretch.DefaultStubMetres);

        private static (double Latitude, double Longitude) At(double east, double north) =>
            (Latitude + (north / MetresPerDegree), Longitude + (east / MetresPerDegreeLongitude));

        private static Coordinate Position(double east, double north)
        {
            var (latitude, longitude) = At(east, north);
            return new Coordinate(longitude, latitude);
        }

        private static LineString Line(params (double East, double North)[] points) =>
            Factory.CreateLineString(points.Select(p => Position(p.East, p.North)).ToArray());

        private static MultiLineString Lines(params (double East, double North)[][] lines) =>
            Factory.CreateMultiLineString(lines.Select(l => Line(l)).ToArray());

        private static double North(double latitude) => (latitude - Latitude) * MetresPerDegree;

        private static double East(double longitude) => (longitude - Longitude) * MetresPerDegreeLongitude;

        private static double Metres(Geometry lines) =>
            Enumerable.Range(0, lines.NumGeometries)
                .Select(i => lines.GetGeometryN(i).Coordinates)
                .Sum(c => c.Zip(c.Skip(1), (a, b) => Math.Sqrt(Math.Pow(East(b.X) - East(a.X), 2) + Math.Pow(North(b.Y) - North(a.Y), 2))).Sum());
    }
}
