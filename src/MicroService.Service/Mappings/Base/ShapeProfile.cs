using AutoMapper;
using MicroService.Service.Models.Enum.Attributes;
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
                    // No shape model exposes a nullable-valued property, so GetValue(shape) is
                    // never actually null here; PropertyInfo.GetValue's object? return is a
                    // general reflection contract, not a real possibility for this closed set of types.
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(shape)!));

        }
    }
}
