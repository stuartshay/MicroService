using System;

namespace MicroService.Service.Models.Enum.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FeatureNameAttribute : Attribute
    {
        public FeatureNameAttribute(string attributeName)
        {
            AttributeName = attributeName;
        }

        public string AttributeName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FeatureCollectionAttribute : Attribute
    {
        public string Name { get; set; } = string.Empty;
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FeatureCollectionExcludeAttribute : Attribute
    {
        public string Name { get; set; } = string.Empty;
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MappingKeyAttribute : Attribute
    {
        public MappingKeyAttribute(string attributeName)
        {
            AttributeName = attributeName;
        }

        public string AttributeName { get; set; }
    }
}
