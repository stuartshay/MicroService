using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;

namespace MicroService.Service.Models
{
    public class SubwayShape : ShapeBase
    {
        [FeatureName("objectid")]
        public double ObjectId { get; set; }

        [FeatureName("line")]
        public string Line { get; set; }

        [FeatureName("name")]
        public string Name { get; set; }

        [FeatureName("url")]
        public string Url { get; set; }

        public double Distance { get; set; }
    }
}
