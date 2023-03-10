using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Test.Fixture;
using MicroService.Test.Integration.Interfaces;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Xunit;
using Xunit.Abstractions;

namespace MicroService.Test.Integration
{
    public class IndividualLandmarkHistoricDistrictsTest : IClassFixture<ShapeServiceFixture>, IShapeTest
    {
        public IShapeService<IndividualLandmarkHistoricDistrictsShape> _service;

        private readonly ITestOutputHelper _testOutputHelper;

        public IndividualLandmarkHistoricDistrictsTest(ShapeServiceFixture fixture, ITestOutputHelper output)
        {
            _service = fixture.IndividualLandmarkHistoricDistrictsService;
            _testOutputHelper = output;
        }

        [Fact(DisplayName = "Get Shape File Properties")]
        public void Get_Shape_Properties()
        {
            var sut = _service.GetShapeProperties();
            Assert.NotNull(sut);

            _testOutputHelper.WriteLine($"Shape type: {sut.ShapeType}");

            //Display the min and max bounds of the shapefile
            var bounds = sut.Bounds;
            _testOutputHelper.WriteLine($"Min bounds: ({bounds.MinX},{bounds.MinY})");
            _testOutputHelper.WriteLine($"Max bounds: ({bounds.MaxX},{bounds.MaxY})");
        }


        [Fact(DisplayName = "Get Shape File Database Properties")]
        public void Get_Shape_Database_Properties()
        {
            var sut = _service.GetShapeDatabaseProperties();
            Assert.NotNull(sut);

            // Display summary information about the Dbase file
            _testOutputHelper.WriteLine("Dbase info");
            _testOutputHelper.WriteLine($"{sut.Fields.Length} Columns, {sut.NumRecords} Records");

            for (int i = 0; i < sut.NumFields; i++)
            {
                DbaseFieldDescriptor fldDescriptor = sut.Fields[i];
                _testOutputHelper.WriteLine($"   {fldDescriptor.Name} {fldDescriptor.DbaseType}");
            }
        }

        [Fact(DisplayName = "Get Feature Collection")]
        public void Get_Feature_Collection()
        {
            // Arrange & Act 
            var sut = _service.GetFeatures();

            // Assert
            Assert.NotNull(sut);
            Assert.IsType<List<Feature>>(sut);

            var features = sut.Take(5);
            Assert.NotNull(features);
        }



        public void Get_Feature_Point_Lookup(double x, double y, string expected, object expected2 = null)
        {
            throw new NotImplementedException();
        }



        [InlineData("3066920018", 3368915, "803 East 17th Street")]
        [Theory(DisplayName = "Get Feature Attribute Lookup")]
        public void Get_Feature_Attribute_Lookup(object value1, object value2, string expected)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("Bbl", value1),
                new("Bin", value2),
            };

            // Act
            var sut = _service.GetFeatureLookup(attributes);
            var value = sut?.FirstOrDefault();

            // Assert
            Assert.NotNull(sut);
            Assert.Equal(expected, value?.Address);

            Assert.NotNull(value.Geometry);

            Assert.NotNull(sut);
        }

        public void Get_Feature_Point_Lookup_Not_Found(double x, double y)
        {
            throw new NotImplementedException();
        }

        public void Get_Feature_List()
        {
            throw new NotImplementedException();
        }
    }
}
