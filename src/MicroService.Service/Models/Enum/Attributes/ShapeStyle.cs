namespace MicroService.Service.Models.Enum.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
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
