﻿using MicroService.Service.Models.Base;

namespace MicroService.Service.Models
{
    public class HistoricDistrictShape : ShapeBase, ILandmark
    {
        public string LPNumber { get; set; }

        public string AreaName { get; set; }

        public string BoroName { get; set; }

        public int BoroCode { get; set; }
    }
}
