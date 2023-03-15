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
    public class NypdPolicePrecinctServiceTest : IClassFixture<ShapeServiceFixture>, IShapeTest
    {
        public IShapeService<NypdPrecinctShape> _service;

        private readonly ITestOutputHelper _testOutputHelper;

        public NypdPolicePrecinctServiceTest(ShapeServiceFixture fixture, ITestOutputHelper output)
        {
            _service = fixture.NypdPolicePrecinctService;
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


        [InlineData(1006187, 232036, "40", 0)]
        [InlineData(1000443, 0239270, "32", 0)]
        [InlineData(1021192.9426658918, 212550.01741990919, "115", 0)]
        [Theory(DisplayName = "Get Geospatial Point Lookup")]
        public void Get_Geospatial_Point_Lookup(double x, double y, string expected, object lookupExpected)
        {
            var sut = _service.GetFeatureLookup(x, y);

            Assert.NotNull(sut);
            Assert.Equal(int.Parse(expected), sut.Precinct);
        }


        [InlineData("14", "", "14")]
        [Theory(Skip = "TODO FIX - Not Filtering", DisplayName = "Get Feature Attribute Lookup")]
        public void Get_Feature_Attribute_Lookup(object value1, object value2, string expected)
        {
            // var value = Convert.ToDouble(value1);
            // var value = Convert.ToString(value1);
            var attributes = new List<KeyValuePair<string, object>>
            {
                new("Precinct", value1),
            };

            var sut = _service.GetFeatureLookup(attributes);
            var result = sut.FirstOrDefault();

            Assert.NotNull(sut);
            Assert.Equal(int.Parse(expected), result?.Precinct);
        }

        [InlineData(1006187, 732036)]
        [Theory(DisplayName = "Get Geospatial Point Lookup Not Found")]
        public void Get_Geospatial_Point_Lookup_Not_Found(double x, double y)
        {
            var sut = _service.GetFeatureLookup(x, y);

            Assert.Null(sut);
        }

        [Fact]
        public void Get_Feature_List()
        {
            var sut = _service.GetFeatureList();

            Assert.NotNull(sut);
        }
    }
}