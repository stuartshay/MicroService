using Xunit;
using NetTopologySuite.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Xunit.Abstractions;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;


namespace MicroService.Test.Unit
{
    public class ShapeFileTest
    {
        //https://dominoc925.blogspot.com/2013/04/using-nettopologysuite-to-read-and.html
        //https://gis.stackexchange.com/questions/165022/how-do-i-transform-a-point-using-nettopologysuite
        //https://github.com/NetTopologySuite/NetTopologySuite/issues/274#issuecomment-448283176
        //https://gis.stackexchange.com/questions/165022/how-do-i-transform-a-point-using-nettopologysuite

        //https://github.com/tangxiaodao/nettopologysuite/blob/6a9137af0ed1086ca3fe8d5212c56569689dac00/NetTopologySuite.Tests.NUnit/Performance/Geometries/Prepared/PreparedLineIntersectsPerformanceTest.cs

        private readonly ITestOutputHelper _testOutputHelper;

        public ShapeFileTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
   
        [Theory(DisplayName = "Shape OutputFeatures Borough Boundaries")]
       // [InlineData(@"Borough_Boundaries\nybb")]
        //[InlineData(@"Historic_Districts\LPC_HD_OpenData_2015March")]
        [InlineData(@"NYPD_Sectors\NYPD_Sectors")]
        //[InlineData(@"LPC_Individual_Landmark_and_Historic_Building_Database\LPC_Individual_Landmark_and_Historic_Building_Database")]
        public void Shape_OutputFeatures_Borough_Boundaries(string shapeFilePath)
        {
            string shapeDirectory = $"../../../Files/{shapeFilePath}";
            string shapePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), shapeDirectory));

            GeometryFactory factory = new GeometryFactory();
            ShapefileDataReader shapeFileDataReader = new ShapefileDataReader(shapePath, factory);

            ShapefileHeader shpHeader = shapeFileDataReader.ShapeHeader;
            _testOutputHelper.WriteLine($"Shape type: {shpHeader.ShapeType}");

            //Display the min and max bounds of the shapefile
            var bounds = shpHeader.Bounds;
            _testOutputHelper.WriteLine($"Min bounds: ({bounds.MinX},{bounds.MinY})");
            _testOutputHelper.WriteLine($"Max bounds: ({bounds.MaxX},{bounds.MaxY})");

            //Display summary information about the Dbase file
            DbaseFileHeader header = shapeFileDataReader.DbaseHeader;
            _testOutputHelper.WriteLine("Dbase info");
            _testOutputHelper.WriteLine($"{header.Fields.Length} Columns, {header.NumRecords} Records");

            for (int i = 0; i < header.NumFields; i++)
            {
                DbaseFieldDescriptor fldDescriptor = header.Fields[i];
                _testOutputHelper.WriteLine($"   {fldDescriptor.Name} {fldDescriptor.DbaseType}");
            }

            //Read through all records of the shapefile (geometry and attributes) into a feature collection 
            var features = new List<Feature>();
            while (shapeFileDataReader.Read())
            {
                Feature feature = new Feature();
                AttributesTable attributesTable = new AttributesTable();

                string[] keys = new string[header.NumFields];
                var geometry = shapeFileDataReader.Geometry;

                for (int i = 0; i < header.NumFields; i++)
                {
                    DbaseFieldDescriptor fldDescriptor = header.Fields[i];
                    keys[i] = fldDescriptor.Name;
                    
                    // First Field Geometry
                    var value = shapeFileDataReader.GetValue(i + 1);
                    attributesTable.Add(fldDescriptor.Name, value);
                }

                feature.Geometry = geometry;
                feature.Attributes = attributesTable;
                features.Add(feature);
            }

            var result = features;
            foreach (var f in features)
            {
                var z = f.Geometry.Contains(new Point(1006187, 232036));
                if (z)
                {
                    var z1 = f.Attributes["pct"];
                    //var z2 = f.Attributes["patrol_bor"];
                    //var z3 = f.Attributes["sector"];
                }
            }



        }

        private IList<IPoint> Contains(Geometry geom, List<GeoAPI.Geometries.Coordinate> cordinates)
        {
            var prepGeom = new NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory().Create(geom);
            
            var res = new List<IPoint>();
            NetTopologySuite.Geometries.Coordinate c = new NetTopologySuite.Geometries.Coordinate(232, 2323);

            var p = new Point(11111, 222222);
            var x = prepGeom.Contains(p);
            var y = x;





            //foreach (var point in cordinates)
            //{
            //    var p = new Point(point.X, point.Y);
            //    if (prepGeom.Overlaps(p))
            //    {
            //       // res.Add(point);
            //    }
            //    if (prepGeom.Contains(p))
            //    {
            //       // res.Add(point);
            //    }
            //}

            return res;
        }

        //private static Feature FindPoint(double lat, double lon)
        //{
        //    Coordinate c = new Coordinate(lat, lon);

        //    IGeometry g = factory.CreateGeometry(Geometry.DefaultFactory.CreatePoint(c));

        //    foreach (Feature f in Features)
        //    {
        //        if (f.Geometry.Overlaps(g))
        //            return f;
        //        if (f.Geometry.EnvelopeInternal.Contains(c))
        //            return f;
        //        if (f.Geometry.Boundary.Contains(g))
        //            return f;
        //        if (f.Geometry.Contains(g))
        //            return f;
        //    }
        //    return null;
        //}



        [Fact]
        public void Test_WKTReader()
        {
            WktReaderFunctions.GeometryContainsPoint();
        }


    }
}
