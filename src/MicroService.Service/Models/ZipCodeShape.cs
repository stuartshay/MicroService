using MicroService.Service.Models.Base;

namespace MicroService.Service.Models
{
    public class ZipCodeShape : ShapeBase
    {
        public string ZipCode { get; set; }

        public string BldgZip { get; set; }

        public string PostOfficeName { get; set; }

        public int Population { get; set; }

        public double Area { get; set; }

        public string State { get; set; }

        public string County { get; set; }
    }
}
