using MicroService.Data.Enum;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MicroService.Service.Services.Streets
{
    /// <summary>
    /// One street name in the centerline, with its borough and how many segments it has.
    /// </summary>
    public sealed record StreetName(string Name, string Borough, int Segments);

    /// <summary>
    /// A street's segments and the centerline spelling they were found under; none when no spelling matched.
    /// </summary>
    public sealed record StreetSegments(string? Spelling, IReadOnlyList<Geometry> Segments);

    /// <summary>
    /// The city's Street Centerline (CSCL, NYC Open Data <c>inkn-q76z</c>), read through the Socrata
    /// API: a street's segments by name and borough, and street names matching a search. Answers are
    /// kept a day in memory; the centerline changes a few times a year.
    /// </summary>
    public class StreetCenterlineClient
    {
        /// <summary>
        /// The dataset's rows endpoint, relative to https://data.cityofnewyork.us/.
        /// </summary>
        public const string ResourcePath = "resource/inkn-q76z.json";

        private const int MaxSegments = 5000;

        private const int MaxSearchResults = 20;

        private static readonly TimeSpan Lifetime = TimeSpan.FromDays(1);

        private static readonly JsonSerializerOptions GeoJsonOptions = new()
        {
            Converters = { new GeoJsonConverterFactory() },
        };

        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;

        public StreetCenterlineClient(HttpClient http, IMemoryCache cache)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// The segments of the street named <paramref name="name"/> (normalized, <see cref="StreetNames.Normalize"/>)
        /// in the borough, in WGS84, under the first of its <see cref="StreetNames.Spellings"/> the centerline has.
        /// </summary>
        public async Task<StreetSegments> GetSegmentsAsync(string name, Borough borough, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(name);
            var key = $"streets|segments|{(int)borough}|{name}";
            if (_cache.TryGetValue(key, out StreetSegments? cached) && cached is not null)
            {
                return cached;
            }

            var spellings = StreetNames.Spellings(name);
            var where = $"boroughcode='{(int)borough}' AND full_street_name in({string.Join(',', spellings.Select(n => $"'{Quote(n)}'"))})";
            var query = $"?$select=full_street_name,the_geom&$where={Uri.EscapeDataString(where)}&$limit={MaxSegments}";
            using var document = await GetJsonAsync(query, cancellationToken).ConfigureAwait(false);

            var bySpelling = new Dictionary<string, List<Geometry>>(StringComparer.Ordinal);
            foreach (var row in document.RootElement.EnumerateArray())
            {
                var spelling = row.TryGetProperty("full_street_name", out var n) ? n.GetString() : null;
                if (spelling is not null && row.TryGetProperty("the_geom", out var shape) && shape.ValueKind == JsonValueKind.Object
                    && shape.Deserialize<Geometry>(GeoJsonOptions) is { IsEmpty: false } geometry)
                {
                    if (!bySpelling.TryGetValue(spelling, out var list))
                    {
                        bySpelling[spelling] = list = [];
                    }

                    list.Add(geometry);
                }
            }

            // Two spellings can be two streets (EAST DR and E  DR), so only the first one found is used
            var found = spellings.FirstOrDefault(bySpelling.ContainsKey);
            var segments = new StreetSegments(found, found is null ? [] : bySpelling[found]);

            _cache.Set(key, segments, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Lifetime,
                Size = (segments.Segments.Sum(s => s.NumPoints) * 16) + 64,
            });
            return segments;
        }

        /// <summary>
        /// The street names that contain <paramref name="text"/> (CSCL spelling), in one borough or all,
        /// most segments first.
        /// </summary>
        public async Task<IReadOnlyList<StreetName>> SearchAsync(string text, Borough? borough, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(text);
            var key = $"streets|search|{(borough is null ? 0 : (int)borough)}|{text}";
            if (_cache.TryGetValue(key, out IReadOnlyList<StreetName>? cached) && cached is not null)
            {
                return cached;
            }

            using var document = await GetJsonAsync(SearchQuery(text, borough), cancellationToken).ConfigureAwait(false);
            IReadOnlyList<StreetName> names = document.RootElement.EnumerateArray()
                .Select(ToStreetName)
                .OfType<StreetName>()
                .ToList();

            _cache.Set(key, names, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Lifetime,
                Size = (names.Count * 64) + 64,
            });
            return names;
        }

        /// <summary>
        /// The names containing any spelling of <paramref name="text"/> (CSCL pads a leading one-letter
        /// direction with two spaces), with their segment counts, most first.
        /// </summary>
        private static string SearchQuery(string text, Borough? borough)
        {
            var likes = StreetNames.Spellings(text).Select(t => $"upper(full_street_name) like '%{Quote(t)}%'");
            var where = $"({string.Join(" OR ", likes)})"
                + (borough is null ? string.Empty : $" AND boroughcode='{(int)borough}'");
            return "?$select=full_street_name,boroughcode,count(*) AS segments"
                + $"&$where={Uri.EscapeDataString(where)}"
                + "&$group=full_street_name,boroughcode&$order=segments DESC"
                + $"&$limit={MaxSearchResults}";
        }

        /// <summary>
        /// A search row as a name, or null without a name or a known borough; a row without a count has 0.
        /// </summary>
        private static StreetName? ToStreetName(JsonElement row)
        {
            var name = Text(row, "full_street_name");
            if (string.IsNullOrWhiteSpace(name)
                || !int.TryParse(Text(row, "boroughcode"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
                || !Enum.IsDefined(typeof(Borough), number))
            {
                return null;
            }

            var segments = int.TryParse(Text(row, "segments"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) ? count : 0;
            return new StreetName(StreetNames.Collapse(name), ((Borough)number).ToString(), segments);
        }

        private static string? Text(JsonElement row, string property) =>
            row.TryGetProperty(property, out var value) ? value.GetString() : null;

        private async Task<JsonDocument> GetJsonAsync(string query, CancellationToken cancellationToken)
        {
            using var response = await _http.GetAsync(new Uri(ResourcePath + query, UriKind.Relative), cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                document.Dispose();
                throw new HttpRequestException("The street centerline did not return a list.");
            }

            return document;
        }

        // SoQL string literals double their single quotes
        private static string Quote(string value) => value.Replace("'", "''", StringComparison.Ordinal);
    }
}
