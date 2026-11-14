using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using Eurodan.Core.Models;
using System.Windows.Input;

namespace Eurodan.WPF.ViewModels
{
    public class BuildingRegistrationViewModel : ViewModelBase
    {
        private readonly BuildingService _buildingService;

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _address = string.Empty;
        public string Address
        {
            get => _address;
            set
            {
                _address = value;
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

        public BuildingRegistrationViewModel(BuildingService buildingService)
        {
            _buildingService = buildingService;
            RegisterCommand = new RelayCommand(RegisterBuilding);
        }

        private void RegisterBuilding(object? parameter)
        {
            Building building = new Building
            {
                Name = Name,
                Address = Address
            };

            try
            {
                int id = _buildingService.RegisterBuilding(building);

                StatusMessage = $"New building registered. ID: {id}";
            }
            catch (ArgumentException ex)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception)
            {
                StatusMessage = "Could not save the building.";
            }
        }
    }
}
