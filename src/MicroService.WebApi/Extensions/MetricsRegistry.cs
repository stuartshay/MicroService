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

        /// <summary>
        /// Get Feature Lookup Counter
        /// </summary>
        public static CounterOptions GetFeatureTypeLookupCounter(string type) => new CounterOptions
        {
            Name = $"{type}-Feature Request Lookup Counter",
            MeasurementUnit = Unit.Calls,
            Tags = new MetricTags("Feature",type),
        };
    }

 

    /// <summary>
    /// App Metrics Helpers
    /// </summary>
    public static class MetricsHelpers
    {
        /// <summary>
        /// App Metrics Custom Tag
        /// </summary>
        /// <param name="metricsRoot"></param>
        /// <param name="tagName"></param>
        /// <param name="tagValue"></param>
        public static void SetMetricsCustomTag(IMetricsRoot metricsRoot, string tagName,  string tagValue)
        {
            if (!metricsRoot.Options.GlobalTags.ContainsKey(tagName))
            {
                metricsRoot.Options.GlobalTags.Add(tagName, tagValue);
            }
            else if (string.IsNullOrEmpty(metricsRoot.Options.GlobalTags[tagName]) || metricsRoot.Options.GlobalTags[tagName] == "unknown")
            {
                metricsRoot.Options.GlobalTags[tagName] = tagValue;
            }
        }
    }
}
