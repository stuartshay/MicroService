using System.Collections.Generic;
using MicroService.Data.Models;

namespace MicroService.Data.Repository
{
    public interface IRepository<T> where T : BaseEntity
    {
        void Add(T item);

        void Remove(int id);

        void Update(T item);

        T FindById(int id);

        IEnumerable<T> FindAll();
    }
}
