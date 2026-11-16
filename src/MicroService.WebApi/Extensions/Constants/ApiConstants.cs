namespace MicroService.WebApi.Extensions.Constants
{
    /// <summary>
    /// Api Constants
    /// </summary>
    public static class ApiConstants
    {
        /// <summary>
        /// Application Name
        /// </summary>
        public const string ApplicationName = "MicroService.WebApi";

        /// <summary>
        /// Application Description
        /// </summary>
        public const string ApplicationDescription = "MicroService.WebApi";

        /// <summary>
        /// Open Api Contact Name
        /// </summary>
        public const string OpenApiContactName = "Stuart Shay";

        /// <summary>
        /// Open Api Contact Email
        /// </summary>
        public const string OpenApiContactEmail = "sshay@yahoo.com";

        /// <summary>
        /// One OpenAPI document per API version group (Asp.Versioning group names, 'v'VVV)
        /// </summary>
        public static readonly IReadOnlyList<string> OpenApiDocuments = ["v1"];
    }
}
