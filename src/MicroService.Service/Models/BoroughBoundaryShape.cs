using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attibutes;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.BoroughBoundaries)]
    public class BoroughBoundaryShape : ShapeBase
    {
        [FeatureName("BoroCode")]
        public int BoroCode { get; set; }

        [FeatureName("BoroName")]
        public string BoroName { get; set; }
    }
}
