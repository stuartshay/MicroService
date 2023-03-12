namespace MicroService.Service.Models.Enum.Attributes
{
    public class ShapeStyleAttribute : System.Attribute
    {
        public ShapeStyleAttribute(Color color)
        {
            Color = color;
        }

        public Color Color { get; set; }
    }

    public enum Color
    {
        Red,
        Blue,
        Green,
        Yellow,
        Black,
        Grey,
    }

}
