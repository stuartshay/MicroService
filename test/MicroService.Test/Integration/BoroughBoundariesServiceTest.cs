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
    public class BoroughBoundariesServiceTest : IClassFixture<ShapeServiceFixture>, IShapeTest
    {
        public IShapeService<BoroughBoundaryShape> _service;

        private readonly ITestOutputHelper _testOutputHelper;

        public BoroughBoundariesServiceTest(ShapeServiceFixture fixture, ITestOutputHelper output)
        {
            _service = fixture.BoroughBoundariesService;
            _testOutputHelper = output;
        }

        [Fact(DisplayName = "Get Shape File Properties")]
        [Trait("Category", "Integration")]
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
        [Trait("Category", "Integration")]
        public void Get_Shape_Database_Properties()
        {
            var sut = _service.GetShapeDatabaseProperties();
            Assert.NotNull(sut);

            //Display summary information about the Shape file
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
            var sut = _service.GetFeatures();
            Assert.NotNull(sut);
            Assert.IsType<List<Feature>>(sut);
        }

        [InlineData(1006187, 232036, "Bronx", 2)]
        [InlineData(1000443, 0239270, "Manhattan", 1)]
        [InlineData(1021192.9426658918, 212550.01741990919, "Queens", 4)]
        [Theory(DisplayName = "Get Feature Point Lookup")]
        public void Get_Feature_Point_Lookup(double x, double y, string expected, object expected2)
        {
            var sut = _service.GetFeatureLookup(x, y);

            Assert.NotNull(sut);
            Assert.Equal(expected, sut.BoroName);
            Assert.Equal(expected2, (int)sut.BoroCode);
        }

        [InlineData(1006187, 732036, null)]
        [Theory(DisplayName = "Get Feature Point Lookup Not Found")]
        [Trait("Category", "Integration")]
        public void Get_Feature_Point_Lookup_Not_Found(double x, double y, string expected)
        {
            var sut = _service.GetFeatureLookup(x, y);

            Assert.Null(sut);
            Assert.Equal(expected, sut?.BoroName);
        }

        [InlineData("1", "Manhattan", "Manhattan")]
        [InlineData(1, "Manhattan", "Manhattan")]
        [Theory(DisplayName = "Get Feature Attribute Lookup")]
        public void Get_Feature_Attribute_Lookup(object value1, object value2, string expected)
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                 new("BoroCode", value1),
                 new("BoroName", value2),
            };

            var sut = _service.GetFeatureLookup(attributes);
            var result = sut.FirstOrDefault();

            Assert.NotNull(sut);
            Assert.Equal(expected, result?.BoroName);
        }

        [Fact]
        public void Get_Feature_List()
        {
            var sut = _service.GetFeatureAttributes();

            Assert.NotNull(sut);
        }
    }
}