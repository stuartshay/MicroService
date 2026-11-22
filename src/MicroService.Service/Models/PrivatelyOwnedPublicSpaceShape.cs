using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;

namespace MicroService.Service.Models
{
    /// <summary>
    /// A privately owned public space (City Planning, NYC Open Data rvih-nhyn): a point at its building.
    /// </summary>
    [ShapeProperties(ShapeProperties.PrivatelyOwnedPublicSpaces)]
    public class PrivatelyOwnedPublicSpaceShape : ShapeBase
    {
        [FeatureCollection]
        [FeatureName("pops_numbe")]
        public string PopsNumber { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("building_n")]
        public string BuildingName { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("building_a")]
        public string Address { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("building_l")]
        public string Location { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("public_spa")]
        public string PublicSpaceType { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("hour_of_ac")]
        public string HoursOfAccess { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("amenities_")]
        public string Amenities { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("year_compl")]
        public string YearCompleted { get; set; } = string.Empty;

        [FeatureCollection]
        [FeatureName("borough_na")]
        public string BoroName { get; set; } = string.Empty;
    }
}
