using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

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
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

    }
}
