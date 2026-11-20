using MicroService.Service.Services.Subway;
using Xunit;

namespace MicroService.Test.Unit
{
    [Trait("Category", "Unit")]
    public class SubwayRoutesTest
    {
        [Fact(DisplayName = "Subway routes - numbered first in number order, then letters")]
        public void Order_SortsNumbersThenLetters()
        {
            string[] routes = ["Z", "SIR", "5 Peak", "A", "7", "S", "", "1", "5", "SF"];

            var sorted = routes.OrderBy(r => r, SubwayRoutes.Order).ToList();

            Assert.Equal(["1", "5", "5 Peak", "7", "", "A", "S", "SF", "SIR", "Z"], sorted);
        }

        [Fact(DisplayName = "Subway routes - the comparer is consistent for equal, null and empty codes")]
        public void Order_HandlesEqualNullAndEmpty()
        {
            var order = SubwayRoutes.Order;
            var code = "A";

            Assert.Equal(0, order.Compare(code, code));
            Assert.Equal(0, order.Compare("A", new string('A', 1)));
            Assert.Equal(0, order.Compare(null, null));
            Assert.True(order.Compare(null, "1") < 0);
            Assert.True(order.Compare("1", null) > 0);
            // An empty code isn't numbered, so it goes with the letters, before them
            Assert.True(order.Compare("", "7") > 0);
            Assert.True(order.Compare("", "A") < 0);
        }

        [Theory(DisplayName = "Subway routes - colors, with black text only on yellow")]
        [InlineData("2", "#EE352E", "#FFFFFF")]
        [InlineData("Q", "#FCCC0A", "#000000")]
        [InlineData("R", "#FCCC0A", "#000000")]
        [InlineData("S", "#808183", "#FFFFFF")]
        [InlineData("SR", "#808183", "#FFFFFF")]
        [InlineData("SIR", "#0039A6", "#FFFFFF")]
        public void Colors(string service, string color, string textColor)
        {
            Assert.Equal(color, SubwayRoutes.GetColor(service));
            Assert.Equal(textColor, SubwayRoutes.GetTextColor(service));
        }

        [Theory(DisplayName = "Subway routes - a code the MTA list doesn't have is grey with white text")]
        [InlineData("X")]
        [InlineData("a")]
        [InlineData("")]
        public void Colors_UnknownCodeIsGrey(string service)
        {
            Assert.Equal("#808183", SubwayRoutes.GetColor(service));
            Assert.Equal(SubwayRoutes.White, SubwayRoutes.GetTextColor(service));
        }

        [Fact(DisplayName = "Subway routes - S stands for the three shuttles")]
        public void Shuttles()
        {
            Assert.Equal("S", SubwayRoutes.Shuttle);
            Assert.Equal(["SF", "SR", "ST"], SubwayRoutes.Shuttles);
        }
    }
}
