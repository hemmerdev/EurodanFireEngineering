using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Tests.Fakes
{
    public class FakeClientRepository : IRepository<Client>
    {
        public List<Client> Clients { get; } = new();
        public bool AddCalled { get; private set; }

        public void Add(Client entity)
        {
            AddCalled = true;
            entity.ClientId = 12;
            Clients.Add(entity);
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Client> GetAll()
        {
            return Clients;
        }

        public Client? GetById(int id)
        {
            foreach (Client client in Clients)
            {
                if (client.ClientId == id)
                {
                    return client;
                }
            }
            return null;
        }

        public void Update(Client entity)
        {
            throw new NotImplementedException();
        }
    }
}
