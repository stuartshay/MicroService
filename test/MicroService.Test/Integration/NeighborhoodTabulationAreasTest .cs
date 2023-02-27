﻿using MicroService.Service.Interfaces;
using MicroService.Service.Models;
using MicroService.Test.Fixture;
using MicroService.Test.Integration.Interfaces;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Xunit;
using Xunit.Abstractions;

namespace MicroService.Test.Integration
{
    public class NeighborhoodTabulationAreasTest : IClassFixture<ShapeServiceFixture>, IShapeTest
    {
        public IShapeService<NeighborhoodTabulationAreaShape> _service;

        private readonly ITestOutputHelper _testOutputHelper;

        public NeighborhoodTabulationAreasTest(ShapeServiceFixture fixture, ITestOutputHelper output)
        {
            _service = fixture.NeighborhoodTabulationAreasService;
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

        [Fact(DisplayName = "Get Borough Boundaries Feature List")]
        [Trait("Category", "Integration")]
        public void Get_Borough_Boundaries_Feature_Collection()
        {
            var sut = _service.GetFeatures();
            Assert.NotNull(sut);
            Assert.IsType<List<Feature>>(sut);
        }


        [InlineData(1006187, 232036, "Bronx", null)]
        [InlineData(1000443, 0239270, "Manhattan", null)]
        [Theory(DisplayName = "Get Feature Point Lookup")]
        [Trait("Category", "Integration")]
        public void Get_Feature_Point_Lookup(double x, double y, string expected, int? lookupExpected)
        {
            var sut = _service.GetFeatureLookup(x, y);

            Assert.NotNull(sut);
            Assert.Equal(expected, sut.BoroName);
        }

        [InlineData("12", "MN1202", "MN12 Washington Heights-Inwood (CD 12 Equivalent)")]
        [InlineData(12, "MN1202", "MN12 Washington Heights-Inwood (CD 12 Equivalent)")]
        [Theory(DisplayName = "Get Feature Attribute Lookup")]
        public void Get_Feature_Attribute_Lookup(object value1, object value2, string expected)
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("BoroCode", value1),
                new("NTA2020", value2),
            };

            var sut = _service.GetFeatureLookup(attributes);
            var result = sut.FirstOrDefault();

            Assert.NotNull(sut);
            Assert.Equal(expected, result?.CDTAName);
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

        [Fact]
        public void Get_Feature_List()
        {
            var sut = _service.GetFeatureAttributes();

            Assert.NotNull(sut);
        }


    }
}