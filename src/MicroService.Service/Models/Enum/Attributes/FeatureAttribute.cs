using System;

namespace MicroService.Service.Models.Enum.Attributes
{
    public class FeatureNameAttribute : Attribute
    {
        public FeatureNameAttribute(string attributeName)
        {
            AttributeName = attributeName;
        }

        public string AttributeName { get; set; }
    }

    public class FeatureCollectionAttribute : Attribute
    {
        public string Name { get; set; }
    }

    public class FeatureCollectionExcludeAttribute : Attribute
    {
        public string Name { get; set; }
    }

}
