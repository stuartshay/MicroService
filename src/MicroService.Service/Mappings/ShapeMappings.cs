using AutoMapper;
using MicroService.Service.Models.Base;
using System.Collections.Generic;

namespace MicroService.Service.Mappings
{
    public class ShapeMappings : Profile
    {
        public ShapeMappings()
        {
            CreateMap<KeyValuePair<string, object>, ShapeBase>()
                .ConvertUsing(new KeyValuePairToShapeClassConverter<ShapeBase>());


        }

        public void ValidateMappings()
        {

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
