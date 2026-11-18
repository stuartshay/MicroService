using MicroService.Service.Configuration;
using Microsoft.Extensions.Options;

namespace MicroService.Test.Fixture
{
    /// <summary>
    /// Options whose shape root is the repository's files/ folder, as outside Cloud Run.
    /// </summary>
    public static class RepositoryFiles
    {
        public static IOptions<ApplicationOptions> Options(string? rootDirectory = null) =>
            Microsoft.Extensions.Options.Options.Create(new ApplicationOptions
            {
                ShapeConfiguration = new ShapeConfiguration { ShapeRootDirectory = rootDirectory ?? Directory() },
            });

        public static string Directory()
        {
            var root = new DirectoryInfo(AppContext.BaseDirectory);
            while (root != null && !File.Exists(Path.Join(root.FullName, "MicroService.slnx")))
            {
                root = root.Parent;
            }

            return Path.Join(
                root?.FullName ?? throw new DirectoryNotFoundException("MicroService.slnx not found above the test output."),
                "files");
        }
    }
}
