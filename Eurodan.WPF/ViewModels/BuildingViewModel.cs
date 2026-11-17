using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using System.Windows.Input;

namespace Eurodan.WPF.ViewModels
{
    public class BuildingViewModel : ViewModelBase
    {
        private readonly BuildingService _buildingService;

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _address = string.Empty;
        public string Address
        {
            get => _address;
            set
            {
                if (_address != value)
                {
                    _address = value;
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

        public ICommand RegisterBuildingCommand { get; }

        public BuildingViewModel(BuildingService buildingService)
        {
            _buildingService = buildingService;
            RegisterBuildingCommand = new RelayCommand(RegisterBuilding);
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

                // Clearing the fields to avoid doubleclicking for double registering
                Name = "";
                Address = "";
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
