namespace Eurodan.Core.Interfaces
{
    public interface IRepository<T> : IReadOnlyRepository<T> where T : class
    {
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
    }
}
