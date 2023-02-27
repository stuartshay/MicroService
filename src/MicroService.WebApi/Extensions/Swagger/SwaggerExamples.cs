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
            // BoroughBoundaries
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("BoroCode", 1),
                new("BoroName", "Manhattan"),
            };

            return new FeatureAttributeLookupRequestModel
            {
                Key = "BoroughBoundaries",
                Attributes = attributes,
            };

            //// HistoricDistricts
            //var attributes = new List<KeyValuePair<string, object>>
            //{
            //    new("LPNumber", "LP-02403"),
            //    new("BoroName", "BX"),
            //};

            //return new FeatureAttributeLookupRequestModel
            //{
            //    Key = "HistoricDistricts",
            //    Attributes = attributes,
            //};








            //var attributes = new List<KeyValuePair<string, string>>
            //{
            //    new("lp_number", "LP-00860")
            //};

            //return new FeatureAttributeLookupRequestModel
            //{
            //    Key = "ScenicLandmarks",
            //    Attributes = attributes,
            //};
        }
    }
}
