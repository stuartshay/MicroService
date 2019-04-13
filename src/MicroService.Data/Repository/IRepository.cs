using System.Collections.Generic;
using System.Threading.Tasks;
using MicroService.Data.Models;

namespace MicroService.Data.Repository
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task Add(T item);

        Task Remove(int id);

        Task Update(T item);

        Task<T> FindById(int id);

        Task<IEnumerable<T>> FindAll();
    }
}
