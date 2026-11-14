using Eurodan.Core.Services;

namespace Eurodan.WPF.ViewModels
{
    public class ClientViewModel : ViewModelBase
    {
        private readonly ClientService _clientService;
        
        public ClientViewModel(ClientService clientService)
        {
            _clientService = clientService;
        }

        public ClientRegistrationViewModel CreateClientRegistrationViewModel()
        {
            ClientRegistrationViewModel clientRegistrationViewModel = new(_clientService);
            return clientRegistrationViewModel;
        }
    }
}
