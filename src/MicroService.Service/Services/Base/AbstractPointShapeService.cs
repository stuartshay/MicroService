using MicroService.Service.Models.Enum.Attributes;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System;
using System.Linq;

namespace MicroService.Service.Services.Base
{
    /// <summary>
    /// A shape of points: a lookup returns the feature nearest the point, as no point contains another.
    /// </summary>
    public abstract class AbstractPointShapeService<TShape> : AbstractShapeService<TShape>
        where TShape : class, new()
    {
        private readonly Datum _datum;

        protected AbstractPointShapeService(
            ShapefileDataReaderResolver shapefileDataReaderResolver,
            ILogger logger,
            Func<Feature, TShape> mapFeature)
            : base(shapefileDataReaderResolver, logger, mapFeature)
        {
            _datum = ShapeRegistry.GetByServiceType(GetType()).Datum;
        }

        public override TShape? GetFeatureLookup(double x, double y, Datum datum)
        {
            var point = ToShapePoint(x, y, datum);

            // State Plane feet are a flat metric already; in WGS84 a degree of longitude is
            // shorter than one of latitude (by the cosine of the latitude), so it is scaled first
            var xScale = _datum == Datum.Wgs84 ? Math.Cos(point.Y * Math.PI / 180) : 1;
            var nearest = GetFeatures().MinBy(f => SquaredDistance(f.Geometry, point, xScale));
            return nearest is null ? null : MapFeature(nearest);
        }

        private static double SquaredDistance(Geometry geometry, Point point, double xScale)
        {
            var coordinate = geometry.Coordinate;
            var dx = (coordinate.X - point.X) * xScale;
            var dy = coordinate.Y - point.Y;
            return (dx * dx) + (dy * dy);
        }
    }
}
