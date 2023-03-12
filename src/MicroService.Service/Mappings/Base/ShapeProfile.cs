using AutoMapper;
using MicroService.Service.Models.Enum.Attibutes;
using System.Collections.Generic;
using System.Linq;

namespace MicroService.Service.Mappings.Base
{
    public abstract class ShapeProfile<TShape> : Profile where TShape : class
    {
        protected ShapeProfile()
        {
            CreateMap<TShape, IDictionary<string, object>>()
                .ConvertUsing(shape => shape.GetType().GetProperties()
                    .Where(prop => !prop.GetCustomAttributes(typeof(FeatureCollectionExcludeAttribute), false).Any())
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(shape)));

        }
    }
}
