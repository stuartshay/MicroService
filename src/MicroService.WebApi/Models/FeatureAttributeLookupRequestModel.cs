namespace MicroService.WebApi.Models
{
    /// <summary>
    /// Request model for looking up a shape feature by attribute key/value pairs.
    /// </summary>
    public class FeatureAttributeLookupRequestModel
    {
        /// <summary>
        /// Lookup Service Key
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Attribute Key Value Pairs
        /// </summary>
        public List<KeyValuePair<string, object>>? Attributes { get; set; }

    }
}
