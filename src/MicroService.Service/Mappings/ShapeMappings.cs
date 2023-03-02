using AutoMapper;
using MicroService.Service.Models;
using MicroService.Service.Models.Base;
using System.Collections.Generic;

namespace MicroService.Service.Mappings
{
    public class ShapeMappings : Profile
    {
        public ShapeMappings()
        {
            CreateMap<KeyValuePair<string, object>, BoroughBoundaryShape>()
                .ConvertUsing(new KeyValuePairToShapeClassConverter<BoroughBoundaryShape>());

            //CreateMap<List<KeyValuePair<string, object>>, BoroughBoundaryShape>()
            //    .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => int.Parse(src.FirstOrDefault(kvp => kvp.Key == "BoroCode").Value.ToString())))
            //    .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.FirstOrDefault(kvp => kvp.Key == "BoroName").Value.ToString()))
            //    .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.FirstOrDefault(kvp => kvp.Key == "Shape_Area").Value.ToString())))
            //    .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.FirstOrDefault(kvp => kvp.Key == "Shape_Leng").Value.ToString())));

            //CreateMap<List<KeyValuePair<string, object>>, CommunityDistrictShape>()
            //    .ForMember(dest => dest.Cd, opt => opt.MapFrom(src => int.Parse(src.FirstOrDefault(kvp => kvp.Key == "CD").Value.ToString())))
            //    .ForMember(dest => dest.BoroCd, opt => opt.MapFrom(src => int.Parse(src.FirstOrDefault(kvp => kvp.Key == "BoroCD").Value.ToString())))
            //    .ForMember(dest => dest.BoroCode, opt => opt.MapFrom(src => int.Parse(src.FirstOrDefault(kvp => kvp.Key == "BoroCode").Value.ToString())))
            //    .ForMember(dest => dest.Borough, opt => opt.MapFrom(src => src.FirstOrDefault(kvp => kvp.Key == "Borough").Value.ToString()))
            //    .ForMember(dest => dest.BoroName, opt => opt.MapFrom(src => src.FirstOrDefault(kvp => kvp.Key == "BoroName").Value.ToString()))
            //    .ForMember(dest => dest.ShapeArea, opt => opt.MapFrom(src => double.Parse(src.FirstOrDefault(kvp => kvp.Key == "Shape_Area").Value.ToString())))
            //    .ForMember(dest => dest.ShapeLength, opt => opt.MapFrom(src => double.Parse(src.FirstOrDefault(kvp => kvp.Key == "Shape_Leng").Value.ToString())));


        }



        public T ValidateFeatureKey<T>(List<KeyValuePair<string, object>> attributes) where T : ShapeBase, new()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<KeyValuePair<string, object>, T>()
                    .ConvertUsing(new KeyValuePairToShapeClassConverter<T>());
            });

            var mapper = new Mapper(config);

            var shapeClass = mapper.Map<T>(attributes);

            return shapeClass;
        }
    }


}
