using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;
using System.Net.Mail;

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

            client.CompanyName = client.CompanyName.Trim();

            if (client.CompanyName.Length > 150)
            {
                throw new ArgumentException("Company name cannot exceed 150 characters.");
            }

            if (client.ContactName != null)
            {
                client.ContactName = client.ContactName.Trim();
                if (client.ContactName.Length > 100)
                {
                    throw new ArgumentException("Contact name cannot exceed 100 characters.");
                }
            }

            if (client.Phone != null)
            {
                client.Phone = client.Phone.Trim();
                ValidatePhone(client.Phone);
            }

            if (client.Email != null)
            {
                client.Email = client.Email.Trim();
                ValidateEmail(client.Email);
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

        private static void ValidatePhone(string? phoneNumber)  // Validates the phone number format and length, not specfic country formats.
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return; // Phone is optional, so no validation needed for null or whitespace
            }

            if (phoneNumber.Length > 30)
            {
                throw new ArgumentException("Phone number cannot exceed 30 characters.");
            }

            string cleanedPhoneNumber = phoneNumber    // Local copy of the phone number used to validate
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(".", "");

            bool isInternational = cleanedPhoneNumber.StartsWith("+");

            string digits = isInternational ?
                cleanedPhoneNumber.Substring(1) : cleanedPhoneNumber;       // Remove the '+' from international numbers before digit check

            if (digits.Length == 0)
            {
                throw new ArgumentException("Phone number must contain digits.");
            }

            foreach (char c in digits) // Check that all characters are digits
            {
                if (!char.IsDigit(c))
                {
                    throw new ArgumentException(
                        "Enter a phone number using digits, an optional leading +, and spaces, hyphens, brackets or dots");
                }
            }
        }

        private static void ValidateEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return; // Email is optional, so no validation needed for null or whitespace
            }

            if (email.Length > 254)
            {
                throw new ArgumentException("Email cannot exceed 254 characters.");
            }

            if (!MailAddress.TryCreate(email, out _)) // Use MailAddress.TryCreate to validate the email format.
                                                      // Discard the parsed MailAddress, we only need the bool result.
            {
                throw new ArgumentException("Enter a valid email address, for example: name@example.com.");
            }
        }
    }
}
