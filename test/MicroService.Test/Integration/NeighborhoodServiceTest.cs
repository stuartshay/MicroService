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
    public class NeighborhoodServiceTest : IClassFixture<ShapeServiceFixture>, IShapeTest
    {
        public IShapeService<NeighborhoodShape> _service;

        private readonly ITestOutputHelper _testOutputHelper;

        public NeighborhoodServiceTest(ShapeServiceFixture fixture, ITestOutputHelper output)
        {
            _service = fixture.NeighborhoodService;
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

        [InlineData(1006187, 232036, "Bronx", "BX39")]
        [InlineData(1000443, 0239270, "Manhattan", "MN03")]
        [InlineData(1021192.9426658918, 212550.01741990919, "Queens", "QN26")]
        [Theory(DisplayName = "Get Feature Point Lookup")]
        [Trait("Category", "Integration")]
        public void Get_Feature_Point_Lookup(double x, double y, string expected, object expected2)
        {
            var sut = _service.GetFeatureLookup(x, y);

            Assert.NotNull(sut);
            Assert.Equal(expected, sut.BoroName);
            Assert.Equal(expected2, sut.NTACode);
        }

        [InlineData("1", "MN40", "Upper East Side-Carnegie Hill")]
        [InlineData(1, "MN40", "Upper East Side-Carnegie Hill")]
        [InlineData("1", "MN15", "Clinton")]
        [InlineData("1", "MN25", "Battery Park City-Lower Manhattan")]
        [Theory(DisplayName = "Get Feature Attribute Lookup")]
        public void Get_Feature_Attribute_Lookup(object value1, object value2, string expected)
        {
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("BoroCode", value1),
                new("NTACode", value2),
            };

            var sut = _service.GetFeatureLookup(attributes);
            var result = sut.FirstOrDefault();

            Assert.NotNull(sut);
            Assert.Equal(expected, result?.NTAName);
        }

        [InlineData(1006187, 732036)]
        [Theory(DisplayName = "Get Feature Point Lookup Not Found")]
        [Trait("Category", "Integration")]
        public void Get_Feature_Point_Lookup_Not_Found(double x, double y)
        {
            var sut = _service.GetFeatureLookup(x, y);

            Assert.Null(sut);
        }


        [InlineData("1", "MN15", "Clinton")]
        [InlineData("1", "MN25", "Battery Park City-Lower Manhattan")]
        [InlineData("1", "MN40", "Upper East Side-Carnegie Hill")]
        [Theory(DisplayName = "GetFeatureCollection returns expected feature collection")]
        public void GetFeatureCollection_ValidInput_ReturnsExpectedFeature(string value1, string value2, string expected)
        {
            // Arrange
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("BoroCode", value1),
                new("NTACode", value2),
            };

            // Act
            var sut = _service.GetFeatureCollection(attributes);
            var result = sut.Single();

            // Assert
            Assert.NotNull(sut);
            Assert.IsType<FeatureCollection>(sut);
            Assert.NotNull(result);

            Assert.Equal(expected, result.Attributes["NTAName"]);
        }

        [Fact]
        public void Get_Feature_List()
        {
            var sut = _service.GetFeatureList();

            Assert.NotNull(sut);
        }

    }
}