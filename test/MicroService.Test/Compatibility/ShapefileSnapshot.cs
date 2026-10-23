using MicroService.Service.Mappings;
using MicroService.Service.Models;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Converters;
using System.Security.Cryptography;
using System.Text.Json;

namespace MicroService.Test.Compatibility
{
    internal sealed record ShapefileSnapshot(int FeatureCount, int MappingFailures, string AttributeHash, string GeometryHash, string MappedHash, string MetadataHash)
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Converters = { new GeoJsonConverterFactory() }
        };

        public static ShapefileSnapshot Capture(ShapeProperties dataset, IEnumerable<Feature> features,
            ShapefileProperties shape, ShapeDatabaseProperties database)
        {
            using var attributes = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var geometry = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            using var mapped = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var count = 0;
            var mappingFailures = 0;
            foreach (var feature in features)
            {
                count++;
                attributes.AppendData(JsonSerializer.SerializeToUtf8Bytes(feature.Attributes.GetNames().Select(name => new
                {
                    Name = name,
                    Type = feature.Attributes[name]?.GetType().FullName,
                    Value = feature.Attributes[name]
                })));
                attributes.AppendData("\n"u8);
                geometry.AppendData(feature.Geometry.AsBinary());
                geometry.AppendData("\n"u8);
                try
                {
                    var model = Map(dataset, feature);
                    mapped.AppendData(JsonSerializer.SerializeToUtf8Bytes(model, model.GetType(), JsonOptions));
                }
                catch (Exception exception) when (exception is ArgumentException or FormatException or OverflowException)
                {
                    // Record existing mapper failures as part of the baseline; the reader
                    // migration must neither introduce failures nor conceal existing ones.
                    mappingFailures++;
                    mapped.AppendData(JsonSerializer.SerializeToUtf8Bytes(new { Error = exception.GetType().FullName, exception.Message }));
                }
                mapped.AppendData("\n"u8);
            }
            var metadata = JsonSerializer.SerializeToUtf8Bytes(new
            {
                shape.ShapeType,
                shape.Bounds.MinX, shape.Bounds.MinY, shape.Bounds.MaxX, shape.Bounds.MaxY,
                database.NumFields, database.NumRecords, database.LastUpdateDate,
                Fields = database.Fields.Select(f => new { f.Name, Type = f.Type.FullName, f.DbaseType, f.Length, f.DecimalCount })
            });
            return new ShapefileSnapshot(count, mappingFailures, Convert.ToHexString(attributes.GetHashAndReset()),
                Convert.ToHexString(geometry.GetHashAndReset()), Convert.ToHexString(mapped.GetHashAndReset()),
                Convert.ToHexString(SHA256.HashData(metadata)));
        }

        private static object Map(ShapeProperties dataset, Feature feature) => dataset switch
        {
            ShapeProperties.BoroughBoundaries => BoroughBoundaryShapeProfile.Map(feature),
            ShapeProperties.CommunityDistricts => CommunityDistrictShapeProfile.Map(feature),
            ShapeProperties.DSNYDistricts => DsnyDistrictsShapeProfile.Map(feature),
            ShapeProperties.HistoricDistricts => HistoricDistrictShapeProfile.Map(feature),
            ShapeProperties.IndividualLandmarkSite => IndividualLandmarkSiteShapeProfile.Map(feature),
            ShapeProperties.IndividualLandmarkHistoricDistricts => IndividualLandmarkHistoricDistrictsShapeProfile.Map(feature),
            ShapeProperties.NationalRegisterHistoricPlaces => NationalRegisterHistoricPlacesShapeProfile.Map(feature),
            ShapeProperties.Neighborhoods => NeighborhoodShapeMappingsProfile.Map(feature),
            ShapeProperties.NeighborhoodTabulationAreas => NeighborhoodTabulationAreaShapeProfile.Map(feature),
            ShapeProperties.NypdPolicePrecincts => NypdPrecinctShapeProfile.Map(feature),
            ShapeProperties.NypdSectors => NypdSectorShapeProfile.Map(feature),
            ShapeProperties.NychaDevelopments => NychaDevelopmentShapeProfile.Map(feature),
            ShapeProperties.Parks => ParkShapeProfile.Map(feature),
            ShapeProperties.ScenicLandmarks => FeatureToScenicLandmarkShapeProfile.Map(feature),
            ShapeProperties.Subway => SubwayShapeProfile.Map(feature),
            ShapeProperties.ZipCodes => ZipCodeMappingsProfile.Map(feature),
            _ => throw new ArgumentOutOfRangeException(nameof(dataset), dataset, null)
        };
    }
}
