using App.Metrics;
using App.Metrics.Counter;

namespace MicroService.WebApi.Extensions
{
    /// <summary>
    /// App Metrics Registry
    /// </summary>
    public static class MetricsRegistry
    {
        /// <summary>
        /// Get Shapes Counter
        /// </summary>
        public static CounterOptions GetShapesCounter => new CounterOptions
        {
            Name = "Shapes Counter",
            MeasurementUnit = Unit.Calls,
        };

        /// <summary>
        /// Get Shapes Properties Counter
        /// </summary>
        public static CounterOptions GetShapePropertiesCounter => new CounterOptions
        {
            Name = "Shapes Properties Counter",
            MeasurementUnit = Unit.Calls,
        };

        /// <summary>
        /// Get Feature Lookup Counter
        /// </summary>
        public static CounterOptions GetFeatureLookupCounter => new CounterOptions
        {
            Name = "Feature Lookup Counter",
            MeasurementUnit = Unit.Calls,
        };
    }
}
