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
            var shapeType = "IndividualLandmarkHistoricDistricts";

            //var shapeType = "IndividualLandmarkSite";

            // BoroughBoundaries
            if (shapeType == "BoroughBoundaries")
            {
                return new FeatureAttributeLookupRequestModel
                {
                    Key = "BoroughBoundaries",
                    Attributes = new List<KeyValuePair<string, object>>
                    {
                        new("BoroCode", 1),
                        new("BoroName", "Manhattan"),
                    },
                };
            }

            // CommunityDistricts
            if (shapeType == "CommunityDistricts")
            {
                return new FeatureAttributeLookupRequestModel
                {
                    Key = "CommunityDistricts",
                    Attributes = new List<KeyValuePair<string, object>>
                    {
                        new("BoroCd", 101),
                    },
                };
            }

            // DSNYDistricts
            if (shapeType == "DSNYDistricts")
            {
                return new FeatureAttributeLookupRequestModel
                {
                    Key = "DSNYDistricts",
                    Attributes = new List<KeyValuePair<string, object>>
                    {
                        new("District","SI02"),
                    },
                };
            }

            // HistoricDistricts
            if (shapeType == "HistoricDistricts")
            {
                return new FeatureAttributeLookupRequestModel
                {
                    Key = "HistoricDistricts",
                    Attributes = new List<KeyValuePair<string, object>>
                    {
                        new("LPNumber", "LP-02403"),
                    },
                };
            }

            // IndividualLandmarkHistoricDistricts
            if (shapeType == "IndividualLandmarkHistoricDistricts")
            {
                return new FeatureAttributeLookupRequestModel
                {
                    Key = "IndividualLandmarkHistoricDistricts",
                    Attributes = new List<KeyValuePair<string, object>>
                    {
                        new("Bbl", "3079170009"),
                        //new("HistDist", "Fraunces Tavern Block Historic District"),
                    },
                };
            }

            // IndividualLandmarkSite
            if (shapeType == "IndividualLandmarkSite")
            {
                return new FeatureAttributeLookupRequestModel
                {
                    Key = "IndividualLandmarkSite",
                    Attributes = new List<KeyValuePair<string, object>>
                    {
                        new("LPNumber", "LP-00001"),
                        new("BBL", 3079170009),
                    },
                };
            }

            // ScenicLandmarks
            if (shapeType == "ScenicLandmarks")
            {
                return new FeatureAttributeLookupRequestModel
                {
                    Key = "ScenicLandmarks",
                    Attributes = new List<KeyValuePair<string, object>>
                    {
                        new("LPNumber", "LP-00879"),
                        new("BoroName", "MN"),
                    },
                };
            }


            throw new ArgumentException($"Invalid Swagger Example Request");
        }
    }
}
