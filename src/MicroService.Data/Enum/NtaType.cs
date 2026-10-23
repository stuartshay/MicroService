using System.ComponentModel;

namespace MicroService.Data.Enum
{
    public enum NtaType
    {
        [Description("Residential")]
        Residential = 0,

        [Description("Rikers Island")]
        Rikers = 5,

        [Description("Other Special Areas (including forts, navy yard, etc)")]
        Other = 6,

        [Description("Cemetery")]
        Cemetery = 7,

        [Description("Airport")]
        Airport = 8,

        [Description("Park")]
        Park = 9

    }
}
