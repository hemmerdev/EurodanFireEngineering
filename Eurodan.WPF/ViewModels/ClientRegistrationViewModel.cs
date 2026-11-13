using Eurodan.Core.Services;
using Eurodan.Core.Models;
using Eurodan.WPF.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Eurodan.WPF.ViewModels
{
    public class ClientRegistrationViewModel : ViewModelBase
    {
        private readonly ClientService _clientService;

        private string _companyName = string.Empty;
        public string CompanyName
        {
            get => _companyName;
            set
            {
                _companyName = value;
                OnPropertyChanged();
            }
        }

        private string? _contactName;
        public string? ContactName
        {
            get => _contactName;
            set
            {
                _contactName = value;
                OnPropertyChanged();
            }
            
        }

        private string? _email;
        public string? Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        private string? _phone;
        public string? Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand RegisterCommand { get; }

        public ClientRegistrationViewModel(ClientService clientService)
        {
            _clientService = clientService;
            RegisterCommand = new RelayCommand(RegisterClient);
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
