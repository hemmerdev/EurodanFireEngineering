using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;
using System.Runtime.CompilerServices;

namespace Eurodan.Core.Services
{
    public class ClientService
    {
        private readonly IRepository<Client> _clientRepository;

        public ClientService(IRepository<Client> repository)
        {
            _clientRepository = repository;
        }

        public List<Client> GetAllClients()
        {
            return _clientRepository.GetAll().ToList();
        }

        public int RegisterClient(Client client)
        {
            ValidateClient(client);
            _clientRepository.Add(client);
            return client.ClientId;
        }

        public bool UpdateClient(Client client)
        {
            ValidateClient(client);
            Client? existingClient = _clientRepository.GetById(client.ClientId);

            if (existingClient == null) return false;
            _clientRepository.Update(client);

            return true;
        }

        private void ValidateClient(Client client)
        {
            ArgumentNullException.ThrowIfNull(client);
            if (string.IsNullOrWhiteSpace(client.CompanyName))
            {
                throw new ArgumentException("Company name is required.");
            }

            if (client.CompanyName != null && client.CompanyName.Length > 150)
            {
                throw new ArgumentException("Company name cannot exceed 150 characters.");
            }

            if (client.ContactName != null && client.ContactName.Length > 100)
            {
                throw new ArgumentException("Contact name cannot exceed 100 characters.");
            }

            if (client.Phone != null && client.Phone.Length > 30)
            {
                throw new ArgumentException("Phone number cannot exceed 30 characters.");
            }

            if (client.Email != null && client.Email.Length > 254)
            {
                throw new ArgumentException("Email cannot exceed 254 characters");
            }
        }
    }
}
