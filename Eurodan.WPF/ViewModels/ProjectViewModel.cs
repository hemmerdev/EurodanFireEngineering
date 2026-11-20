using Eurodan.Core.Models;
using Eurodan.Core.Services;
using System.Collections.ObjectModel;
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
        public ObservableCollection<Building> Buildings { get; } = new();
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
                }
            }
        }

        public ProjectViewModel(ProjectService projectService)
        {
            _projectService = projectService;
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

                var selectedClientId = SelectedClientId; // Store the currently selected client ID

                // Clear existing collections and add the retrieved data
                Clients.Clear();
                Buildings.Clear();
                BuildingClasses.Clear();
                TechnicalIssues.Clear();

                // Rebuild the collections with the retrieved data
                foreach (var client in clients)
                {
                    Clients.Add(client);
                }

                foreach (var building in buildings)
                {
                    Buildings.Add(building);
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

                StatusMessage = ""; // Clear any previous status message
            }
            catch (DbException)
            {
                StatusMessage = $"Could not load the project choices. Please try again.";
            }
        }
    }
}