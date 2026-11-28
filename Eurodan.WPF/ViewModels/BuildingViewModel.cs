using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using System.Collections.ObjectModel;
using System.Data.Common;
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

                    DeleteBuildingCommand.RaiseCanExecuteChanged();
                    UpdateBuildingCommand.RaiseCanExecuteChanged();

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
        public RelayCommand UpdateBuildingCommand { get; }
        public RelayCommand DeleteBuildingCommand { get; }

        public BuildingViewModel(BuildingService buildingService)
        {
            _buildingService = buildingService;
            RegisterBuildingCommand = new RelayCommand(RegisterBuilding);
            UpdateBuildingCommand = new RelayCommand(
                UpdateBuilding,
                _ => SelectedBuilding != null);
            DeleteBuildingCommand = new RelayCommand(
                DeleteBuilding,
                _ => SelectedBuilding != null);
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

                // Clearing the fields to avoid double-clicking for double registering
                Name = "";
                Address = "";

                SelectedBuilding = null;
            }
            catch (ArgumentException ex)
            {
                StatusMessage = ex.Message;
                return;
            }
            catch (DbException)
            {
                StatusMessage = "Could not save the building because of a database error. Please try again.";
                return;
            }
            catch (Exception)
            {
                StatusMessage = "An unexpected error occurred while saving the building.";
                return;
            }

            try
            {
                LoadBuildings();
            }
            catch (Exception)
            {
                StatusMessage = "The building was registered, but the list could not be refreshed.";
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
            catch (DbException)
            {
                StatusMessage = "Could not update the building because of a database error. Please try again.";
                return;
            }
            catch (Exception)
            {
                StatusMessage = "An unexpected error occurred while updating the building.";
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

        private void DeleteBuilding(object? parameter)
        {
            if (SelectedBuilding == null)
            {
                StatusMessage = "Please select a building to delete.";
                return;
            }

            int buildingId = SelectedBuilding.BuildingId;
            string name = SelectedBuilding.Name;

            try
            {
                bool deleted = _buildingService.DeleteBuilding(buildingId);

                if (!deleted)
                {
                    StatusMessage = $"The building {name} could not be deleted.";
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
            catch (DbException)
            {
                StatusMessage = "Could not delete the building because of a database error. Please try again.";
                return;
            }
            catch (Exception)
            {
                StatusMessage = "An unexpected error occurred while deleting the building.";
                return;
            }

            // Successfully deleted, clear the fields and reload the issues
            try
            {
                SelectedBuilding = null;
                Name = "";
                Address = "";
                LoadBuildings();
                StatusMessage = $"{name} deleted successfully.";
            }
            catch (Exception)
            {
                StatusMessage = $"The building was deleted, but could not reload the list.";
                return;
            }
        }

        private void LoadBuildings()
        {
            var newBuildings = _buildingService.GetAllBuildings();

            Buildings.Clear();
            foreach (var building in newBuildings)
            {
                Buildings.Add(building);
            }
        }

        public void Refresh()
        {
            SelectedBuilding = null;
            Name = "";
            Address = "";
            StatusMessage = "";

            try
            {
                LoadBuildings();
            }
            catch (DbException)
            {
                StatusMessage = "Could not refresh the building list because of a database error. Please try again.";
            }
            catch (Exception)
            {
                StatusMessage = "An unexpected error occurred while refreshing the building list.";
            }
        }
    }
}
