using System;
using System.Collections.Generic;

namespace MicroService.Service.Services.Subway
{
    /// <summary>
    /// The MTA's route colors and the order routes are listed in.
    /// </summary>
    public static class SubwayRoutes
    {
        public const string White = "#FFFFFF";

        public const string Black = "#000000";

        /// <summary>
        /// Station lists write every shuttle as S; the service lines name them SF (Franklin
        /// Avenue), SR (Rockaway Park) and ST (42 Street).
        /// </summary>
        public const string Shuttle = "S";

        public static IReadOnlyList<string> Shuttles { get; } = ["SF", "SR", "ST"];

        private const string Grey = "#808183";

        private const string Blue = "#0039A6";

        private const string Yellow = "#FCCC0A";

        private static readonly Dictionary<string, string> Colors = new(StringComparer.Ordinal)
        {
            ["1"] = "#EE352E",
            ["2"] = "#EE352E",
            ["3"] = "#EE352E",
            ["4"] = "#00933C",
            ["5"] = "#00933C",
            ["5 Peak"] = "#00933C",
            ["6"] = "#00933C",
            ["7"] = "#B933AD",
            ["A"] = Blue,
            ["C"] = Blue,
            ["E"] = Blue,
            ["B"] = "#FF6319",
            ["D"] = "#FF6319",
            ["F"] = "#FF6319",
            ["M"] = "#FF6319",
            ["G"] = "#6CBE45",
            ["J"] = "#996633",
            ["Z"] = "#996633",
            ["L"] = "#A7A9AC",
            ["N"] = Yellow,
            ["Q"] = Yellow,
            ["R"] = Yellow,
            ["W"] = Yellow,
            [Shuttle] = Grey,
            ["SF"] = Grey,
            ["SR"] = Grey,
            ["ST"] = Grey,
            ["SIR"] = Blue,
        };

        /// <summary>
        /// The route's color; grey for a code the MTA list doesn't have.
        /// </summary>
        public static string GetColor(string service) =>
            Colors.TryGetValue(service, out var color) ? color : Grey;

        /// <summary>
        /// Black on the yellow N, Q, R and W, white on every other color.
        /// </summary>
        public static string GetTextColor(string service) =>
            GetColor(service) == Yellow ? Black : White;

        /// <summary>
        /// Numbered routes first, in number order, then lettered ones alphabetically.
        /// </summary>
        public static IComparer<string> Order { get; } = Comparer<string>.Create(Compare);

        private static int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            bool xNumbered = x.Length > 0 && char.IsAsciiDigit(x[0]);
            bool yNumbered = y.Length > 0 && char.IsAsciiDigit(y[0]);

            if (xNumbered != yNumbered)
            {
                return xNumbered ? -1 : 1;
            }

            return string.CompareOrdinal(x, y);
        }
    }
}
