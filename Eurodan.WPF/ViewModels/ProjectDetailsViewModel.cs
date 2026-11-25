using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using System.Data.Common;

namespace Eurodan.WPF.ViewModels
{
    public class ProjectDetailsViewModel : ViewModelBase
    {
        private readonly ProjectService _projectService;
        private Project? _selectedProject;
        private Client? _selectedClient;
        private string _statusMessage = string.Empty;

        public RelayCommand LoadProjectDetailsCommand { get; }

        public Project? SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (_selectedProject != value)
                {
                    _selectedProject = value;
                    OnPropertyChanged();
                }
            }
        }

        public Client? SelectedClient
        {

            get => _selectedClient;
            set
            {
                if (_selectedClient != value)
                {

                    _selectedClient = value;
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

        public ProjectDetailsViewModel(ProjectService projectService)
        {
            _projectService = projectService;

            LoadProjectDetailsCommand = new RelayCommand(LoadProjectDetails);
        }

        public void ShowError(string message)
        {
            StatusMessage = message;
        }

        private void LoadProjectDetails(object? parameter)
        {
            // Clear the previous details before loading the next selected project.
            SelectedProject = null;
            SelectedClient = null;
            StatusMessage = string.Empty;

            if (parameter is not int projectId || projectId <= 0)
            {
                StatusMessage = "Select a project to view its details.";
                return;
            }

            try
            {
                SelectedProject = _projectService.GetProjectDetails(projectId);

                if (SelectedProject == null)
                {
                    StatusMessage = "Project could not be found.";
                    return;
                }

                // Use the project's ClientId to load the client contact information!
                SelectedClient = _projectService.GetClientById(SelectedProject.ClientId);
            }
            catch (DbException)
            {
                StatusMessage = "Could not load project details.";
            }
        }
    }
}