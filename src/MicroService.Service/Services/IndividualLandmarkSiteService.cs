using MicroService.Data.Enum;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using System;
using System.Collections.Generic;

namespace MicroService.Service.Services
{
    public class IndividualLandmarkSiteService : AbstractShapeService<IndividualLandmarkSiteShape>, IShapeService<IndividualLandmarkSiteShape>
    {
        public IndividualLandmarkSiteService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.IndividualLandmarkSite));
        }

        public IEnumerable<IndividualLandmarkSiteShape> GetFeatureAttributes()
        {
            var features = GetFeatures();
            var results = new List<IndividualLandmarkSiteShape>(features.Count);

            foreach (var f in features)
            {
                var borough = f.Attributes["borough"].ToString();
                var model = new IndividualLandmarkSiteShape
                {
                    //LPNumber = f.Attributes["LP_NUMBER"].ToString(),
                    //AreaName = f.Attributes["AREA_NAME"].ToString(),
                    BoroName = borough,
                    BoroCode = (int)Enum.Parse(typeof(Borough), borough),
                };

                results.Add(model);
            }

            return results;
        }

        public override IndividualLandmarkSiteShape GetFeatureLookup(double x, double y)
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<IndividualLandmarkSiteShape> GetFeatureLookup(List<KeyValuePair<string, string>> features)
        {
            throw new System.NotImplementedException();
        }
    }
}
