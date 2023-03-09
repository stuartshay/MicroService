using System.ComponentModel;

namespace MicroService.Service.Models.Enum
{
    public enum ShapeProperties
    {
        [Description("Borough Boundaries")]
        [Shape("Borough_Boundaries/NAD83", "nybb", Datum.Nad83)]
        BoroughBoundaries,

        [Description("Community_Districts")]
        [Shape("Community_Districts", "nycd", Datum.Nad83)]
        CommunityDistricts,

        [Description("DSNY_Districts")]
        [Shape("DSNY_Districts/WGS84", "dsny", Datum.Wgs84)]
        DSNYDistricts,

        [Description("Historic Districts")]
        [Shape("Historic_Districts/WGS84", "Historic_Districts", Datum.Wgs84)]
        HistoricDistricts,

        [Description("Individual Landmark Sites")]
        [Shape("Individual_Landmark_Sites", "Individual_Landmark_Sites", Datum.Wgs84)]
        IndividualLandmarkSite,

        [Description("Individual Landmark & Historic Districts")]
        [Shape("LPC_Individual_Landmark_and_Historic_Building_Database", "LPC_Individual_Landmark_and_Historic_District_Building_Database_202302", Datum.Wgs84)]
        IndividualLandmarkHistoricDistricts,

        [Description("National Register Historic_Places")]
        [Shape("National_Register_ Historic_Places/WGS84", "HRHP", Datum.Wgs84)]
        NationalRegisterHistoricPlaces,

        [Description("Neighborhoods")]
        [Shape("Neighborhoods", "nynta", Datum.Nad83)]
        Neighborhoods,

        [Description("Neighborhood Tabulation Areas 2020")]
        [Shape("Neighborhood_Tabulation_Areas", "nynta2020", Datum.Nad83)]
        NeighborhoodTabulationAreas,

        [Description("NYPD Police Precincts")]
        [Shape("NYPD_Police_Precincts", "nypp", Datum.Nad83)]
        NypdPolicePrecincts,

        [Description("NYPD Sectors")]
        [Shape("NYPD_Sectors", "NYPD_Sectors", Datum.Nad83)]
        NypdSectors,

        [Description("NYCHA Developments")]
        [Shape("NYCHA_Developments", "NYCHA_Developments", Datum.Nad83)]
        NychaDevelopments,

        [Description("Park")]
        [Shape("Park", "PARK", Datum.Nad83)]
        Parks,

        [Description("Scenic Landmarks")]
        [Shape("Scenic_Landmarks/WGS84", "Scenic_Landmarks", Datum.Wgs84)]
        ScenicLandmarks,


        [Description("Subway")]
        [Shape("Subway_Entrances", "Subway_Entrances", Datum.Nad83)]
        Subway,


        [Description("Zip Codes")]
        [Shape("Zip_Codes", "ZIP_CODE_040114", Datum.Nad83)]
        ZipCodes,
    }
}
