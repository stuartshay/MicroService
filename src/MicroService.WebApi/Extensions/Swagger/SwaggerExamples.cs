using MicroService.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace MicroService.WebApi.Extensions.Swagger
{
    /// <summary>
    /// Swagger Display Examples
    /// </summary>
    public class SwaggerFeatureRequestExamples : IExamplesProvider<FeatureAttributeLookupRequestModel>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public FeatureAttributeLookupRequestModel GetExamples()
        {
            //var attributes = new List<KeyValuePair<string, string>>
            //{
            //    new("BOROUGH", "BX"),
            //    new("LP_NUMBER", "LP-02403")
            //};

            //return new FeatureAttributeLookupRequestModel
            //{
            //    Key = "HistoricDistricts",
            //    Attributes = attributes,
            //};

            var attributes = new List<KeyValuePair<string, string>>
            {
                new("lp_number", "LP-00860")
            };

            return new FeatureAttributeLookupRequestModel
            {
                Key = "ScenicLandmarks",
                Attributes = attributes,
            };
        }
    }
}
