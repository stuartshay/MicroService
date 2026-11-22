using MicroService.Service.Models.Enum.Attributes;
using System.ComponentModel;

namespace MicroService.Service.Models.Enum
{
    public enum ShapeProperties
    {
        [Description("Borough Boundaries")]
        [Shape("Borough_Boundaries/WGS84", "nybb", Datum.Wgs84)]
        [ShapeStyle(Color.Blue)]
        BoroughBoundaries,

        [Description("Community_Districts")]
        [Shape("Community_Districts", "nycd", Datum.Nad83)]
        [ShapeStyle(Color.Blue)]
        CommunityDistricts,

        [Description("DSNY_Districts")]
        [Shape("DSNY_Districts/WGS84", "dsny", Datum.Wgs84)]
        [ShapeStyle(Color.Blue)]
        DSNYDistricts,

        [Description("Historic Districts")]
        [Shape("Historic_Districts/WGS84", "Historic_Districts", Datum.Wgs84)]
        [ShapeStyle(Color.Red)]
        HistoricDistricts,

        [Description("Individual Landmark Sites")]
        [Shape("Individual_Landmark_Sites", "Individual_Landmark_Sites", Datum.Wgs84)]
        [ShapeStyle(Color.Blue)]
        IndividualLandmarkSite,

        [Description("Individual Landmark & Historic Districts")]
        [Shape("LPC_Individual_Landmark_and_Historic_Building_Database", "LPC_Individual_Landmark_and_Historic_District_Building_Database_20231209", Datum.Wgs84)]
        [ShapeStyle(Color.Blue)]
        IndividualLandmarkHistoricDistricts,

        [Description("National Register Historic_Places")]
        [Shape("National_Register_ Historic_Places/WGS84", "HRHP", Datum.Wgs84)]
        [ShapeStyle(Color.Blue)]
        NationalRegisterHistoricPlaces,

        [Description("Neighborhoods")]
        [Shape("Neighborhoods", "nynta", Datum.Nad83)]
        [ShapeStyle(Color.Blue)]
        Neighborhoods,

        [Description("Neighborhood Tabulation Areas 2020")]
        [Shape("Neighborhood_Tabulation_Areas", "nynta2020", Datum.Nad83)]
        [ShapeStyle(Color.Blue)]
        NeighborhoodTabulationAreas,

        [Description("NYPD Police Precincts")]
        [Shape("NYPD_Police_Precincts", "nypp", Datum.Nad83)]
        [ShapeStyle(Color.Blue)]
        NypdPolicePrecincts,

        [Description("NYPD Sectors")]
        [Shape("NYPD_Sectors", "NYPD_Sectors", Datum.Nad83)]
        [ShapeStyle(Color.Blue)]
        NypdSectors,

        [Description("NYCHA Developments")]
        [Shape("NYCHA_Developments", "NYCHA_Developments", Datum.Nad83)]
        [ShapeStyle(Color.Blue)]
        NychaDevelopments,

        [Description("Park")]
        [Shape("Park", "PARK", Datum.Nad83)]
        [ShapeStyle(Color.Blue)]
        Parks,

        [Description("Scenic Landmarks")]
        [Shape("Scenic_Landmarks/WGS84", "Scenic_Landmarks", Datum.Wgs84)]
        [ShapeStyle(Color.Blue)]
        ScenicLandmarks,

        [Description("Subway")]
        [Shape("Subway_Entrances", "Subway_Entrances", Datum.Wgs84)]
        [ShapeStyle(Color.Blue)]
        Subway,

        [Description("Zip Codes")]
        [Shape("Zip_Codes", "ZIP_CODE_040114", Datum.Nad83)]
        [ShapeStyle(Color.Blue)]
        ZipCodes,

        // Last, so the numbers of the members above stay as they were (the endpoints accept them)
        // The MTA's subway service lines (data.ny.gov s692-irgq): one line per service,
        // served shaped for maps by /api/v1/Subway/lines
        [Description("Subway Service Lines")]
        [Shape("Subway_Service_Lines", "Subway_Service_Lines", Datum.Wgs84)]
        [ShapeStyle(Color.Blue)]
        SubwayServiceLines,

        // Map layers (docs/map-layers-plan.md), exported from NYC Open Data in WGS 84 by
        // scripts/refresh-shapes.py
        [Description("Privately Owned Public Spaces")]
        [Shape("Privately_Owned_Public_Spaces/WGS84", "POPS", Datum.Wgs84)]
        [ShapeStyle(Color.Yellow)]
        PrivatelyOwnedPublicSpaces,

        [Description("Greenstreets")]
        [Shape("Greenstreets/WGS84", "Greenstreets", Datum.Wgs84)]
        [ShapeStyle(Color.Green)]
        Greenstreets,

        [Description("Libraries")]
        [Shape("Libraries/WGS84", "Libraries", Datum.Wgs84)]
        [ShapeStyle(Color.Blue)]
        Libraries,

        [Description("Post Offices")]
        [Shape("Post_Offices/WGS84", "Post_Offices", Datum.Wgs84)]
        [ShapeStyle(Color.Red)]
        PostOffices,
    }
}
