using System.Collections.Generic;
using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Test.Fixture;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Xunit;
using Xunit.Abstractions;

namespace MicroService.Test.Integration
{
    public class BoroughBoundariesServiceTest : IClassFixture<ShapeServiceFixture>
    {
        public IShapeService<BoroughBoundaryShape> _boroughBoundariesService;

        private readonly ITestOutputHelper _testOutputHelper;

        public BoroughBoundariesServiceTest(ShapeServiceFixture fixture, ITestOutputHelper output)
        {
            _boroughBoundariesService = fixture.BoroughBoundariesService;
            _testOutputHelper = output;
        }

        [Fact(Skip = "Ignore", DisplayName = "Get Shape File Properties")]
        [Trait("Category", "Integration")]
        public void Get_Shape_Properties()
        {
            var sut = _boroughBoundariesService.GetShapeProperties();
            Assert.NotNull(sut);

            _testOutputHelper.WriteLine($"Shape type: {sut.ShapeType}");

            //Display the min and max bounds of the shapefile
            var bounds = sut.Bounds;
            _testOutputHelper.WriteLine($"Min bounds: ({bounds.MinX},{bounds.MinY})");
            _testOutputHelper.WriteLine($"Max bounds: ({bounds.MaxX},{bounds.MaxY})");
        }


        [Fact(Skip = "Ignore", DisplayName = "Get Shape File Database Properties")]
        [Trait("Category", "Integration")]
        public void Get_Shape_Database_Properties()
        {
            var sut = _boroughBoundariesService.GetShapeDatabaseProperties();
            Assert.NotNull(sut);

            //Display summary information about the Dbase file
            _testOutputHelper.WriteLine("Dbase info");
            _testOutputHelper.WriteLine($"{sut.Fields.Length} Columns, {sut.NumRecords} Records");

            for (int i = 0; i < sut.NumFields; i++)
            {
                DbaseFieldDescriptor fldDescriptor = sut.Fields[i];
                _testOutputHelper.WriteLine($"   {fldDescriptor.Name} {fldDescriptor.DbaseType}");
            }
        }

        [Fact(Skip = "Ignore", DisplayName = "Get Borough Boundaries Feature List")]
        [Trait("Category", "Integration")]
        public void Get_Borough_Boundaries_Feature_Collection()
        {
            var sut = _boroughBoundariesService.GetFeatures();
            Assert.NotNull(sut);
            Assert.IsType<List<Feature>>(sut);
        }

        [InlineData(1006187, 232036, "Bronx")]
        //[InlineData(1000443, 0239270, "Manhattan")]
        [Theory(Skip = "Ignore", DisplayName = "Get Feature Point Lookup")]
        [Trait("Category", "Integration")]
        public void Get_Borough_Boundaries_Feature_Lookup(double x, double y, string expected)
        {
            var sut = _boroughBoundariesService.GetFeatureLookup(x, y);

            Assert.NotNull(sut);
            Assert.Equal(expected, sut.BoroName);
        }
    }
}
