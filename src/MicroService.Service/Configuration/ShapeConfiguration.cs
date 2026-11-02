namespace MicroService.Service.Configuration
{
    public class ShapeConfiguration
    {
        public string? ShapeRootDirectory { get; set; }

        public string? CronExpression { get; set; }

        public string? ShapeSystemRootDirectory => ShapeRootDirectory == null
            ? null
            : Path.GetFullPath(ShapeRootDirectory, Directory.GetCurrentDirectory());
    }
}
