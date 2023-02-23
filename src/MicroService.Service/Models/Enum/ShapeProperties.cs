using System.ComponentModel;

namespace MicroService.Service.Models.Enum
{
    public enum ShapeProperties
    {
        [Description("Borough Boundaries")]
        [ShapeAttributes("Borough_Boundaries", "nybb")]
        BoroughBoundaries,

        [Description("Community_Districts")]
        [ShapeAttributes("Community_Districts", "nycd")]
        CommunityDistricts,

        [Description("DSNY_Districts")]
        [ShapeAttributes("DSNY_Districts", "dsny")]
        DSNYDistricts,

        [Description("Historic Districts")]
        [ShapeAttributes("Historic_Districts", "Historic_Districts")]
        HistoricDistricts,

        [Description("Neighborhoods")]
        [ShapeAttributes("Neighborhoods", "nynta")]
        Neighborhoods,

        [Description("Neighborhood Tabulation Areas 2020")]
        [ShapeAttributes("Neighborhood_Tabulation_Areas", "nynta2020")]
        NeighborhoodTabulationAreas,

        [Description("NYPD Police Precincts")]
        [ShapeAttributes("NYPD_Police_Precincts", "nypp")]
        NypdPolicePrecincts,

        [Description("NYPD Sectors")]
        [ShapeAttributes("NYPD_Sectors", "NYPD_Sectors")]
        NypdSectors,

        [Description("NYCHA Developments")]
        [ShapeAttributes("NYCHA_Developments", "NYCHA_Developments")]
        NychaDevelopments,

        [Description("Park")]
        [ShapeAttributes("Park", "PARK")]
        Parks,

        [Description("Scenic Landmarks")]
        [ShapeAttributes("Scenic_Landmarks", "Scenic_Landmarks")]
        ScenicLandmarks,


        [Description("Subway")]
        [ShapeAttributes("Subway_Entrances", "Subway_Entrances")]
        Subway,


        [Description("Zip Codes")]
        [ShapeAttributes("Zip_Codes", "ZIP_CODE_040114")]
        ZipCodes,
    }
}
