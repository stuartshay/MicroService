using MicroService.Service.Services.Base;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.IO.Converters;
using NetTopologySuite.Simplify;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace MicroService.Service.Services.Layers
{
    /// <summary>
    /// A box in WGS84 degrees: west, south, east and north.
    /// </summary>
    public sealed record LayerBox(double West, double South, double East, double North)
    {
        /// <summary>The widest a box may be, in degrees each way (about 40 km by 55 km).</summary>
        public const double MaxSpanDegrees = 0.5;

        public Envelope ToEnvelope() => new(West, East, South, North);

        /// <summary>
        /// "west,south,east,north" in decimal degrees, inside the NYC area and at most
        /// <see cref="MaxSpanDegrees"/> each way; otherwise null with the reason.
        /// </summary>
        public static LayerBox? Parse(string? text, out string? error)
        {
            error = null;
            var parts = (text ?? string.Empty).Split(',', StringSplitOptions.TrimEntries);
            var values = new double[4];
            if (parts.Length != 4 || parts.Select((p, i) => double.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out values[i])).Any(ok => !ok)
                || values.Any(v => !double.IsFinite(v)))
            {
                error = "bbox is west,south,east,north in decimal degrees.";
                return null;
            }

            var box = new LayerBox(values[0], values[1], values[2], values[3]);
            if (box.West >= box.East || box.South >= box.North)
            {
                error = "bbox west must be less than east, and south less than north.";
            }
            else if (box.West < -75 || box.East > -73 || box.South < 40 || box.North > 41.5)
            {
                error = "bbox must be in the New York City area (longitude -75 to -73, latitude 40 to 41.5).";
            }
            else if (box.East - box.West > MaxSpanDegrees || box.North - box.South > MaxSpanDegrees)
            {
                error = $"bbox may span at most {MaxSpanDegrees} degrees each way.";
            }

            return error is null ? box : null;
        }
    }

    /// <summary>
    /// A layer's features in a box, shaped for drawing: WGS84, polygons simplified, coordinates
    /// rounded, and only a title and a few labelled details per feature.
    /// </summary>
    public class LayerService
    {
        /// <summary>Polygon simplification in metres: well under a pixel at street zooms.</summary>
        public const double SimplifyMetres = 2;

        /// <summary>The most features one answer carries.</summary>
        public const int MaxFeatures = 5000;

        private const double MetresPerDegree = 111_320;
        private const int CoordinateDecimals = 6;

        private static readonly JsonSerializerOptions GeoJsonOptions = new()
        {
            Converters = { new GeoJsonConverterFactory() },
        };

        private readonly ShapefileDataReaderResolver _readers;

        public LayerService(ShapefileDataReaderResolver readers)
        {
            _readers = readers ?? throw new ArgumentNullException(nameof(readers));
        }

        /// <summary>How many features each layer has.</summary>
        public int Count(LayerDefinition layer) => Features(layer).Count;

        /// <summary>
        /// The layer's features whose outline touches the box, as a GeoJSON FeatureCollection; each
        /// feature has the properties title and, when it has any, details (label to value).
        /// </summary>
        public string GetGeoJson(LayerDefinition layer, LayerBox box)
        {
            ArgumentNullException.ThrowIfNull(layer);
            ArgumentNullException.ThrowIfNull(box);
            var envelope = box.ToEnvelope();
            var factory = new GeometryFactory(new PrecisionModel(), 4326);
            var tolerance = SimplifyMetres / MetresPerDegree;

            // The envelope check is a quick first pass; a polygon's envelope can overlap the box
            // while the polygon itself doesn't (a bay between two shores), so the geometry decides
            var area = PreparedGeometryFactory.Prepare(factory.ToGeometry(envelope));
            var touching = Features(layer)
                .Where(f => f.Geometry.EnvelopeInternal.Intersects(envelope) && area.Intersects(f.Geometry))
                .Take(MaxFeatures);

            var collection = new FeatureCollection();
            foreach (var feature in touching)
            {
                var geometry = layer.Geometry == LayerCatalog.Polygon
                    ? DouglasPeuckerSimplifier.Simplify(feature.Geometry, tolerance)
                    : feature.Geometry;
                collection.Add(new Feature(Round(geometry, factory), Properties(layer, feature)));
            }

            return JsonSerializer.Serialize(collection, GeoJsonOptions);
        }

        /// <summary>
        /// The layer's features. Every layer's shape is WGS84 (LayerCatalog's test checks it); a NAD83
        /// shape added as a layer would need GeoTransformationHelper.CopyNad83ToWgs84 here.
        /// </summary>
        private IReadOnlyCollection<IFeature> Features(LayerDefinition layer) => _readers(layer.Key).GetFeatures();

        private static AttributesTable Properties(LayerDefinition layer, IFeature feature)
        {
            var properties = new AttributesTable { { "title", Text(feature, layer.TitleField) } };
            var details = layer.Details
                .Select(d => (d.Label, Value: Text(feature, d.Field)))
                .Where(d => d.Value.Length > 0)
                .ToDictionary(d => d.Label, d => (object)d.Value);
            if (details.Count > 0)
            {
                properties.Add("details", details);
            }

            return properties;
        }

        private static string Text(IFeature feature, string field) =>
            feature.Attributes.Exists(field)
                ? (Convert.ToString(feature.Attributes[field], CultureInfo.InvariantCulture) ?? string.Empty).Trim()
                : string.Empty;

        private static Geometry Round(Geometry geometry, GeometryFactory factory)
        {
            var copy = factory.CreateGeometry(geometry);
            copy.Apply(new RoundFilter(CoordinateDecimals));
            copy.GeometryChanged();
            return copy;
        }

        private sealed class RoundFilter(int decimals) : ICoordinateSequenceFilter
        {
            public bool Done => false;

            public bool GeometryChanged => true;

            public void Filter(CoordinateSequence seq, int i)
            {
                seq.SetX(i, Math.Round(seq.GetX(i), decimals));
                seq.SetY(i, Math.Round(seq.GetY(i), decimals));
            }
        }
    }
}
