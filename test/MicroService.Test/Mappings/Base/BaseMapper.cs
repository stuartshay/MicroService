using AutoMapper;
using MicroService.Service.Mappings;

namespace MicroService.Test.Mappings.Base
{
    public abstract class BaseMapper<T> where T : class
    {
        protected readonly IMapper Mapper;

        protected BaseMapper()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddMaps(typeof(FeatureToBoroughBoundaryShapeProfile).Assembly);
            });

            Mapper = mapperConfig.CreateMapper();
        }
    }
}
