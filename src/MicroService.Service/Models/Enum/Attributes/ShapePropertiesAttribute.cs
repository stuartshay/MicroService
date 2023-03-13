using System;

namespace MicroService.Service.Models.Enum.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ShapePropertiesAttribute : Attribute
    {
        public ShapeProperties ShapeProperties { get; }

        public ShapePropertiesAttribute(ShapeProperties shapeProperties)
        {
            ShapeProperties = shapeProperties;
        }
    }
}
