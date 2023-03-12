using MicroService.Service.Models.Enum.Attributes;
using System.ComponentModel;

namespace MicroService.Service.Models.Enum
{
    public enum FlatFileProperties
    {
        [Description("Subway Station Locations")]
        [FlatFile("Subway_Station_Locations", "Stations.csv", "StationFlatFile", FileTypes.Csv)]
        SubwayStationLocations,

        [Description("Subway Station Complex")]
        [FlatFile("Subway_Complex", "StationComplexes.csv", "StationComplexFlatFile", FileTypes.Csv)]
        SubwayStationComplex,

    }
}
