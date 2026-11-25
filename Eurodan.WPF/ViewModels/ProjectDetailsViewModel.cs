using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using System.Data.Common;

namespace Eurodan.WPF.ViewModels
{
    public class ProjectDetailsViewModel : ViewModelBase
    {
        private readonly ProjectService _projectService;
        private readonly Action<int> _editProject;
        private Project? _selectedProject;
        private Client? _selectedClient;
        private string _statusMessage = string.Empty;

        public RelayCommand LoadProjectCommand { get; }
        public RelayCommand EditProjectCommand { get; }


        public Project? SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (_selectedProject != value)
                {
                    _selectedProject = value;
                    OnPropertyChanged();

                    EditProjectCommand.RaiseCanExecuteChanged();
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

       
        public ProjectDetailsViewModel( ProjectService projectService, Action<int> editProject)
        {
            _projectService = projectService;

            // Main supplies the method that opens the project for editing.
            _editProject = editProject;

            LoadProjectCommand = new RelayCommand(LoadProject);
            EditProjectCommand = new RelayCommand(EditProject, CanEditProject);
        }


        private bool CanEditProject(object? parameter)
        {
            return SelectedProject != null;
        }

        private void EditProject(object? parameter)
        {
            if (SelectedProject == null)
            {
                return;
            }

            // Ask Main to open this project in Manage Projects.
            _editProject(SelectedProject.ProjectId);
        }


        private void LoadProject(object? parameter)
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