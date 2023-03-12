using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum.Attibutes;

namespace MicroService.Service.Models
{
    public class BoroughBoundaryShape : ShapeBase
    {

        [FeatureName("BoroCode")]
        public int BoroCode { get; set; }

        [FeatureName("BoroName")]
        public string BoroName { get; set; }
    }
}
