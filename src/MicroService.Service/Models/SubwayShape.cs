using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    [ShapeProperties(ShapeProperties.Subway)]
    public class SubwayShape : ShapeBase
    {
        [FeatureName("objectid")]
        public double ObjectId { get; set; }

        [FeatureName("line")]
        public string Line { get; set; } = string.Empty;

        [FeatureName("name")]
        public string Name { get; set; } = string.Empty;

        [FeatureName("url")]
        public string Url { get; set; } = string.Empty;

        public double Distance { get; set; }
    }
}
