using MicroService.Data.Enum;
using MicroService.Service.Mappings.Base;
using MicroService.Service.Models;
using NetTopologySuite.Features;
using System;
using System.Text.RegularExpressions;

namespace MicroService.Service.Mappings
{
    public class HistoricDistrictShapeProfile : ShapeProfile<HistoricDistrictShape>
    {
        public HistoricDistrictShapeProfile()
        {
            CreateMap<Feature, HistoricDistrictShape>()
                .ForMember(dest => dest.LPNumber, opt => opt.MapFrom(src => src.Attributes["lp_number"].ToString()))
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Attributes["area_name"].ToString()))
                .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.Attributes["borough"].ToString()))
                .ForMember(dest => dest.BoundaryName, opt =>
                    opt.MapFrom(src => string.IsNullOrEmpty(src.Attributes["boundary_n"].ToString()) ? null
                        : Regex.Replace(src.Attributes["boundary_n"].ToString(), @"\u0000", string.Empty)))
                .ForMember(dest => dest.CalendarDate, opt => opt.MapFrom(src => src.Attributes["caldate"].ToString()))



                //.ForMember(dest => dest.Current, opt => opt.MapFrom(src => src.Attributes["current"].ToString()))
                //.ForMember(dest => dest.DesignationDate, opt => opt.MapFrom(src => src.Attributes["desdate"].ToString()))
                //.ForMember(dest => dest.Extension, opt => opt.MapFrom(src => src.Attributes["extension"].ToString()))
                //.ForMember(dest => dest.LastAction, opt => opt.MapFrom(src => src.Attributes["last_actio"].ToString()))
                //.ForMember(dest => dest.OtherHearing, opt => opt.MapFrom(src => src.Attributes["other_hear"].ToString()))
                //.ForMember(dest => dest.PublicHearing, opt => opt.MapFrom(src => src.Attributes["public_hea"].ToString()))
                //.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Attributes["status_of_"].ToString()))




                .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => (int)Enum.Parse(typeof(Borough), src.Attributes["borough"].ToString())))
                .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_area"].ToString())))
                .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.Attributes["shape_leng"].ToString())))
                .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src => src.Geometry))
                .ForMember(dest => dest.Feature, opt => opt.Ignore());
        }
    }

}
