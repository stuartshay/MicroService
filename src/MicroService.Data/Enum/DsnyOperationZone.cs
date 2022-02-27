using System.ComponentModel;

namespace MicroService.Data.Enum
{
    public enum DsnyOperationZone
    {
        [Description("Manhattan")]
        MN = 1,

        [Description("Bronx")]
        BX = 2,

        [Description("Brooklyn North")]
        BKN = 3,

        [Description("Brooklyn South")]
        BKS = 4,

        [Description("Queens West")]
        QW = 5,

        [Description("Queens East")]
        QE = 6,

        [Description("Staten Island")]
        SI = 7,
    }
}
