using MicroService.Service.Services.Streets;
using Xunit;

namespace MicroService.Test.Unit
{
    public class StreetNamesTest
    {
        [Theory(DisplayName = "Street names - common spellings become the Street Centerline's")]
        [InlineData("Fifth Avenue", "5 AVE")]
        [InlineData("5th Ave", "5 AVE")]
        [InlineData("  madison   avenue ", "MADISON AVE")]
        [InlineData("W. 42nd Street", "W 42 ST")]
        [InlineData("West Street", "WEST ST")]
        [InlineData("St. Nicholas Avenue", "ST NICHOLAS AVE")]
        [InlineData("Avenue of the Americas", "AVE OF THE AMERICAS")]
        [InlineData("Central Park West", "CENTRAL PARK WEST")]
        public void Normalize(string name, string expected) => Assert.Equal(expected, StreetNames.Normalize(name));

        [Fact(DisplayName = "Street names - a direction is tried as the word and as the letter, the name as written first")]
        public void Spellings()
        {
            Assert.Equal(["WEST 42 ST", "W  42 ST"], StreetNames.Spellings("WEST 42 ST"));
            Assert.Equal(["W  42 ST", "WEST 42 ST"], StreetNames.Spellings("W 42 ST"));
            Assert.Equal(["WEST ST", "W  ST"], StreetNames.Spellings("WEST ST"));
            Assert.Equal(["CENTRAL PARK WEST", "CENTRAL PARK W"], StreetNames.Spellings("CENTRAL PARK WEST"));
            Assert.Equal(["PARK AVE S", "PARK AVE SOUTH"], StreetNames.Spellings("PARK AVE S"));
            Assert.Equal(["5 AVE"], StreetNames.Spellings("5 AVE"));
            Assert.Equal(["BROADWAY"], StreetNames.Spellings("BROADWAY"));

            // Two words: a trailing direction is part of the name (AVE S is not AVE SOUTH)
            Assert.Equal(["AVE S"], StreetNames.Spellings("AVE S"));
        }

        [Fact(DisplayName = "Street names - one key for every spelling of a street")]
        public void Key()
        {
            Assert.Equal(StreetNames.Key("W 42 ST"), StreetNames.Key("WEST 42 ST"));
            Assert.NotEqual(StreetNames.Key("W 42 ST"), StreetNames.Key("E 42 ST"));
            Assert.Equal("W 42 ST", StreetNames.Collapse("W  42 ST"));
            Assert.Equal(string.Empty, StreetNames.Collapse(null!));
        }

        [Theory(DisplayName = "Street names - letters, figures, spaces, apostrophes and hyphens, at most 64")]
        [InlineData("5 AVE", true)]
        [InlineData("Adam Clayton Powell Jr. Blvd", true)]
        [InlineData("St John's Pl", true)]
        [InlineData("", false)]
        [InlineData("5 AVE'; DROP", false)]
        [InlineData("5 AVE%", false)]
        public void IsValid(string name, bool expected) => Assert.Equal(expected, StreetNames.IsValid(name));

        [Fact(DisplayName = "Street names - an empty name has itself as its only spelling")]
        public void Spellings_OfNothing() => Assert.Equal([string.Empty], StreetNames.Spellings(string.Empty));

        [Fact(DisplayName = "Street names - longer than 64 is not a name")]
        public void IsValid_TooLong() => Assert.False(StreetNames.IsValid(new string('A', 65)));
    }
}
