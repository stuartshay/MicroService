using AutoMapper;
using MicroService.Service.Mappings;

namespace MicroService.Test.Mappings
{
    public abstract class BaseMapperTest<T> where T : class
    {
        protected readonly IMapper Mapper;

        protected BaseMapperTest()
        {
            var mapperConfig = new MapperConfiguration(config =>
            {
                config.AddProfile<ShapeMappings>();
            });

            Mapper = mapperConfig.CreateMapper();
        }
    }
}
