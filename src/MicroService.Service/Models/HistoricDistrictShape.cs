using MicroService.Service.Models.Base;

namespace MicroService.Service.Models
{
    public class HistoricDistrictShape : ShapeBase
    {
        public string LPNumber { get; set; }

        public string AreaName { get; set; }

        public string BoroName { get; set; }
    }
}
