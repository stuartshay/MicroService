using Xunit;

namespace MicroService.Test.Integration.Interfaces
{
    internal interface IShapeTest
    {
        [Trait("Category", "Integration")]
        void Get_Feature_Point_Lookup(double x, double y, string expected, int? lookupExpected);

        [Trait("Category", "Integration")]
        void Get_Feature_Attribute_Lookup(string value1, string value2, string expected);


        [Trait("Category", "Integration")]
        void Get_Feature_Point_Lookup_Not_Found(double x, double y, string expected);



    }
}
