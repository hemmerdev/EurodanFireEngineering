using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;

namespace Eurodan.WPF.ViewModels
{
    public class ProjectViewModel : ViewModelBase
    {

        private readonly ProjectService _projectService;
        private string _projectNumber = string.Empty;
        private string _projectName = string.Empty;
        private string _statusMessage = string.Empty;
        private int? _selectedClientId;

        public ObservableCollection<Client> Clients { get; } = new();
        public ObservableCollection<BuildingSelectionViewModel> BuildingOptions { get; } = new();
        public ObservableCollection<BuildingClass> BuildingClasses { get; } = new();
        public ObservableCollection<TechnicalIssue> TechnicalIssues { get; } = new();

        // Form Properties
        public string ProjectNumber
        {
            get => _projectNumber;
            set
            {
                if (_projectNumber != value)
                {
                    _projectNumber = value;
                    OnPropertyChanged();
                    SaveProjectCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName != value)
                {
                    _projectName = value;
                    OnPropertyChanged();
                    SaveProjectCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? SelectedClientId
        {
            get => _selectedClientId;
            set
            {
                if (_selectedClientId != value)
                {
                    _selectedClientId = value;
                    OnPropertyChanged();
                    SaveProjectCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public RelayCommand SaveProjectCommand { get; }

        public ProjectViewModel(ProjectService projectService)
        {
            _projectService = projectService;

            SaveProjectCommand = new RelayCommand(SaveProject, CanSaveProject);
        }

        private void BuildingOptionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BuildingSelectionViewModel.IsSelected))
            {
                SaveProjectCommand.RaiseCanExecuteChanged();
            }
        }

        private bool CanSaveProject(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(ProjectNumber) &&
                   !string.IsNullOrWhiteSpace(ProjectName) &&
                   SelectedClientId.HasValue &&
                   BuildingOptions.Any(b => b.IsSelected);
        }

        private void SaveProject(object? parameter)
        {
            if (!SelectedClientId.HasValue)
            {
                StatusMessage = "Please select a client before saving the project.";
                return;
            }

            Project project = new Project
            {
                ProjectNumber = ProjectNumber,
                ProjectName = ProjectName,
                ClientId = SelectedClientId.Value,
                Buildings = BuildingOptions
                    .Where(b => b.IsSelected)
                    .Select(b => b.Building)
                    .ToList()
            };

            try
            {
                int projectId = _projectService.CreateProject(project);

                StatusMessage = $"Project created successfully with ID: {projectId}";
            }
            catch (ArgumentException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            catch (DbException)
            {
                StatusMessage = "Could not save the project. Please try again.";
            }
            catch (Exception)
            {
                StatusMessage = "An unexpected error occurred";
            }
        }

        public void LoadProjectLookups()
        {
            try
            {
                // Retrieve all clients, buildings, building classes, and technical issues from the service
                var clients = _projectService.GetAllClients();
                var buildings = _projectService.GetAllBuildings();
                var buildingClasses = _projectService.GetAllBuildingClasses();
                var technicalIssues = _projectService.GetAllTechnicalIssues();

                int? selectedClientId = SelectedClientId; // Store the currently selected client ID
                List<int> selectedBuildingIds = new();

                foreach (var buildingOption in BuildingOptions)
                {
                    if (buildingOption.IsSelected)
                    {
                        selectedBuildingIds.Add(buildingOption.Building.BuildingId);
                    }

                    buildingOption.PropertyChanged -= BuildingOptionPropertyChanged;
                }

                // Clear existing collections and add the retrieved data
                Clients.Clear();
                BuildingOptions.Clear();
                BuildingClasses.Clear();
                TechnicalIssues.Clear();

                // Rebuild the collections with the retrieved data
                foreach (var client in clients)
                {
                    Clients.Add(client);
                }

                foreach (var building in buildings)
                {
                    var buildingOption = new BuildingSelectionViewModel(building) // Create an option that holds this building and its checkbox state.
                    {
                        IsSelected = selectedBuildingIds.Contains(building.BuildingId) // Keep this building checked if it was selected before refreshing the list.
                    };

                    BuildingOptions.Add(buildingOption); // Add the completed option to the collection displayed by the form.
                    buildingOption.PropertyChanged += BuildingOptionPropertyChanged;
                }

                foreach (var buildingClass in buildingClasses)
                {
                    BuildingClasses.Add(buildingClass);
                }

                foreach (var technicalIssue in technicalIssues)
                {
                    TechnicalIssues.Add(technicalIssue);
                }

                // Restore the previously selected client ID if it still exists in the new list
                if (selectedClientId.HasValue && Clients
                    .Any(client => client.ClientId == selectedClientId.Value))
                {
                    SelectedClientId = selectedClientId;
                }
                else
                {
                    SelectedClientId = null; // Reset if the previously selected client no longer exists
                }

                SaveProjectCommand.RaiseCanExecuteChanged(); // Update the command's ability to execute based on the new data
                StatusMessage = ""; // Clear any previous status message
            }
            catch (DbException)
            {
                StatusMessage = $"Could not load the project choices. Please try again.";
            }
        }
    }
}