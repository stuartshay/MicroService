using System.ComponentModel;

namespace MicroService.Service.Models.Enum
{
    public enum ShapeProperties
    {
        [Description("Borough Boundaries")]
        [Shape("Borough_Boundaries", "nybb")]
        BoroughBoundaries,

        [Description("Community_Districts")]
        [Shape("Community_Districts", "nycd")]
        CommunityDistricts,

        [Description("DSNY_Districts")]
        [Shape("DSNY_Districts", "dsny")]
        DSNYDistricts,

        [Description("Historic Districts")]
        [Shape("Historic_Districts", "Historic_Districts")]
        HistoricDistricts,

        [Description("Individual Landmark Sites")]
        [Shape("Individual_Landmark_Sites", "Individual_Landmark_Sites")]
        IndividualLandmarkSite,

        [Description("Neighborhoods")]
        [Shape("Neighborhoods", "nynta")]
        Neighborhoods,

        [Description("Neighborhood Tabulation Areas 2020")]
        [Shape("Neighborhood_Tabulation_Areas", "nynta2020")]
        NeighborhoodTabulationAreas,

        [Description("NYPD Police Precincts")]
        [Shape("NYPD_Police_Precincts", "nypp")]
        NypdPolicePrecincts,

        [Description("NYPD Sectors")]
        [Shape("NYPD_Sectors", "NYPD_Sectors")]
        NypdSectors,

        [Description("NYCHA Developments")]
        [Shape("NYCHA_Developments", "NYCHA_Developments")]
        NychaDevelopments,

        [Description("Park")]
        [Shape("Park", "PARK")]
        Parks,

        [Description("Scenic Landmarks")]
        [Shape("Scenic_Landmarks", "Scenic_Landmarks")]
        ScenicLandmarks,


        [Description("Subway")]
        [Shape("Subway_Entrances", "Subway_Entrances")]
        Subway,


        [Description("Zip Codes")]
        [Shape("Zip_Codes", "ZIP_CODE_040114")]
        ZipCodes,
    }
}
