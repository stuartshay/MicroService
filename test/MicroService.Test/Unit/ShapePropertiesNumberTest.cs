using MicroService.Service.Models.Enum;
using Xunit;

namespace MicroService.Test.Unit
{
    /// <summary>
    /// The FeatureService endpoints accept a dataset by its number as well as its name
    /// (e.g. GET /api/v1/FeatureService/15), so a new member goes last and the existing numbers
    /// never move (Codex review on #579: SubwayServiceLines before ZipCodes made 15 the subway lines).
    /// </summary>
    public class ShapePropertiesNumberTest
    {
        [Theory]
        [InlineData(ShapeProperties.BoroughBoundaries, 0)]
        [InlineData(ShapeProperties.CommunityDistricts, 1)]
        [InlineData(ShapeProperties.DSNYDistricts, 2)]
        [InlineData(ShapeProperties.HistoricDistricts, 3)]
        [InlineData(ShapeProperties.IndividualLandmarkSite, 4)]
        [InlineData(ShapeProperties.IndividualLandmarkHistoricDistricts, 5)]
        [InlineData(ShapeProperties.NationalRegisterHistoricPlaces, 6)]
        [InlineData(ShapeProperties.Neighborhoods, 7)]
        [InlineData(ShapeProperties.NeighborhoodTabulationAreas, 8)]
        [InlineData(ShapeProperties.NypdPolicePrecincts, 9)]
        [InlineData(ShapeProperties.NypdSectors, 10)]
        [InlineData(ShapeProperties.NychaDevelopments, 11)]
        [InlineData(ShapeProperties.Parks, 12)]
        [InlineData(ShapeProperties.ScenicLandmarks, 13)]
        [InlineData(ShapeProperties.Subway, 14)]
        [InlineData(ShapeProperties.ZipCodes, 15)]
        [InlineData(ShapeProperties.SubwayServiceLines, 16)]
        public void Dataset_Numbers_Do_Not_Move(ShapeProperties dataset, int number)
        {
            Assert.Equal(number, (int)dataset);
        }
    }
}
