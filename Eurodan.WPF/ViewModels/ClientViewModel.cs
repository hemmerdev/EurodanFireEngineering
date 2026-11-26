using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Eurodan.WPF.ViewModels
{
    public class ClientViewModel : ViewModelBase
    {
        private readonly ClientService _clientService;

        public ObservableCollection<Client> Clients { get; } = new();

        private Client? _selectedClient;

        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (_selectedClient != value)
                {
                    _selectedClient = value;
                    OnPropertyChanged();

                    CompanyName = value?.CompanyName ?? string.Empty;
                    ContactName = value?.ContactName;
                    Email = value?.Email;
                    Phone = value?.Phone;
                }
            }
        }

        private string _companyName = string.Empty;
        public string CompanyName
        {
            get => _companyName;
            set
            {
                if (_companyName != value)
                {
                    _companyName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _contactName;
        public string? ContactName
        {
            get => _contactName;
            set
            {
                if (_contactName != value)
                {
                    _contactName = value;
                    OnPropertyChanged();
                }

            }
        }

        private string? _email;
        public string? Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _phone;
        public string? Phone
        {
            get => _phone;
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand RegisterClientCommand { get; }
        public ICommand UpdateClientCommand { get; }
        public ICommand DeleteClientCommand { get; }

        public ClientViewModel(ClientService clientService)
        {
            _clientService = clientService;
            RegisterClientCommand = new RelayCommand(RegisterClient);
            UpdateClientCommand = new RelayCommand(UpdateClient);
            DeleteClientCommand = new RelayCommand(DeleteClient);
            LoadClients();
        }

        private void RegisterClient(object? parameter)
        {
            Client client = new Client
            {
                CompanyName = CompanyName,
                ContactName = ContactName,
                Email = Email,
                Phone = Phone
            };

            try
            {
                int id = _clientService.RegisterClient(client);

                StatusMessage = $"New client registered! ID: {id}";

                // Clearing the fields to avoid double-clicking for double registering
                CompanyName = "";
                ContactName = "";
                Email = "";
                Phone = "";

                SelectedClient = null;
            }
            catch (ArgumentException ex)
            {
                StatusMessage = ex.Message;
                return;
            }
            catch (Exception)
            {
                StatusMessage = "Could not save the client.";
                return;
            }

            try
            {
                LoadClients();
            }
            catch (Exception)
            {
                StatusMessage = "The client was registered, but the list could not be refreshed.";
            }
        }

        private void UpdateClient(object? parameter)
        {
            if (SelectedClient == null)
            {
                StatusMessage = "Please select a client to update.";
                return;
            }

            var updatedClient = new Client
            {
                ClientId = SelectedClient.ClientId,
                CompanyName = CompanyName,
                ContactName = ContactName,
                Email = Email,
                Phone = Phone
            };

            try
            {
                bool updated = _clientService.UpdateClient(updatedClient);

                if (!updated)
                {
                    StatusMessage = "The selected client could not be found.";
                    return;
                }
            }
            catch (ArgumentException ex)
            {
                StatusMessage = ex.Message;
                return;
            }
            catch (Exception)
            {
                StatusMessage = "Could not update the client.";
                return;
            }

            try
            {
                SelectedClient = null;
                LoadClients();
                StatusMessage = "Client updated successfully.";
            }
            catch (Exception)
            {
                StatusMessage = "The client was updated, but the list could not be refreshed.";
            }
        }
        
        private void DeleteClient(object? parameter)
        {
            if (SelectedClient == null)
            {
                StatusMessage = "Please select a client to delete.";
                return;
            }

            var clientToDelete = new Client
            {
                ClientId = SelectedClient.ClientId,
                CompanyName = SelectedClient.CompanyName,
                ContactName = SelectedClient.ContactName,
                Email = SelectedClient.Email,
                Phone = SelectedClient.Phone
            };

            try
            {
                bool deleted = _clientService.DeleteClient(SelectedClient.ClientId);

                if (!deleted)
                {
                    StatusMessage = $"The client {clientToDelete.CompanyName} could not be deleted.";
                    return;
                }
            }
            catch (ArgumentException ex)
            {
                StatusMessage = ex.Message;
                return;
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = ex.Message;
                return;
            }
            catch (Exception)
            {
                StatusMessage = "Could not delete the client.";
                return;
            }

            // Successfully deleted, clear the fields and reload the issues
            try
            {
                SelectedClient = null;
                ContactName = "";
                CompanyName = "";
                Email = "";
                Phone = "";
                LoadClients();
                StatusMessage = $"{clientToDelete.CompanyName} deleted successfully.";
            }
            catch (Exception)
            {
                StatusMessage = $"The client was deleted, but could not reload the list.";
                return;
            }
        }

        private void LoadClients()
        {
            var newClients = _clientService.GetAllClients();

            Clients.Clear();
            foreach (var client in newClients)
            {
                Clients.Add(client);
            }
        }
    }
}
