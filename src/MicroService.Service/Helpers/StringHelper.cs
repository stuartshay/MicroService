using System.Text.RegularExpressions;

namespace MicroService.Service.Helpers
{
    public static partial class StringHelper
    {
        public static string RemoveIntegers(this string input)
        {
            return DigitsAndHyphensRegex().Replace(input, string.Empty);
        }

        [GeneratedRegex(@"[\d-]")]
        private static partial Regex DigitsAndHyphensRegex();
    }
}
