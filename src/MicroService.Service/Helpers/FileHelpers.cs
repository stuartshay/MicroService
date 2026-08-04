using System;
using System.IO;
using System.Reflection;

namespace MicroService.Service.Helpers
{
    public static class FileHelpers
    {
        public static string GetFilesDirectory()
        {
            var appPath = GetAssemblyDirectory();
            var dataPath = @"../../../../../files";
            var dataDirectory = Path.GetFullPath(Path.Combine(appPath, dataPath));

            return dataDirectory;
        }

        private static string GetAssemblyDirectory()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(location)
                ?? throw new InvalidOperationException("Unable to determine the executing assembly's directory.");
        }

    }
}
