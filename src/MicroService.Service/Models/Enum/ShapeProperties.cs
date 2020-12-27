﻿using System.ComponentModel;

namespace MicroService.Service.Models.Enum
{
    public enum ShapeProperties
    {
        [Description("Borough Boundaries")]
        [ShapeAttributes("Borough_Boundaries", "nybb")]
        BoroughBoundaries,

        [Description("Historic Districts")]
        [ShapeAttributes("Historic_Districts", "Historic_Districts")]
        HistoricDistricts,

        [Description("Neighborhoods")]
        [ShapeAttributes("Neighborhoods", "nynta")]
        Neighborhoods,

        [Description("NYPD Police Precincts")]
        [ShapeAttributes("NYPD_Police_Precincts", "nypp")]
        NypdPolicePrecincts,

        [Description("NYPD Sectors")]
        [ShapeAttributes("NYPD_Sectors", "NYPD_Sectors")]
        NypdSectors,

        [Description("Park")]
        [ShapeAttributes("Park", "PARK")]
        Parks,

        [Description("Subway")]
        [ShapeAttributes("Subway_Stations", "subway")]
        Subway,

        [Description("Zip Codes")]
        [ShapeAttributes("Zip_Codes", "ZIP_CODE_040114")]
        ZipCodes,
    }
}
