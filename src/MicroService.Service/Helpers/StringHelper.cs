using System.Text.RegularExpressions;

namespace MicroService.Service.Helpers
{
    public static class StringHelper
    {
        public static string RemoveIntegers(this string input)
        {
            return Regex.Replace(input, @"[\d-]", string.Empty);
        }
    }
}
