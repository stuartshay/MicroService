using MicroService.Service.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Services.Layers
{
    /// <summary>
    /// One detail a layer's features show: a label and the shapefile field it comes from.
    /// </summary>
    public sealed record LayerDetail(string Label, string Field);

    /// <summary>
    /// A shape a map can draw as a layer (docs/map-layers-plan.md): its name, geometry, color,
    /// the field that names a feature and the details a click shows.
    /// </summary>
    public sealed record LayerDefinition(
        ShapeProperties Shape,
        string Name,
        string Geometry,
        string Color,
        string Source,
        string DataUpdated,
        string TitleField,
        IReadOnlyList<LayerDetail> Details)
    {
        public string Key => Shape.ToString();
    }

    /// <summary>
    /// The layers a map can draw. Colors are distinct from one another and from the place markers;
    /// every app draws a layer in the color given here.
    /// </summary>
    public static class LayerCatalog
    {
        public const string Polygon = "polygon";
        public const string Point = "point";

        public static IReadOnlyList<LayerDefinition> All { get; } =
        [
            new(ShapeProperties.BoroughBoundaries, "Borough Boundaries", Polygon, "#6B7280",
                "NYC Open Data gthc-hcne", "2026-11-02", "BoroName", []),
            new(ShapeProperties.HistoricDistricts, "Historic Districts", Polygon, "#B45309",
                "NYC Open Data skyk-mpzq", "2026-06-18", "area_name",
                [new("LP number", "lp_number"), new("Designated", "desdate")]),
            new(ShapeProperties.ScenicLandmarks, "Scenic Landmarks", Polygon, "#047857",
                "NYC Open Data qexa-qpj6", "2026-06-18", "scen_lm_na",
                [new("LP number", "lp_number"), new("Designated", "desdate")]),
            new(ShapeProperties.PrivatelyOwnedPublicSpaces, "Privately Owned Public Spaces", Point, "#7C3AED",
                "NYC Open Data rvih-nhyn", "2026-11-05", "building_a",
                [new("Building", "building_n"), new("Space", "public_spa"), new("Hours", "hour_of_ac"), new("Amenities", "amenities_")]),
            new(ShapeProperties.Greenstreets, "Greenstreets", Polygon, "#65A30D",
                "NYC Open Data mk9u-qu7i", "2026-11-02", "location",
                [new("Property", "gispropnum"), new("Status", "featuresta")]),
            new(ShapeProperties.Libraries, "Libraries", Point, "#2563EB",
                "NYC Open Data feuq-due4", "2016-06-22", "name",
                [new("System", "system"), new("Street", "streetname"), new("Website", "url")]),
            new(ShapeProperties.PostOffices, "Post Offices", Point, "#DC2626",
                "NYC Open Data bdha-6eqy", "2014-11-18", "name",
                [new("Street", "streetname"), new("ZIP code", "zip")]),
        ];

        private static readonly Dictionary<string, LayerDefinition> ByKey =
            All.ToDictionary(l => l.Key, StringComparer.OrdinalIgnoreCase);

        /// <summary>The layer with this key in any case, or null.</summary>
        public static LayerDefinition? Find(string? key) =>
            key is not null && ByKey.TryGetValue(key.Trim(), out var layer) ? layer : null;
    }
}
