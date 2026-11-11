namespace Eurodan.Core.Interfaces
{
    public interface IReadOnlyRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T? GetById(int id);
    }
}
