namespace MicroService.WebApi.Models
{
    /// <summary>
    /// Feature Request Model
    /// </summary>
    public class FeatureRequestModel
    {
        /// <summary>
        ///  X Point.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        ///  Y Point.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Lookup Service Key
        /// </summary>
        public string? Key { get; set; }
    }

}
