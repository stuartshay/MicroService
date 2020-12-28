using System.ComponentModel;

namespace MicroService.Service.Models.Enum
{
    public enum FlatFileProperties
    {
        [Description("Subway Station Locations")]
        [FlatFileAttributes("Subway_Station_Locations", "Stations.csv", FileTypes.Csv)]
        SubwayStationLocations,

        [Description("Subway Station Complex")]
        [FlatFileAttributes("Subway_Complex", "StationComplexes.csv", FileTypes.Csv)]
        SubwayStationComplex,

    }
}
