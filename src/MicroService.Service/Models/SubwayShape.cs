using MicroService.Service.Models.Base;

namespace MicroService.Service.Models
{
    public class SubwayShape : ShapeBase
    { 
        public int ObjectId { get; set; }

        public string Line { get; set; }

        public string Name { get; set; }

        public string Notes { get; set; }

        public double Distance { get; set; }
    }
}
