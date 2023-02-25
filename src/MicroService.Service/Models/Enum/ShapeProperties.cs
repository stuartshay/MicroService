using System.ComponentModel;

namespace MicroService.Service.Models.Enum
{
    public enum ShapeProperties
    {
        [Description("Borough Boundaries")]
        [ShapeAttribute("Borough_Boundaries", "nybb")]
        BoroughBoundaries,

        [Description("Community_Districts")]
        [ShapeAttribute("Community_Districts", "nycd")]
        CommunityDistricts,

        [Description("DSNY_Districts")]
        [ShapeAttribute("DSNY_Districts", "dsny")]
        DSNYDistricts,

        [Description("Historic Districts")]
        [ShapeAttribute("Historic_Districts", "Historic_Districts")]
        HistoricDistricts,

        [Description("Individual Landmark Sites")]
        [ShapeAttribute("Individual_Landmark_Sites", "Individual_Landmark_Sites")]
        IndividualLandmarkSite,

        [Description("Neighborhoods")]
        [ShapeAttribute("Neighborhoods", "nynta")]
        Neighborhoods,

        [Description("Neighborhood Tabulation Areas 2020")]
        [ShapeAttribute("Neighborhood_Tabulation_Areas", "nynta2020")]
        NeighborhoodTabulationAreas,

        [Description("NYPD Police Precincts")]
        [ShapeAttribute("NYPD_Police_Precincts", "nypp")]
        NypdPolicePrecincts,

        [Description("NYPD Sectors")]
        [ShapeAttribute("NYPD_Sectors", "NYPD_Sectors")]
        NypdSectors,

        [Description("NYCHA Developments")]
        [ShapeAttribute("NYCHA_Developments", "NYCHA_Developments")]
        NychaDevelopments,

        [Description("Park")]
        [ShapeAttribute("Park", "PARK")]
        Parks,

        [Description("Scenic Landmarks")]
        [ShapeAttribute("Scenic_Landmarks", "Scenic_Landmarks")]
        ScenicLandmarks,


        [Description("Subway")]
        [ShapeAttribute("Subway_Entrances", "Subway_Entrances")]
        Subway,


        [Description("Zip Codes")]
        [ShapeAttribute("Zip_Codes", "ZIP_CODE_040114")]
        ZipCodes,
    }
}
