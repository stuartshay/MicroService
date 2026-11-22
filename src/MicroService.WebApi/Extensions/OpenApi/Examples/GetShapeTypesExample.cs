using MicroService.WebApi.Extensions.OpenApi;

namespace MicroService.WebApi.Extensions.OpenApi.Examples
{
    /// <summary>
    /// Swagger Example Shape Type 
    /// </summary>
    public class GetShapeTypesExample : IExampleProvider<IEnumerable<object>>
    {
        /// <summary>
        /// Builds the example response payload listing available shape types.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> GetExamples()
        {
            return new List<object>
            {
                new {
                    key = "BoroughBoundaries",
                    description = "Borough Boundaries",
                    fileName = "nybb",
                    directory = "Borough_Boundaries/WGS84",
                    datum = "Wgs84"
                },
                new {
                    key = "CommunityDistricts",
                    description = "Community Districts",
                    fileName = "nycd",
                    directory = "Community_Districts",
                    datum = "Nad83"
                }
            };
        }
    }
}