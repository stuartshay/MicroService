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
        public async Task<IReadOnlyList<Geometry>> GetSegmentsAsync(string name, Borough borough, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(name);
            var key = $"streets|segments|{(int)borough}|{name}";
            if (_cache.TryGetValue(key, out IReadOnlyList<Geometry>? cached) && cached is not null)
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
            IReadOnlyList<Geometry> segments = spellings.Select(s => bySpelling.GetValueOrDefault(s)).FirstOrDefault(l => l is not null) ?? [];

            _cache.Set(key, segments, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Lifetime,
                Size = segments.Sum(s => s.NumPoints) * 16 + 64,
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

            // CSCL pads a leading one-letter direction with two spaces, so search for each spelling
            var likes = StreetNames.Spellings(text).Select(t => $"upper(full_street_name) like '%{Quote(t)}%'");
            var where = $"({string.Join(" OR ", likes)})"
                + (borough is null ? string.Empty : $" AND boroughcode='{(int)borough}'");
            var query = "?$select=full_street_name,boroughcode,count(*) AS segments"
                + $"&$where={Uri.EscapeDataString(where)}"
                + "&$group=full_street_name,boroughcode&$order=segments DESC"
                + $"&$limit={MaxSearchResults}";
            using var document = await GetJsonAsync(query, cancellationToken).ConfigureAwait(false);

            var names = new List<StreetName>();
            foreach (var row in document.RootElement.EnumerateArray())
            {
                var name = row.TryGetProperty("full_street_name", out var n) ? n.GetString() : null;
                var code = row.TryGetProperty("boroughcode", out var b) ? b.GetString() : null;
                var count = row.TryGetProperty("segments", out var c) && int.TryParse(c.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;
                if (string.IsNullOrWhiteSpace(name) || !int.TryParse(code, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
                    || !Enum.IsDefined(typeof(Borough), number))
                {
                    continue;
                }

                names.Add(new StreetName(StreetNames.Collapse(name), ((Borough)number).ToString(), count));
            }

            _cache.Set(key, (IReadOnlyList<StreetName>)names, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Lifetime,
                Size = names.Count * 64 + 64,
            });
            return names;
        }

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
