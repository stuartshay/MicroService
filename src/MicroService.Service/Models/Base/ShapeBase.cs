using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MicroService.Service.Models.Base
{
    public class ShapeBase
    {
        public double ShapeArea { get; set; }

        public double ShapeLength { get; set; }

        [JsonIgnore]
        public List<Coordinate> Coordinates { get; set; }
    }

    public class Coordinate
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }


}
