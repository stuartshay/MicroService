using System.ComponentModel;

namespace MicroService.Data.Enum
{
    public enum Borough
    {
        [Description("Manhattan")]
        MN = 1,

        [Description("Bronx")]
        BX = 2,

        [Description("Brooklyn")]
        BK = 3,

        [Description("Queens")]
        QN = 4,

        [Description("Staten Island")]
        SI = 5,
    }
}
