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
                .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.Attributes["ZIPCODE"].ToString()))
                .ForMember(dest => dest.BldgZip, opt => opt.MapFrom(src => src.Attributes["BLDGZIP"].ToString()))
                .ForMember(dest => dest.PostOfficeName, opt => opt.MapFrom(src => src.Attributes["PO_NAME"].ToString()))
                .ForMember(dest => dest.Population, opt => opt.MapFrom(src => int.Parse(src.Attributes["POPULATION"].ToString())))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.Attributes["STATE"].ToString()))
                .ForMember(dest => dest.County, opt => opt.MapFrom(src => src.Attributes["COUNTY"].ToString()))
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Attributes["AREA"].ToString()))
                .ForMember(dest => dest.StateFibs, opt => opt.MapFrom(src => src.Attributes["ST_FIPS"].ToString()))
                .ForMember(dest => dest.CityFibs, opt => opt.MapFrom(src => src.Attributes["CTY_FIPS"].ToString()))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Attributes["URL"].ToString()))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["SHAPE_AREA"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["SHAPE_LEN"].ToString())))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }

    }
}
