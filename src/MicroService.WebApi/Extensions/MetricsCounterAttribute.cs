﻿using App.Metrics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroService.WebApi.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public class MetricsCounterAttribute : IResultFilter
    {
        private readonly IMetrics _metrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsCounterAttribute"/> class.
        /// </summary>
        /// <param name="metrics"></param>
        public MetricsCounterAttribute(IMetrics metrics)
        {
            _metrics = metrics;
        }

        /// <inheritdoc/>
        public void OnResultExecuting(ResultExecutingContext context)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Feature Lookup Counter
        /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters?view=aspnetcore-5.0#action-filters
        /// </summary>
        /// <param name="context"></param>
        public void OnResultExecuted(ResultExecutedContext context)
        {
            _metrics.Measure.Counter.Increment(MetricsRegistry.GetFeatureLookupCounter);
        }
    }
}
