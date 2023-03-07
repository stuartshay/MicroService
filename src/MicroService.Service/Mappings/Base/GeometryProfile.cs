using AutoMapper;
using MicroService.Service.Models.Base;
using NetTopologySuite.Geometries;

namespace MicroService.Service.Mappings.Base
{
    public class GeometryProfile : Profile
    {
        public GeometryProfile()
        {
            CreateMap<Envelope, BoundingBox>();
            CreateMap<Coordinate, CentrePoint>();
        }
    }
}
