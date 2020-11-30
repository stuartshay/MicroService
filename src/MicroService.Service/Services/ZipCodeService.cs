using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Geometries;

namespace MicroService.Service.Services
{
    public class ZipCodeService<T> : AbstractShapeService<ZipCodeShape>, IShapeService<ZipCodeShape>
    {
        public ZipCodeService(ShapefileDataReaderResolver shapefileDataReaderResolver)
        {
            ShapeFileDataReader = shapefileDataReaderResolver(nameof(ShapeProperties.ZipCodes));
        }

        public override ZipCodeShape GetFeatureLookup(double x, double y)
        {
            // Validate Point is in Range
            var point = new Point(x, y);

            var model = new ZipCodeShape();

            var features = GetFeatures();
            foreach (var f in features)
            {
                var exists = f.Geometry.Contains(point);
                if (exists)
                {
                    model = new ZipCodeShape
                    {
                        ZipCode = f.Attributes["ZIPCODE"].ToString(),
                        BldgZip = f.Attributes["BLDGZIP"].ToString(),
                        PostOfficeName = f.Attributes["PO_NAME"].ToString(), 
                        Population = int.Parse(f.Attributes["POPULATION"].ToString()),
                        Area = double.Parse(f.Attributes["AREA"].ToString()), 
                        State = f.Attributes["STATE"].ToString(),
                        County = f.Attributes["COUNTY"].ToString(),
                    };
                }
            }

            if (!model.ArePropertiesNotNull())
            {
                return null;
            }

            return model;
        }
    }
}
