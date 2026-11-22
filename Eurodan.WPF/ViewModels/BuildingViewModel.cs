using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Eurodan.WPF.ViewModels
{
    public class BuildingViewModel : ViewModelBase
    {
        private readonly BuildingService _buildingService;

        public ObservableCollection<Building> Buildings { get; } = new();

        private Building? _selectedBuilding;

        public Building? SelectedBuilding
        {
            get => _selectedBuilding;
            set
            {
                if (_selectedBuilding != value)
                {
                    _selectedBuilding = value;
                    OnPropertyChanged();

                    Name = value?.Name ?? string.Empty;
                    Address = value?.Address ?? string.Empty;
                }
            }
        }

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
        public ICommand UpdateBuildingCommand { get; }

        public BuildingViewModel(BuildingService buildingService)
        {
            _buildingService = buildingService;
            RegisterBuildingCommand = new RelayCommand(RegisterBuilding);
            UpdateBuildingCommand = new RelayCommand(UpdateBuilding);
            LoadBuildings();
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

                SelectedBuilding = null;
                LoadBuildings();
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

        private void UpdateBuilding(object? parameter)
        {
            if (SelectedBuilding == null)
            {
                StatusMessage = "Please select a building to update.";
                return;
            }

            var updatedBuilding = new Building
            {
                BuildingId = SelectedBuilding.BuildingId,
                Name = Name,
                Address = Address
            };

            try
            {
                bool updated = _buildingService.UpdateBuilding(updatedBuilding);

                if (!updated)
                {
                    StatusMessage = "The selected building could not be found.";
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
                StatusMessage = "Could not update the building.";
                return;
            }

            try
            {
                SelectedBuilding = null;
                LoadBuildings();
                StatusMessage = "Building updated successfully.";
            }
            catch (Exception)
            {
                StatusMessage = "The building was updated, but the list could not be refreshed.";
            }
        }

        private void LoadBuildings()
        {
            Buildings.Clear();
            foreach (var building in _buildingService.GetAllBuildings())
            {
                Buildings.Add(building);
            }
        }
    }
}
