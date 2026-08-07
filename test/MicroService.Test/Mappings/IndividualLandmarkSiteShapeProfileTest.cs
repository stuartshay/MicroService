using AutoMapper;
using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Test.Mappings.Base;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Xunit;

namespace MicroService.Test.Mappings
{
    public class IndividualLandmarkSiteShapeProfileTest : BaseMapper<IndividualLandmarkSiteShape>
    {
        private static Feature BuildFeature(object? alternativeName)
        {
            var attributes = new Dictionary<string, object?>
            {
                { "lpc_lpnumb", "LP-00001" },
                { "lpc_name", "Test Landmark" },
                { "borough", "MN" },
                { "bbl", "1000010001" },
                { "address", "1 Test Street" },
                { "block", "1" },
                { "lot", "1" },
                { "objectid", "1" },
                { "date_des_d", "2000-01-01" },
                { "lpc_altern", alternativeName },
                { "lpc_site_d", "Individual Landmark" },
                { "landmark_t", "Individual Landmark" },
                { "lpc_site_s", "Designated" },
                { "url_report", "http://example.com" },
                { "shape_area", "1.0" },
                { "shape_leng", "1.0" },
            };

            return new Feature(new Point(0, 0), new AttributesTable(attributes!));
        }

        [Fact]
        public void Map_ReturnsNullAlternativeName_WhenAttributeIsAbsent()
        {
            var feature = BuildFeature(null);

            var shape = Mapper.Map<IndividualLandmarkSiteShape>(feature);

            Assert.Null(shape.AlternativeName);
        }

        [Fact]
        public void Map_StripsNullCharacters_FromAlternativeName()
        {
            var value = "Old" + '\u0000' + "Name";
            var feature = BuildFeature(value);

            var shape = Mapper.Map<IndividualLandmarkSiteShape>(feature);

            Assert.Equal("OldName", shape.AlternativeName);
        }
    }
}
