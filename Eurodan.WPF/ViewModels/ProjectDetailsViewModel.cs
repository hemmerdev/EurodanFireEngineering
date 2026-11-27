using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using System.Data.Common;
using System.Windows;

namespace Eurodan.WPF.ViewModels
{
    public class ProjectDetailsViewModel : ViewModelBase
    {
        private readonly ProjectService _projectService;
        private Project? _selectedProject;
        private Client? _selectedClient;
        private string _statusMessage = string.Empty;

        public RelayCommand LoadProjectDetailsCommand { get; }
        public RelayCommand ArchiveProjectCommand { get; }
        public RelayCommand RestoreProjectCommand { get; }

        // Raised after a successful archive or restore, carrying the projectId
        // MainViewModel owns the overview, so it reloads the list and shows the result message
        public event EventHandler<int>? ProjectArchiveStateChanged;

        public Project? SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (_selectedProject != value)
                {
                    _selectedProject = value;
                    OnPropertyChanged();

                    // Calculated from SelectedProject, so announce it here
                    OnPropertyChanged(nameof(IsSelectedProjectArchived));
                    OnPropertyChanged(nameof(IsSelectedProjectActive));
                }
            }
        }

        // Yes/no flags for the Archive/Restore buttons - both are false when no project is loaded,
        // so an empty panel shows neither button.
        public bool IsSelectedProjectArchived =>
            SelectedProject != null && SelectedProject.IsArchived;
        public bool IsSelectedProjectActive =>
            SelectedProject != null && !SelectedProject.IsArchived;

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
            ArchiveProjectCommand = new RelayCommand(ArchiveProject);
            RestoreProjectCommand = new RelayCommand(RestoreProject);
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

        // UC05: Asks for confirmation first (No is the default, so a stray Enter does nothing).
        // Only the service call is inside the try, and the event is raised after it, so a failed
        // list refresh is never reported as a failed archive.
        private void ArchiveProject(object? parameter)
        {
            if (SelectedProject == null)
            {
                StatusMessage = "Select a project to archive.";
                return;
            }

            int projectId = SelectedProject.ProjectId;
            string projectName = SelectedProject.ProjectName;

            MessageBoxResult answer = MessageBox.Show(
                $"Archive project {projectName}?",
                "Archive project",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No
            );

            if (answer != MessageBoxResult.Yes)
            {
                StatusMessage = "Archiving cancelled.";
                return;
            }

            try
            {
                _projectService.ArchiveProject(projectId);
            }
            catch (ArgumentException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return;
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return;
            }
            catch (DbException)
            {
                StatusMessage = "Could not archive the project. Please try again.";
                return;
            }
            catch (Exception)
            {
                StatusMessage = "An unexpected error occurred.";
                return;
            }

            ProjectArchiveStateChanged?.Invoke(this, projectId);
        }

        // UC05: No confirmation here. Restoring is harmless and can be undone by archiving again.
        private void RestoreProject(object? parameter)
        {
            if (SelectedProject == null)
            {
                StatusMessage = "Select a project to restore.";
                return;
            }

            int projectId = SelectedProject.ProjectId;

            try
            {
                _projectService.RestoreProject(projectId);
            }
            catch (ArgumentException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return;
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return;
            }
            catch (DbException)
            {
                StatusMessage = "Could not restore the project. Please try again.";
                return;
            }
            catch (Exception)
            {
                StatusMessage = "An unexpected error occurred.";
                return;
            }

            ProjectArchiveStateChanged?.Invoke(this, projectId);
        }
    }
}