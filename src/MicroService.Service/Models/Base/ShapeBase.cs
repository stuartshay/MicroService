namespace MicroService.Service.Models.Base
{
    public class ShapeBase
    {
        public double ShapeArea { get; set; }

        public double ShapeLength { get; set; }

        public BoundingBox BoundingBox { get; set; }
    }

    public class BoundingBox
    {
        public double Area { get; set; }

        public CentrePoint Centre { get; set; }

        public double Diameter { get; set; }

        public double MinX { get; set; }

        public double MaxX { get; set; }

        public double MinY { get; set; }

        public double MaxY { get; set; }

        public double Width => MaxX - MinX;

        public double Height => MaxY - MinY;

        public double MinExtent { get; set; }

        public double MaxExtent { get; set; }
    }

    public class CentrePoint
    {
        public double X { get; set; }

        public double Y { get; set; }

    }
}
