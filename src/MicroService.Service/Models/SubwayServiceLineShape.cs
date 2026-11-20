using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    /// <summary>
    /// One feature of the MTA's Subway Service Lines: the route of a service (1, A, SIR, 5 Peak ...).
    /// A and C each have two features.
    /// </summary>
    [ShapeProperties(ShapeProperties.SubwayServiceLines)]
    public class SubwayServiceLineShape : ShapeBase
    {
        [FeatureName("objectid")]
        public int ObjectId { get; set; }

        [FeatureName("service")]
        public string Service { get; set; } = string.Empty;

        [FeatureName("service_na")]
        public string ServiceName { get; set; } = string.Empty;
    }
}
