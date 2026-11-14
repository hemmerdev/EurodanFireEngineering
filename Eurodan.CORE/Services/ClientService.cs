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

            _repository.Add(client);
            return client.ClientId;
        }
    }
}
