using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Tests.Fakes
{
    public class FakeClientRepository : IRepository<Client>
    {
        public bool AddCalled { get; private set; }

        public void Add(Client entity)
        {
            AddCalled = true;
            entity.ClientId = 12;
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Client> GetAll()
        {
            throw new NotImplementedException();
        }

        public Client? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(Client entity)
        {
            throw new NotImplementedException();
        }
    }
}
