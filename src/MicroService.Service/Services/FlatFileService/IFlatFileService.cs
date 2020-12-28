using System.Collections.Generic;
using System.Threading.Tasks;
using MicroService.Service.Models.Base;

namespace MicroService.Service.Services.FlatFileService
{
    public interface IFlatFileService<out T> where T : FlatFileBase
    {
        IEnumerable<FlatFileBase> GetAll();

    }
}
