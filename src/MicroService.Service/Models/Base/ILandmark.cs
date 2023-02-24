namespace MicroService.Service.Models.Base
{
    internal interface ILandmark
    {
        public string LPNumber { get; set; }

        public string AreaName { get; set; }

        public string BoroName { get; set; }

        public int BoroCode { get; set; }
    }
}
