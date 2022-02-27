using MicroService.Service.Models.Base;

namespace MicroService.Service.Models
{
    public class DSNYDistrictsShape : ShapeBase
    {
        public string District { get; set; }

        public string DistrictCode { get; set; }

        public string OperationZone { get; set; }

        public string OperationZoneName { get; set; }

    }
}
