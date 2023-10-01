namespace MicroService.Service.Configuration
{
    public class ShapeConfiguration
    {
        public string? ShapeRootDirectory { get; set; }

        public string? CronExpression { get; set; }

        public string? ShapeSystemRootDirectory => Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ShapeRootDirectory));
    }
}
