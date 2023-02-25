using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using Coordinate = MicroService.Service.Models.Base.Coordinate;

namespace MicroService.Service.Services
{
    public class ZipCodeService : AbstractShapeService<ZipCodeShape>, IShapeService<ZipCodeShape>
    {
        public ZipCodeService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.ZipCodes));
        }

        public override ZipCodeShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var features = GetFeatures();
            var matchingFeature = features.FirstOrDefault(f => f.Geometry.Contains(point));

            if (matchingFeature == null)
            {
                return null;
            }

            return new ZipCodeShape
            {
                ZipCode = matchingFeature.Attributes["ZIPCODE"].ToString(),
                BldgZip = matchingFeature.Attributes["BLDGZIP"].ToString(),
                PostOfficeName = matchingFeature.Attributes["PO_NAME"].ToString(),
                Population = int.Parse(matchingFeature.Attributes["POPULATION"].ToString()),
                Area = double.Parse(matchingFeature.Attributes["AREA"].ToString()),
                State = matchingFeature.Attributes["STATE"].ToString(),
                County = matchingFeature.Attributes["COUNTY"].ToString(),
                ShapeArea = double.Parse(matchingFeature.Attributes["SHAPE_AREA"].ToString()),
                ShapeLength = double.Parse(matchingFeature.Attributes["SHAPE_LEN"].ToString()),
                Coordinates = new List<Coordinate>(),
            };
        }

        public override IEnumerable<ZipCodeShape> GetFeatureLookup(List<KeyValuePair<string, string>> attributes)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ZipCodeShape> GetFeatureAttributes()
        {
            var features = GetFeatures();

            return features.Select(f => new ZipCodeShape
            {
                ZipCode = f.Attributes["ZIPCODE"].ToString(),
                BldgZip = f.Attributes["BLDGZIP"].ToString(),
                PostOfficeName = f.Attributes["PO_NAME"].ToString(),
                Population = int.Parse(f.Attributes["POPULATION"].ToString()),
                Area = double.Parse(f.Attributes["AREA"].ToString()),
                State = f.Attributes["STATE"].ToString(),
                County = f.Attributes["COUNTY"].ToString(),
                ShapeArea = double.Parse(f.Attributes["SHAPE_AREA"].ToString()),
                ShapeLength = double.Parse(f.Attributes["SHAPE_LEN"].ToString()),
            });
        }
    }
}