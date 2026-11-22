using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MicroService.Service.Services.Streets
{
    /// <summary>
    /// Street names as the city's Street Centerline (CSCL) writes them: upper case, numbered streets in
    /// figures and street types abbreviated, e.g. "5 AVE", "PARK AVE S". CSCL is not consistent about
    /// directions ("WEST ST" and "EAST RIVER LN", but "W  42 ST" and "E  BROADWAY" with two spaces), so a
    /// name is looked up by each of its <see cref="Spellings"/>.
    /// </summary>
    public static partial class StreetNames
    {
        /// <summary>
        /// The longest name a request takes.
        /// </summary>
        public const int MaxLength = 64;

        private static readonly Dictionary<string, string> Words = new(StringComparer.OrdinalIgnoreCase)
        {
            ["FIRST"] = "1", ["SECOND"] = "2", ["THIRD"] = "3", ["FOURTH"] = "4", ["FIFTH"] = "5",
            ["SIXTH"] = "6", ["SEVENTH"] = "7", ["EIGHTH"] = "8", ["NINTH"] = "9", ["TENTH"] = "10",
            ["ELEVENTH"] = "11", ["TWELFTH"] = "12",
            ["AVENUE"] = "AVE", ["AV"] = "AVE", ["STREET"] = "ST", ["PLACE"] = "PL", ["BOULEVARD"] = "BLVD",
            ["ROAD"] = "RD", ["DRIVE"] = "DR", ["PARKWAY"] = "PKWY", ["LANE"] = "LN", ["TERRACE"] = "TER",
            ["COURT"] = "CT", ["SQUARE"] = "SQ",
        };

        private static readonly Dictionary<string, string> Directions = new(StringComparer.Ordinal)
        {
            ["EAST"] = "E", ["WEST"] = "W", ["NORTH"] = "N", ["SOUTH"] = "S",
            ["E"] = "EAST", ["W"] = "WEST", ["N"] = "NORTH", ["S"] = "SOUTH",
        };

        /// <summary>
        /// A name in CSCL style, single-spaced: "Fifth Avenue" and "5th Ave" are both "5 AVE"; "W. 42nd Street"
        /// is "W 42 ST". Directions are left as written; <see cref="Spellings"/> tries the other forms.
        /// </summary>
        public static string Normalize(string name)
        {
            ArgumentNullException.ThrowIfNull(name);
            var words = WordPattern().Matches(name.Replace('.', ' ')).Select(m => Word(m.Value));
            return string.Join(' ', words).ToUpperInvariant();
        }

        /// <summary>
        /// The spellings to look a normalized name up by, the name itself first: a leading direction as the
        /// word and as its letter padded with two spaces ("WEST 42 ST", "W  42 ST"), and a trailing direction
        /// after two or more words as the letter and as the word ("PARK AVE S", "PARK AVE SOUTH").
        /// </summary>
        public static IReadOnlyList<string> Spellings(string normalized)
        {
            ArgumentNullException.ThrowIfNull(normalized);
            var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2)
            {
                return [string.Join(' ', words)];
            }

            var first = Choices(words[0], leading: true);
            var last = words.Length >= 3 ? Choices(words[^1], leading: false) : [words[^1]];
            var middle = string.Join(' ', words[1..^1]);

            var spellings = new List<string>();
            foreach (var start in first)
            {
                foreach (var end in last)
                {
                    // A one-letter leading direction is padded to two characters
                    var separator = start.Length == 1 && Directions.ContainsKey(start) ? "  " : " ";
                    var rest = middle.Length == 0 ? end : middle + " " + end;
                    var spelling = start + separator + rest;
                    if (!spellings.Contains(spelling, StringComparer.Ordinal))
                    {
                        spellings.Add(spelling);
                    }
                }
            }

            // The name as written comes first, single-spaced
            var written = string.Join(' ', words);
            var index = spellings.FindIndex(s => Collapse(s) == written);
            if (index > 0)
            {
                var spelling = spellings[index];
                spellings.RemoveAt(index);
                spellings.Insert(0, spelling);
            }

            return spellings;
        }

        /// <summary>
        /// One key for every spelling of a name, to tell when two requested names are the same street.
        /// </summary>
        public static string Key(string normalized) =>
            string.Join('|', Spellings(normalized).Select(Collapse).Order(StringComparer.Ordinal));

        /// <summary>
        /// A CSCL name single-spaced, as the API shows it ("W  42 ST" is "W 42 ST").
        /// </summary>
        public static string Collapse(string name) =>
            string.Join(' ', (name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries));

        /// <summary>
        /// True for a name the requests take: letters, figures, spaces, apostrophes and hyphens, at most <see cref="MaxLength"/> characters.
        /// </summary>
        public static bool IsValid(string name) =>
            !string.IsNullOrWhiteSpace(name) && name.Length <= MaxLength && AllowedPattern().IsMatch(name);

        private static string[] Choices(string word, bool leading)
        {
            if (!Directions.TryGetValue(word, out var other))
            {
                return [word];
            }

            // Leading: the word, then the letter; trailing: the letter, then the word
            var letter = word.Length == 1 ? word : other;
            var full = word.Length == 1 ? other : word;
            return leading ? [full, letter] : [letter, full];
        }

        private static string Word(string word)
        {
            // 42nd, 1st, 3rd, 5th: the figure alone
            var ordinal = OrdinalPattern().Match(word);
            if (ordinal.Success)
            {
                return ordinal.Groups[1].Value;
            }

            return Words.TryGetValue(word, out var replacement) ? replacement : word;
        }

        [GeneratedRegex(@"[A-Za-z0-9'\-]+")]
        private static partial Regex WordPattern();

        [GeneratedRegex(@"^(\d+)(?:ST|[NR]D|TH)$", RegexOptions.IgnoreCase)]
        private static partial Regex OrdinalPattern();

        [GeneratedRegex(@"^[A-Za-z0-9 .'\-]+$")]
        private static partial Regex AllowedPattern();
    }
}
