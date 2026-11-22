using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;
using System.Linq;

namespace MicroService.Service.Mappings
{
    public class PrivatelyOwnedPublicSpaceShapeProfile : ShapeProfile<PrivatelyOwnedPublicSpaceShape>
    {
        protected PrivatelyOwnedPublicSpaceShapeProfile()
        {
        }

        public static PrivatelyOwnedPublicSpaceShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);
            return new PrivatelyOwnedPublicSpaceShape
            {
                PopsNumber = GetString(src, "pops_numbe"),
                BuildingName = GetString(src, "building_n"),
                Address = GetString(src, "building_a"),
                Location = GetString(src, "building_l"),
                PublicSpaceType = GetString(src, "public_spa"),
                HoursOfAccess = GetString(src, "hour_of_ac"),
                Amenities = GetString(src, "amenities_"),
                YearCompleted = GetString(src, "year_compl"),
                BoroName = GetString(src, "borough_na"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }

    public class GreenstreetShapeProfile : ShapeProfile<GreenstreetShape>
    {
        protected GreenstreetShapeProfile()
        {
        }

        public static GreenstreetShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);
            return new GreenstreetShape
            {
                PropertyNumber = GetString(src, "gispropnum"),
                SiteName = GetString(src, "sitename"),
                Location = GetString(src, "location"),
                Borough = GetString(src, "borough"),
                Status = GetString(src, "featuresta"),
                Acres = GetDouble(src, "acres"),
                ShapeArea = GetDouble(src, "starea"),
                ShapeLength = GetDouble(src, "stlength"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }

    public class LibraryShapeProfile : ShapeProfile<LibraryShape>
    {
        protected LibraryShapeProfile()
        {
        }

        public static LibraryShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);
            return new LibraryShape
            {
                Name = GetString(src, "name"),
                System = GetString(src, "system"),
                Address = Address(src),
                City = GetString(src, "city"),
                ZipCode = GetString(src, "zip"),
                Url = GetString(src, "url"),
                Bbl = GetString(src, "bbl"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }

        /// <summary>The house number and street, e.g. 203 West 115th Street.</summary>
        internal static string Address(Feature src) =>
            string.Join(' ', new[] { GetString(src, "housenum"), GetString(src, "streetname") }
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => part.Trim()));
    }

    public class PostOfficeShapeProfile : ShapeProfile<PostOfficeShape>
    {
        protected PostOfficeShapeProfile()
        {
        }

        public static PostOfficeShape Map(Feature src)
        {
            ArgumentNullException.ThrowIfNull(src);
            return new PostOfficeShape
            {
                Name = GetString(src, "name"),
                Address = LibraryShapeProfile.Address(src),
                City = GetString(src, "city"),
                ZipCode = GetString(src, "zip"),
                Bbl = GetString(src, "bbl"),
                BoundingBox = MapBoundingBox(src.BoundingBox),
                Geometry = src.Geometry,
            };
        }
    }
}
