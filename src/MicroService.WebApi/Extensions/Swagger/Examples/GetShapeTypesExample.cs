using Swashbuckle.AspNetCore.Filters;

namespace MicroService.WebApi.Extensions.Swagger.Examples
{
    /// <summary>
    /// Swagger Example Shape Type 
    /// </summary>
    public class GetShapeTypesExample : IExamplesProvider<IEnumerable<object>>
    {
        /// <summary>
        /// 
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
                    directory = "Borough_Boundaries/NAD83",
                    datum = "Nad83"
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
