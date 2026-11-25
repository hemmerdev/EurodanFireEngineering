using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Core.Services
{
    public class ClientService
    {
        private readonly IRepository<Client> _clientRepository;
        private readonly IReadOnlyRepository<Project> _projectRepository;

        public ClientService(IRepository<Client> clientRepository, IReadOnlyRepository<Project> projectRepository)
        {
            _clientRepository = clientRepository;
            _projectRepository = projectRepository;
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
            if (existingClient == null)
            {
                return false;
            }

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

        public bool DeleteClient(int clientId)
        {
            if (_clientRepository.GetById(clientId) == null)
            {
                return false;
            }

            if (_projectRepository.GetAll()
                .Any(project => project.ClientId == clientId))
            {
                throw new InvalidOperationException("The client cannot be deleted, as it is assigned to a project.");
            }

            _clientRepository.Delete(clientId);
            return true;
        }
    }
}
