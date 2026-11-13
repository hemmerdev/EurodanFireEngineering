using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;
using System.Runtime.CompilerServices;

namespace Eurodan.Core.Services
{
    public class ClientService
    {
        private readonly IRepository<Client> _repository;

        public ClientService(IRepository<Client> repository)
        {
            _repository = repository;
        }

        public int RegisterClient(Client client)
        {
            ArgumentNullException.ThrowIfNull(client);
            if (string.IsNullOrWhiteSpace(client.CompanyName) && string.IsNullOrWhiteSpace(client.ContactName))
            {
                throw new ArgumentException("Enter a company name or contact name.");
            }

            _repository.Add(client);
            return client.ClientId;
        }
    }
}
