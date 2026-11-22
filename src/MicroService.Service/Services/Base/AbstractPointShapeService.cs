using MicroService.Service.Models.Enum.Attributes;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
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
        protected AbstractPointShapeService(
            ShapefileDataReaderResolver shapefileDataReaderResolver,
            ILogger logger,
            Func<Feature, TShape> mapFeature)
            : base(shapefileDataReaderResolver, logger, mapFeature)
        {
        }

        public override TShape? GetFeatureLookup(double x, double y, Datum datum)
        {
            var point = ToShapePoint(x, y, datum);
            var nearest = GetFeatures().MinBy(f => f.Geometry.Distance(point));
            return nearest is null ? null : MapFeature(nearest);
        }
    }
}
