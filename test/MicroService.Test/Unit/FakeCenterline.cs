using System.Net;
using System.Text;
using System.Text.Json;

namespace MicroService.Test.Unit
{
    /// <summary>
    /// A stand-in for the Street Centerline's Socrata API: made-up segments by street name and search rows,
    /// with every request it was sent.
    /// </summary>
    public sealed class FakeCenterline : HttpMessageHandler
    {
        // A made-up avenue running 5 km north from 40.75,-73.98, in two segments
        public const double Latitude = 40.75;
        public const double Longitude = -73.98;
        public const double MetresPerDegree = 111_320;

        public List<string> Queries { get; } = [];

        /// <summary>
        /// Segments by CSCL spelling, each as [lon, lat] points.
        /// </summary>
        public Dictionary<string, double[][][]> Streets { get; } = new(StringComparer.Ordinal)
        {
            ["5 AVE"] = [Segment(0, 2500), Segment(2500, 5000)],
            ["W  42 ST"] = [Segment(0, 1000)],
        };

        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        public string? Body { get; set; }

        /// <summary>
        /// Extra raw rows added to every segments reply, e.g. rows without a usable shape.
        /// </summary>
        public List<object> ExtraSegmentRows { get; } = [];

        public static double[][] Segment(double fromNorth, double toNorth) =>
        [
            [Longitude, Latitude + fromNorth / MetresPerDegree],
            [Longitude, Latitude + (fromNorth + toNorth) / 2 / MetresPerDegree],
            [Longitude, Latitude + toNorth / MetresPerDegree],
        ];

        public static (double Latitude, double Longitude) North(double metres) => (Latitude + metres / MetresPerDegree, Longitude);

        /// <summary>
        /// Thrown instead of answering, e.g. a TaskCanceledException as an HttpClient timeout throws.
        /// </summary>
        public Exception? Failure { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Answers asynchronously, as a real handler does; the caller owns the response
            await Task.Yield();
            var query = Uri.UnescapeDataString(request.RequestUri!.Query);
            Queries.Add(query);
            if (Failure is not null)
            {
                throw Failure;
            }

            var body = Body ?? (query.Contains("$group=", StringComparison.Ordinal) ? Search() : Segments(query));
            return new HttpResponseMessage(Status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
        }

        private static string Search() => JsonSerializer.Serialize(new object[]
        {
            new { full_street_name = "5 AVE", boroughcode = "1", segments = "139" },
            new { full_street_name = "W  42 ST", boroughcode = "1", segments = "11" },
            new { full_street_name = "5 AVE", boroughcode = "9", segments = "1" },
            new { full_street_name = "BROADWAY", boroughcode = "X", segments = "3" },
            new { boroughcode = "1", segments = "2" },
            new { full_street_name = "NOWHERE ST", segments = "2" },
            new { full_street_name = "MADISON AVE", boroughcode = "1" },
        });

        private string Segments(string query)
        {
            var rows = Streets
                .Where(s => query.Contains($"'{s.Key}'", StringComparison.Ordinal))
                .SelectMany(s => s.Value.Select(segment => new
                {
                    full_street_name = s.Key,
                    the_geom = new { type = "MultiLineString", coordinates = new[] { segment } },
                }))
                .Cast<object>()
                .Concat(ExtraSegmentRows);
            return JsonSerializer.Serialize(rows);
        }
    }
}
