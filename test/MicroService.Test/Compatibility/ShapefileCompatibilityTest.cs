using MicroService.Service.Helpers;
using MicroService.Service.Models.Enum;
using MicroService.Service.Models.Enum.Attributes;
using System.Text.Json;
using Xunit;

namespace MicroService.Test.Compatibility
{
    [Trait("Category", "Integration")]
    public class ShapefileCompatibilityTest
    {
        public static TheoryData<ShapeProperties> Datasets => new(System.Enum.GetValues<ShapeProperties>());

        [Theory]
        [MemberData(nameof(Datasets))]
        public void EveryFeature_MatchesLegacyReaderSnapshot(ShapeProperties dataset)
        {
            AssertSnapshot(dataset, dataset, dataset.ToString());
        }

        private static void AssertSnapshot(ShapeProperties dataset, ShapeProperties source, string baseline)
        {
            var root = FindRepositoryRoot();
            var attribute = source.GetAttribute<ShapeAttribute>();
            var path = Path.Join(root, "files", attribute.Directory, attribute.FileName);
            var baselinePath = Path.Join(root, "test", "MicroService.Test", "Compatibility", "legacy-reader-baseline.json");
            var baselines = JsonSerializer.Deserialize<Dictionary<string, ShapefileSnapshot>>(File.ReadAllText(baselinePath))!;
            var (shape, database) = ShapefileSource.ReadMetadata(path);
            var actual = ShapefileSnapshot.Capture(dataset, ShapefileSource.ReadFeatures(path), shape, database);
            Assert.Equal(baselines[baseline], actual);
        }

        private static string FindRepositoryRoot()
        {
            var root = new DirectoryInfo(AppContext.BaseDirectory);
            while (root != null && !File.Exists(Path.Join(root.FullName, "MicroService.sln")))
            {
                root = root.Parent;
            }
            return root?.FullName ?? throw new DirectoryNotFoundException("MicroService.sln not found above the test output.");
        }
    }
}
