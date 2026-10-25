using MicroService.Service.Mappings;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Xunit;

namespace MicroService.Test.Mappings
{
    public class ShapeMappingsTests
    {
        private static readonly Point DefaultGeometry = new(1, 2);
        private static readonly Envelope DefaultBounds = new(-74.0, -73.9, 40.7, 40.8);

        private static Feature CreateFeature(IDictionary<string, object?> attributes) =>
            new(DefaultGeometry, new AttributesTable(attributes!))
            {
                BoundingBox = DefaultBounds,
            };

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void BoroughBoundaryShapeProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "BoroCode", "1" },
                { "BoroName", "Manhattan" },
                { "Shape_Area", "10.5" },
                { "Shape_Leng", "20.5" },
            });

            var result = BoroughBoundaryShapeProfile.Map(feature);

            Assert.Equal(1, result.BoroCode);
            Assert.Equal("Manhattan", result.BoroName);
            Assert.Equal(10.5, result.ShapeArea);
            Assert.Equal(20.5, result.ShapeLength);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void CommunityDistrictShapeProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "BoroCD", "101" },
                { "Shape_Area", "10.5" },
                { "Shape_Leng", "20.5" },
            });

            var result = CommunityDistrictShapeProfile.Map(feature);

            Assert.Equal(1, result.Cd);
            Assert.Equal(101, result.BoroCd);
            Assert.Equal(1, result.BoroCode);
            Assert.Equal("MN", result.Borough);
            Assert.Equal("Manhattan", result.BoroName);
            Assert.Equal(10.5, result.ShapeArea);
            Assert.Equal(20.5, result.ShapeLength);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void DsnyDistrictsShapeProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "district", "MN01" },
                { "districtco", "101" },
                { "fid", "1" },
                { "globalid", "gid-1" },
                { "shape_area", "10.5" },
                { "shape_leng", "20.5" },
            });

            var result = DsnyDistrictsShapeProfile.Map(feature);

            Assert.Equal("MN01", result.District);
            Assert.Equal(101, result.DistrictCode);
            Assert.Equal("1", result.Fid);
            Assert.Equal("gid-1", result.GlobalId);
            Assert.Equal("MN", result.OperationZone);
            Assert.Equal("Manhattan", result.OperationZoneName);
            Assert.Equal(10.5, result.ShapeArea);
            Assert.Equal(20.5, result.ShapeLength);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void HistoricDistrictShapeProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "lp_number", "LP-00099" },
                { "area_name", "Brooklyn Heights Historic District" },
                { "borough", "BK" },
                { "boundary_n", "Boundary" + '\u0000' },
                { "caldate", "1965-01-01" },
                { "current_", "Yes" },
                { "desdate", "1965-11-23" },
                { "extension", "No" },
                { "last_actio", "DESIGNATED" },
                { "other_hear", null },
                { "public_hea", "1965-10-01" },
                { "status_of_", "DESIGNATED" },
                { "shape_area", "100.5" },
                { "shape_leng", "50.5" },
            });

            var result = HistoricDistrictShapeProfile.Map(feature);

            Assert.Equal("LP-00099", result.LPNumber);
            Assert.Equal("Brooklyn Heights Historic District", result.AreaName);
            Assert.Equal("BK", result.BoroName);
            Assert.Equal(3, result.BoroCode);
            Assert.Equal("Boundary", result.BoundaryName);
            Assert.Equal("1965-01-01", result.CalendarDate);
            Assert.Equal("Yes", result.Current);
            Assert.Equal("1965-11-23", result.DesignationDate);
            Assert.Equal("No", result.Extension);
            Assert.Equal("DESIGNATED", result.LastAction);
            Assert.Null(result.OtherHearing);
            Assert.Equal("1965-10-01", result.PublicHearing);
            Assert.Equal("DESIGNATED", result.Status);
            Assert.Equal(100.5, result.ShapeArea);
            Assert.Equal(50.5, result.ShapeLength);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void IndividualLandmarkHistoricDistrictsShapeProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "bin", "3000001" },
                { "bbl", "3066920018" },
                { "borough", "BK" },
                { "block", "6692" },
                { "lot", "18" },
                { "des_addres", "803 East 17th Street" },
                { "circa", "1" },
                { "date_low", "1912" },
                { "date_high", "1912" },
                { "date_combo", "c. 1912" },
                { "alt_date_1", "1958" },
                { "alt_date_2", "0" },
                { "arch_build", "Architect" },
                { "own_devel", "Owner" },
                { "alt_arch_1", "AltArch1" },
                { "alt_arch_2", "0" },
                { "altered", "1" },
                { "style_prim", "Colonial" },
                { "style_sec", "0" },
                { "style_oth", "0" },
                { "mat_prim", "Wood" },
                { "mat_sec", "Brick" },
                { "mat_third", "0" },
                { "mat_four", "0" },
                { "mat_other", "0" },
                { "use_orig", "Residential" },
                { "use_other", "0" },
                { "build_type", "House" },
                { "build_oth", "0" },
                { "build_nme", "Wyckoff" },
                { "notes", "None" },
                { "hist_dist", "Historic District" },
                { "lm_new", "0" },
                { "lm_orig", "Pieter Claesen Wyckoff House" },
                { "shape_area", "373.0" },
                { "shape_leng", "77.3" },
            });

            var result = IndividualLandmarkHistoricDistrictsShapeProfile.Map(feature);

            Assert.Equal(3000001d, result.Bin);
            Assert.Equal("3066920018", result.Bbl);
            Assert.Equal("BK", result.BoroName);
            Assert.Equal(0, result.BoroCode);
            Assert.Equal(6692d, result.Block);
            Assert.Equal(18d, result.Lot);
            Assert.Equal("803 East 17th Street", result.Address);
            Assert.Equal(1, result.Circa);
            Assert.Equal(1912, result.DateLow);
            Assert.Equal(1912, result.DateHigh);
            Assert.Equal(1, result.Altered);
            Assert.Equal("Pieter Claesen Wyckoff House", result.AreaName);
            Assert.Equal(string.Empty, result.LPNumber);
            Assert.Equal(373.0, result.ShapeArea);
            Assert.Equal(77.3, result.ShapeLength);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void IndividualLandmarkSiteShapeProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "lpc_lpnumb", "LP-00001" },
                { "lpc_name", "Wyckoff House" },
                { "borough", "BK" },
                { "bbl", "3079170009" },
                { "address", "5816 Clarendon Road" },
                { "block", "7917" },
                { "lot", "9" },
                { "objectid", "1" },
                { "date_des_d", "1965-10-14" },
                { "lpc_altern", "Alt" + '\u0000' + "Name" },
                { "lpc_site_d", "Site" },
                { "landmark_t", "Individual Landmark" },
                { "lpc_site_s", "Designated" },
                { "url_report", "http://example.com/report.pdf" },
                { "shape_area", "10.0" },
                { "shape_leng", "20.0" },
            });

            var result = IndividualLandmarkSiteShapeProfile.Map(feature);

            Assert.Equal("LP-00001", result.LPNumber);
            Assert.Equal("Wyckoff House", result.AreaName);
            Assert.Equal(3, result.BoroCode);
            Assert.Equal("BK", result.BoroName);
            Assert.Equal(3079170009d, result.BBL);
            Assert.Equal("AltName", result.AlternativeName);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void NationalRegisterHistoricPlacesShapeProfile_MapsExpectedProperties(string culture)
        {
            var previousCulture = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(culture);
                var feature = CreateFeature(new Dictionary<string, object?>
                {
                    { "resource_n", "Ocean Parkway" },
                    { "county", "Kings" },
                    { "date_natio", "1983-09-08" },
                    { "time_natio", "00:00:00.000" },
                    { "sphinx_num", "90NR01303" },
                    { "x", "-73.9687673796" },
                    { "y", "40.6108841936" },
                });

                var result = NationalRegisterHistoricPlacesShapeProfile.Map(feature);

                Assert.Equal("Ocean Parkway", result.ResourceName);
                Assert.Equal("Kings", result.County);
                Assert.Equal(new DateTime(1983, 9, 8), result.DateDesignated);
                Assert.Equal("00:00:00.000", result.TimeDesignated);
                Assert.Equal("90NR01303", result.SphinxNumber);
                Assert.Equal(-73.9687673796d, result.X);
                Assert.Equal(40.6108841936d, result.Y);
                Assert.Same(DefaultGeometry, result.Geometry);
                Assert.NotNull(result.BoundingBox);
                Assert.Null(result.Feature);
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = previousCulture;
            }
        }

        [Fact]
        public void NationalRegisterHistoricPlacesShapeProfile_HandlesNullDateAndNulPaddedTime()
        {
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "resource_n", "Gowanus Canal Historic District" },
                { "county", "Kings" },
                { "date_natio", null },
                { "time_natio", new string('\u0000', 12) },
                { "sphinx_num", "12NR06417" },
                { "x", "-73.989" },
                { "y", "40.677" },
            });

            var result = NationalRegisterHistoricPlacesShapeProfile.Map(feature);

            Assert.Equal("Gowanus Canal Historic District", result.ResourceName);
            Assert.Equal("Kings", result.County);
            Assert.Equal(default, result.DateDesignated);
            Assert.Equal(string.Empty, result.TimeDesignated);
            Assert.Equal("12NR06417", result.SphinxNumber);
            Assert.Equal(-73.989d, result.X);
            Assert.Equal(40.677d, result.Y);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void NeighborhoodShapeMappingsProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "BoroCode", "1" },
                { "BoroName", "Manhattan" },
                { "CountyFIPS", "061" },
                { "NTACode", "MN40" },
                { "NTAName", "Upper East Side-Carnegie Hill" },
                { "Shape_Area", "10.0" },
                { "Shape_Leng", "20.0" },
            });

            var result = NeighborhoodShapeMappingsProfile.Map(feature);

            Assert.Equal(1, result.BoroCode);
            Assert.Equal("Manhattan", result.BoroName);
            Assert.Equal("061", result.CountyFIPS);
            Assert.Equal("MN40", result.NTACode);
            Assert.Equal("Upper East Side-Carnegie Hill", result.NTAName);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void NeighborhoodTabulationAreaShapeProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "BoroCode", "3" },
                { "BoroName", "Brooklyn" },
                { "CountyFIPS", "047" },
                { "NTA2020", "BK0101" },
                { "NTAName", "Greenpoint" },
                { "NTAAbbrev", "Greenpoint" },
                { "NTAType", "0" },
                { "CDTA2020", "BK01" },
                { "CDTAName", "BK01 Williamsburg-Greenpoint" },
                { "Shape_Area", "10.0" },
                { "Shape_Leng", "20.0" },
            });

            var result = NeighborhoodTabulationAreaShapeProfile.Map(feature);

            Assert.Equal(3, result.BoroCode);
            Assert.Equal("Brooklyn", result.BoroName);
            Assert.Equal("047", result.CountyFIPS);
            Assert.Equal("BK0101", result.NTA2020);
            Assert.Equal("Greenpoint", result.NTAName);
            Assert.Equal("Greenpoint", result.NTAAbbrev);
            Assert.Equal(0, result.NTAType);
            Assert.Equal("BK01", result.CDTA2020);
            Assert.Equal("BK01 Williamsburg-Greenpoint", result.CDTAName);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void NychaDevelopmentShapeProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "DEVELOPMEN", "ASTORIA" },
                { "TDS_NUM", "026" },
                { "BOROUGH", "QUEENS" },
            });

            var result = NychaDevelopmentShapeProfile.Map(feature);

            Assert.Equal("ASTORIA", result.Development);
            Assert.Equal("026", result.TdsNumber);
            Assert.Equal("QUEENS", result.Borough);
            Assert.Equal(0, result.ShapeArea);
            Assert.Equal(0, result.ShapeLength);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void NypdPrecinctShapeProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "Precinct", "1" },
                { "Shape_Area", "10.0" },
                { "Shape_Leng", "20.0" },
            });

            var result = NypdPrecinctShapeProfile.Map(feature);

            Assert.Equal(1, result.Precinct);
            Assert.Equal(10.0, result.ShapeArea);
            Assert.Equal(20.0, result.ShapeLength);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void NypdSectorShapeProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "pct", "102" },
                { "sector", "102A" },
                { "patrol_bor", "PBQS" },
                { "phase", "14" },
            });

            var result = NypdSectorShapeProfile.Map(feature);

            Assert.Equal("102", result.Pct);
            Assert.Equal("102A", result.Sector);
            Assert.Equal("PBQS", result.PatrolBoro);
            Assert.Equal("14", result.Phase);
            Assert.Equal(0, result.ShapeArea);
            Assert.Equal(0, result.ShapeLength);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void ParkShapeProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "PARK_NAME", "Central Park" },
                { "PARKNUM", "M010" },
                { "SOURCE_ID", "21491000001" },
                { "FEAT_CODE", "4910" },
                { "SUB_CODE", "491050" },
                { "LANDUSE", "Community Park" },
                { "SYSTEM", "System1" },
                { "STATUS", "Updated" },
                { "SHAPE_Area", "10.0" },
                { "SHAPE_Leng", "20.0" },
            });

            var result = ParkShapeProfile.Map(feature);

            Assert.Equal("Central Park", result.ParkName);
            Assert.Equal("M010", result.ParkNumber);
            Assert.Equal(21491000001L, result.SourceId);
            Assert.Equal(4910, result.FeatureCode);
            Assert.Equal(491050, result.SubCode);
            Assert.Equal("Community Park", result.LandUse);
            Assert.Equal("System1", result.System);
            Assert.Equal("Updated", result.Status);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void FeatureToScenicLandmarkShapeProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "lp_number", "LP-00879" },
                { "scen_lm_na", "Bryant Park" },
                { "borough", "MN" },
                { "shape_area", "10.0" },
                { "shape_leng", "20.0" },
            });

            var result = FeatureToScenicLandmarkShapeProfile.Map(feature);

            Assert.Equal("LP-00879", result.LPNumber);
            Assert.Equal("Bryant Park", result.AreaName);
            Assert.Equal("MN", result.BoroName);
            Assert.Equal(1, result.BoroCode);
            Assert.Equal(10.0, result.ShapeArea);
            Assert.Equal(20.0, result.ShapeLength);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void SubwayShapeProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "line", "4-6" },
                { "name", "Astor Pl" },
                { "objectid", "1" },
                { "url", "http://example.com" },
            });

            var result = SubwayShapeProfile.Map(feature);

            Assert.Equal("4-6", result.Line);
            Assert.Equal("Astor Pl", result.Name);
            Assert.Equal(1d, result.ObjectId);
            Assert.Equal(string.Empty, result.Url);
            Assert.Equal(0d, result.Distance);
            Assert.Equal(0d, result.ShapeArea);
            Assert.Equal(0d, result.ShapeLength);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("ar-SA")]
        public void ZipCodeMappingsProfile_MapsExpectedProperties(string culture)
        {
            using var cultureScope = new CultureScope(culture);
            var feature = CreateFeature(new Dictionary<string, object?>
            {
                { "ZIPCODE", "10001" },
                { "BLDGZIP", "0" },
                { "PO_NAME", "New York" },
                { "POPULATION", "21102" },
                { "STATE", "NY" },
                { "COUNTY", "New York" },
                { "AREA", "17794941.5" },
                { "ST_FIPS", "36" },
                { "CTY_FIPS", "061" },
                { "URL", "http://www.usps.com/" },
                { "SHAPE_AREA", "10.0" },
                { "SHAPE_LEN", "20.0" },
            });

            var result = ZipCodeMappingsProfile.Map(feature);

            Assert.Equal("10001", result.ZipCode);
            Assert.Equal("0", result.BldgZip);
            Assert.Equal("New York", result.PostOfficeName);
            Assert.Equal(21102, result.Population);
            Assert.Equal("NY", result.State);
            Assert.Equal("New York", result.County);
            Assert.Equal(17794941.5, result.Area);
            Assert.Equal("36", result.StateFibs);
            Assert.Equal("061", result.CityFibs);
            Assert.Equal("http://www.usps.com/", result.Url);
            Assert.Equal(10.0, result.ShapeArea);
            Assert.Equal(20.0, result.ShapeLength);
            Assert.Same(DefaultGeometry, result.Geometry);
            Assert.NotNull(result.BoundingBox);
            Assert.Null(result.Feature);
        }
        private sealed class CultureScope : IDisposable
        {
            private readonly System.Globalization.CultureInfo _previous = System.Globalization.CultureInfo.CurrentCulture;
            public CultureScope(string culture) =>
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(culture);
            public void Dispose() => System.Globalization.CultureInfo.CurrentCulture = _previous;
        }
    }
}
