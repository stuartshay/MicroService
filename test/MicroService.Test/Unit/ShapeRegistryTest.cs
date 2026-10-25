using MicroService.Service.Configuration;
using MicroService.Service.Helpers;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using MicroService.Service.Services.Base;
using MicroService.Test.Fixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;
using Xunit;

namespace MicroService.Test.Unit
{
    public class ShapeRegistryTest : IClassFixture<ShapeServiceFixture>
    {
        private readonly ShapeServiceFixture _fixture;

        public ShapeRegistryTest(ShapeServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void All_RegistersEveryShapePropertiesValueExactlyOnce()
        {
            var enumValues = System.Enum.GetValues<ShapeProperties>();

            Assert.Equal(enumValues.Length, ShapeRegistry.All.Count);
            Assert.Equal(enumValues, ShapeRegistry.All.Select(r => r.Key));
            Assert.Equal(enumValues.Length, ShapeRegistry.All.Select(r => r.ServiceType).Distinct().Count());
        }

        [Fact]
        public void All_RegistersEveryConcreteShapeServiceInServicesNamespace()
        {
            var concreteShapeServices = typeof(ShapeRegistry).Assembly.GetTypes()
                .Where(type => type.Namespace == "MicroService.Service.Services"
                               && !type.IsAbstract
                               && type.BaseType is { IsGenericType: true } baseType
                               && baseType.GetGenericTypeDefinition() == typeof(AbstractShapeService<>))
                .OrderBy(type => type.FullName)
                .ToList();

            var registeredServices = ShapeRegistry.All
                .Select(r => r.ServiceType)
                .OrderBy(type => type.FullName)
                .ToList();

            Assert.Equal(concreteShapeServices, registeredServices);
        }

        [Fact]
        public void All_ReadsDirectoryFileNameAndDatumFromShapeAttribute()
        {
            foreach (var registration in ShapeRegistry.All)
            {
                var attribute = registration.Key.GetAttribute<ShapeAttribute>();

                Assert.Equal(attribute.Directory, registration.Directory);
                Assert.Equal(attribute.FileName, registration.FileName);
                Assert.Equal(attribute.Datum, registration.Datum);
                Assert.Same(registration, ShapeRegistry.GetByKey(registration.Key));
                Assert.Same(registration, ShapeRegistry.GetByKey(registration.KeyName));
                Assert.Same(registration, ShapeRegistry.GetByServiceType(registration.ServiceType));
            }
        }

        [Fact]
        public void DatasetKey_MatchesKeyExceptForDocumentedNationalRegisterOverride()
        {
            // Issue #526: NationalRegisterHistoricPlacesService reads IndividualLandmarkSite until Phase 4.
            var overrides = ShapeRegistry.All
                .Where(r => r.DatasetKey != r.Key)
                .ToList();

            var exception = Assert.Single(overrides);
            Assert.Equal(ShapeProperties.NationalRegisterHistoricPlaces, exception.Key);
            Assert.Equal(ShapeProperties.IndividualLandmarkSite, exception.DatasetKey);
            Assert.Equal(nameof(ShapeProperties.IndividualLandmarkSite), exception.DatasetKeyName);
        }

        [Fact]
        public void EveryRegisteredService_ResolvesAndOpensDatasetFileNamedByRegistry()
        {
            var options = _fixture.ServiceProvider.GetRequiredService<IOptions<ApplicationOptions>>();
            var rootDirectory = options.Value.ShapeConfiguration!.ShapeSystemRootDirectory!;

            foreach (var registration in ShapeRegistry.All)
            {
                var resolvedByKey = _fixture.ShapeServiceResolver(registration.KeyName);
                var resolvedByType = _fixture.ServiceProvider.GetRequiredService(registration.ServiceType);

                Assert.IsType(registration.ServiceType, resolvedByKey);
                Assert.Same(resolvedByType, resolvedByKey);

                var readerProperty = registration.ServiceType.GetProperty(
                    "ShapeFileDataReader",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                Assert.NotNull(readerProperty);

                var reader = Assert.IsType<CachedShapefileDataReader>(readerProperty.GetValue(resolvedByKey));
                var datasetRegistration = ShapeRegistry.GetByKey(registration.DatasetKey);
                var expectedFilePath = ShapeRegistry.GetShapeFilePath(rootDirectory, datasetRegistration);

                Assert.Equal(registration.DatasetKeyName, reader.ShapeKey);
                Assert.Equal(expectedFilePath, reader.FilePath);
                Assert.True(resolvedByKey.GetShapeDatabaseProperties().NumRecords > 0);
            }
        }

        [Fact]
        public void LookupMethods_ThrowKeyNotFoundException_ForUnknownKeysAndTypes()
        {
            Assert.Throws<KeyNotFoundException>(() => ShapeRegistry.GetByKey("UnknownKey"));
            Assert.Throws<KeyNotFoundException>(() => ShapeRegistry.GetByKey(null!));
            Assert.Throws<KeyNotFoundException>(() => ShapeRegistry.GetByKey((ShapeProperties)9999));
            Assert.Throws<KeyNotFoundException>(() => ShapeRegistry.GetByServiceType(typeof(string)));
            Assert.Throws<KeyNotFoundException>(() => _fixture.ShapeServiceResolver("UnknownKey"));
        }

        [Fact]
        public void CreateDataReader_ThrowsInvalidOperationException_WhenRootDirectoryIsMissing()
        {
            using var cache = new MemoryCache(new MemoryCacheOptions());
            var options = Options.Create(new ApplicationOptions
            {
                ShapeConfiguration = new ShapeConfiguration { ShapeRootDirectory = null },
            });

            var exception = Assert.Throws<InvalidOperationException>(() =>
                ShapeRegistry.CreateDataReader(cache, options, nameof(ShapeProperties.BoroughBoundaries)));

            Assert.Contains("ShapeConfiguration:ShapeRootDirectory", exception.Message);
        }
    }
}

