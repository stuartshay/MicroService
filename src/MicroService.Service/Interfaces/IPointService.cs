using MicroService.Service.Models.Base;

namespace MicroService.Service.Interfaces
{
    public interface IPointService<out T> where T : ShapeBase
    {

    }
}
