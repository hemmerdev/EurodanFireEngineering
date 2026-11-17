using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using System.Windows.Input;

namespace Eurodan.WPF.ViewModels
{
    public class ClientViewModel : ViewModelBase
    {
        private readonly ClientService _clientService;

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

        public ClientViewModel(ClientService clientService)
        {
            _clientService = clientService;
            RegisterClientCommand = new RelayCommand(RegisterClient);
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

                // Clearing the fields to avoid doubleclicking for double registering
                CompanyName = "";
                ContactName = "";
                Email = "";
                Phone = "";
            }
            catch (ArgumentException ex)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception)
            {
                StatusMessage = "Could not save the client.";
            }
        }
    }
}
