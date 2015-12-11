using System.Collections.Generic;


namespace APLPX.Client.Contracts
{
    public interface IRepository<T> : ILoadList<T>
    {
        T Get(int id );
        bool Insert( T entity );
        bool Update( T entity );
        bool Delete(int id);
    }

    public interface ILoadList<T>
    {
        List<T> LoadList(int ownerId);
    }
}