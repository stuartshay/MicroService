using MicroService.Service.Configuration;
using MicroService.Service.Helpers;
using MicroService.Service.Interfaces;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace MicroService.Service.Services.Base
{
    public sealed class ShapeRegistration
    {
        public ShapeRegistration(
            ShapeProperties key,
            Type serviceType,
            ShapeProperties? datasetKey = null)
        {
            ArgumentNullException.ThrowIfNull(serviceType);

            Key = key;
            KeyName = key.ToString();
            ServiceType = serviceType;
            DatasetKey = datasetKey ?? key;
            DatasetKeyName = DatasetKey.ToString();

            var shapeAttribute = key.GetAttribute<ShapeAttribute>()
                ?? throw new InvalidOperationException($"ShapeAttribute is missing for '{key}'.");
            Directory = shapeAttribute.Directory;
            FileName = shapeAttribute.FileName;
            Datum = shapeAttribute.Datum;
        }

        public ShapeProperties Key { get; }

        public string KeyName { get; }

        public Type ServiceType { get; }

        /// <summary>
        /// The dataset key opened by this service's shapefile reader. Matches <see cref="Key"/>
        /// for every shape except documented legacy bindings (see #526).
        /// </summary>
        public ShapeProperties DatasetKey { get; }

        public string DatasetKeyName { get; }

        public string Directory { get; }

        public string FileName { get; }

        public Datum Datum { get; }
    }

    public static class ShapeRegistry
    {
        public static IReadOnlyList<ShapeRegistration> All { get; } =
        [
            new(ShapeProperties.BoroughBoundaries, typeof(BoroughBoundariesService)),
            new(ShapeProperties.CommunityDistricts, typeof(CommunityDistrictsService)),
            new(ShapeProperties.DSNYDistricts, typeof(DsnyDistrictsService)),
            new(ShapeProperties.HistoricDistricts, typeof(HistoricDistrictService)),
            new(ShapeProperties.IndividualLandmarkSite, typeof(IndividualLandmarkSiteService)),
            new(ShapeProperties.IndividualLandmarkHistoricDistricts, typeof(IndividualLandmarkHistoricDistrictsService)),
            // Documented exception (#526): NationalRegisterHistoricPlacesService currently reads the
            // IndividualLandmarkSite dataset. Phase 4 will switch it to its own HRHP archive.
            new(ShapeProperties.NationalRegisterHistoricPlaces, typeof(NationalRegisterHistoricPlacesService), datasetKey: ShapeProperties.IndividualLandmarkSite),
            new(ShapeProperties.Neighborhoods, typeof(NeighborhoodsService)),
            new(ShapeProperties.NeighborhoodTabulationAreas, typeof(NeighborhoodTabulationAreasService)),
            new(ShapeProperties.NypdPolicePrecincts, typeof(NypdPolicePrecinctService)),
            new(ShapeProperties.NypdSectors, typeof(NypdSectorsService)),
            new(ShapeProperties.NychaDevelopments, typeof(NychaDevelopmentService)),
            new(ShapeProperties.Parks, typeof(ParkService)),
            new(ShapeProperties.ScenicLandmarks, typeof(ScenicLandmarkService)),
            new(ShapeProperties.Subway, typeof(SubwayService)),
            new(ShapeProperties.ZipCodes, typeof(ZipCodeService)),
        ];

        private static readonly IReadOnlyDictionary<string, ShapeRegistration> ByKeyName =
            All.ToDictionary(r => r.KeyName, StringComparer.Ordinal);

        private static readonly IReadOnlyDictionary<ShapeProperties, ShapeRegistration> ByKey =
            All.ToDictionary(r => r.Key);

        private static readonly IReadOnlyDictionary<Type, ShapeRegistration> ByServiceType =
            All.ToDictionary(r => r.ServiceType);

        public static ShapeRegistration GetByKey(string key)
        {
            if (key is not null && ByKeyName.TryGetValue(key, out var registration))
            {
                return registration;
            }

            throw new KeyNotFoundException(key);
        }

        public static ShapeRegistration GetByKey(ShapeProperties key)
        {
            if (ByKey.TryGetValue(key, out var registration))
            {
                return registration;
            }

            throw new KeyNotFoundException(key.ToString());
        }

        public static ShapeRegistration GetByServiceType(Type serviceType)
        {
            ArgumentNullException.ThrowIfNull(serviceType);

            if (ByServiceType.TryGetValue(serviceType, out var registration))
            {
                return registration;
            }

            throw new KeyNotFoundException(serviceType.FullName ?? serviceType.Name);
        }

        public static string GetShapeFilePath(string rootDirectory, ShapeRegistration registration)
        {
            ArgumentNullException.ThrowIfNull(rootDirectory);
            ArgumentNullException.ThrowIfNull(registration);

            var relativeDirectory = registration.Directory.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var relativeFileName = registration.FileName.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return Path.Join(rootDirectory, relativeDirectory, relativeFileName);
        }

        public static CachedShapefileDataReader CreateDataReader(
            IMemoryCache cache,
            IOptions<ApplicationOptions> options,
            string key)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(options);

            var registration = GetByKey(key);
            var rootDirectory = options.Value.ShapeConfiguration?.ShapeSystemRootDirectory
                ?? throw new InvalidOperationException("ShapeConfiguration:ShapeRootDirectory configuration is required.");
            string shapeFileNamePath = GetShapeFilePath(rootDirectory, registration);

            return new CachedShapefileDataReader(cache, registration.KeyName, shapeFileNamePath);
        }

        public static IShapeService<ShapeBase> ResolveService(IServiceProvider serviceProvider, string key)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var registration = GetByKey(key);
            return (serviceProvider.GetService(registration.ServiceType) as IShapeService<ShapeBase>)!;
        }
    }
}

