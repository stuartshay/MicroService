using MicroService.Service.Models.Base;
using System.Collections.Generic;

namespace MicroService.Service.Services.FlatFileService
{
    public interface IFlatFileService
    {
        IEnumerable<FlatFileBase> GetAll();

    }
}
