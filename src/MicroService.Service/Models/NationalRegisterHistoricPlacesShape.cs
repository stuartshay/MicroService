using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;

namespace MicroService.Service.Models
{
    public class NationalRegisterHistoricPlacesShape : ShapeBase, ILandmark
    {

        /*

           {
      "name": "address",
      "fullName": "System.String"
    },
        */


        [FeatureName("bbl")]
        public double BBL { get; set; }

        /*
        {
      "name": "block",
      "fullName": "System.Double"
    },
    {
      "name": "borough",
      "fullName": "System.String"
    },
        */

        [FeatureName("borough")]
        public string BoroName { get; set; }

        public int BoroCode { get; set; }



        /*
    {
      "name": "date_des_d",
      "fullName": "System.DateTime"
    },
    {
      "name": "time_des_d",
      "fullName": "System.String"
    },
    {
      "name": "landmark_t",
      "fullName": "System.String"
    },
    {
      "name": "lot",
      "fullName": "System.Double"
    },
    {
      "name": "lpc_altern",
      "fullName": "System.String"
    },
    {
      "name": "lpc_lpnumb",
      "fullName": "System.String"
    },
    {
      "name": "lpc_name",
      "fullName": "System.String"
    },
        */

        [FeatureName("lpc_lpnumb")]
        public string LPNumber { get; set; }

        [FeatureName("lpc_name")]
        public string AreaName { get; set; }








        /*
    {
      "name": "lpc_site_d",
      "fullName": "System.String"
    },
    {
      "name": "lpc_site_s",
      "fullName": "System.String"
    },
    {
      "name": "objectid",
      "fullName": "System.Double"
    },
    {
      "name": "shape_area",
      "fullName": "System.Double"
    },
    {
      "name": "shape_leng",
      "fullName": "System.Double"
    },
    {
      "name": "url_report",
      "fullName": "System.String"


        */







    }
}
