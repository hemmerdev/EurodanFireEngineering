using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace Eurodan.WPF.ViewModels
{
    public class ProjectOverviewViewModel : ViewModelBase
    {
        private readonly ProjectService _projectService;
        private string _statusMessage = string.Empty;

        public ObservableCollection<ProjectOverviewRowViewModel> Projects { get; } = new();

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

        public RelayCommand LoadProjectsCommand { get; }

        public ProjectOverviewViewModel(ProjectService projectService)
        {
            _projectService = projectService;

            LoadProjectsCommand = new RelayCommand(LoadProjects);
        }

        private void LoadProjects(object? parameter)
        {
            try
            {
                var projects = _projectService.ViewProjectOverview();

                if (projects.Count == 0)
                {
                    StatusMessage = "No projects found.";
                    Projects.Clear();
                    return;
                }

                var clients = _projectService.GetAllClients();
                var rows = new List<ProjectOverviewRowViewModel>();

                // Prepares the display rows
                foreach (var project in projects)
                {
                    Client? matchingClient =
                        clients.FirstOrDefault(client => client.ClientId == project.ClientId);

                    string clientCompanyName =
                        matchingClient?.CompanyName ?? "Client unavailable";

                    var row = new ProjectOverviewRowViewModel(project, clientCompanyName);
                    rows.Add(row);
                }

                Projects.Clear();
                // puts the completed rows into the observable collection for display in the UI
                foreach (var row in rows)
                {
                    Projects.Add(row);
                }

                StatusMessage = "Projects loaded successfully.";
            }
            catch (DbException)
            {
                StatusMessage = "Could not load projects. Please try again later.";
            }
        }
    }
}
