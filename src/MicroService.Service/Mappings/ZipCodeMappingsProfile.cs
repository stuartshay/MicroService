using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;

namespace MicroService.Service.Mappings
{
    public class ZipCodeMappingsProfile : ShapeProfile<ZipCodeShape>
    {
        public ZipCodeMappingsProfile()
        {
            CreateMap<Feature, ZipCodeShape>()
                .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.Attributes["ZIPCODE"].ToString() ?? string.Empty))
                .ForMember(dest => dest.BldgZip, opt => opt.MapFrom(src => src.Attributes["BLDGZIP"].ToString() ?? string.Empty))
                .ForMember(dest => dest.PostOfficeName, opt => opt.MapFrom(src => src.Attributes["PO_NAME"].ToString() ?? string.Empty))
                .ForMember(dest => dest.Population, opt => opt.MapFrom(src => int.Parse(src.Attributes["POPULATION"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.Attributes["STATE"].ToString() ?? string.Empty))
                .ForMember(dest => dest.County, opt => opt.MapFrom(src => src.Attributes["COUNTY"].ToString() ?? string.Empty))
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Attributes["AREA"].ToString() ?? string.Empty))
                .ForMember(dest => dest.StateFibs, opt => opt.MapFrom(src => src.Attributes["ST_FIPS"].ToString() ?? string.Empty))
                .ForMember(dest => dest.CityFibs, opt => opt.MapFrom(src => src.Attributes["CTY_FIPS"].ToString() ?? string.Empty))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Attributes["URL"].ToString() ?? string.Empty))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["SHAPE_AREA"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["SHAPE_LEN"].ToString() ?? string.Empty)))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }

    }
}
