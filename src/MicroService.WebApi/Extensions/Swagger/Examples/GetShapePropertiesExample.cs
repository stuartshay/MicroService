using Swashbuckle.AspNetCore.Filters;

namespace MicroService.WebApi.Extensions.Swagger.Examples;

/// <summary>
/// Shape Properties Swagger Example
/// </summary>
public class GetShapePropertiesExample : IExamplesProvider<object>
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public object GetExamples()
    {
        return new
        {
            NumFields = 17,
            NumRecords = 1517,
            Bounds = new
            {
                MaxX = -73.71947023226627,
                MaxY = 40.91458948298112,
                MinX = -74.25609432109533,
                MinY = 40.49892924839406,
            },
            LastUpdatedDate = DateTime.Now,
            FieldsList = new List<object>
            {
                new { Name = "address", FullName = "System.String" },
                new { Name = "bbl", FullName = "System.Double" },
                new { Name = "block", FullName = "System.Double" },
                new { Name = "borough", FullName = "System.String" },
                new { Name = "date_des_d", FullName = "System.DateTime" },
                new { Name = "time_des_d", FullName = "System.String" },
                new { Name = "landmark_t", FullName = "System.String" },
                new { Name = "lot", FullName = "System.Double" },
                new { Name = "lpc_altern", FullName = "System.String" },
                new { Name = "lpc_lpnumb", FullName = "System.String" },
                new { Name = "lpc_name", FullName = "System.String" },
                new { Name = "lpc_site_d", FullName = "System.String" },
                new { Name = "lpc_site_s", FullName = "System.String" },
                new { Name = "objectid", FullName = "System.Double" },
                new { Name = "shape_area", FullName = "System.Double" },
                new { Name = "shape_leng", FullName = "System.Double" },
                new { Name = "url_report", FullName = "System.String" }
            }
        };
    }
}