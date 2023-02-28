using Xunit;

namespace MicroService.Test.Integration.Interfaces
{
    internal interface IShapeTest
    {
        [Trait("Category", "Integration")]
        void Get_Shape_Properties();

        [Trait("Category", "Integration")]
        void Get_Shape_Database_Properties();

        [Trait("Category", "Integration")]
        void Get_Feature_Collection();

        [Trait("Category", "Integration")]
        void Get_Feature_Point_Lookup(double x, double y, string expected, object lookupExpected = null);

        [Trait("Category", "Integration")]
        void Get_Feature_Attribute_Lookup(object value1, object value2, string expected);

        [Trait("Category", "Integration")]
        void Get_Feature_Point_Lookup_Not_Found(double x, double y, string expected);

        [Trait("Category", "Integration")]
        void Get_Feature_List();

    }
}
