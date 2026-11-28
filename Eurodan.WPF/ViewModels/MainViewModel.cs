using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using Eurodan.WPF.ViewModels.enums;
using System.ComponentModel;

namespace Eurodan.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase? _currentViewModel;

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ActivePage)); // Notify that ActivePage has changed
                }
            }
        }

        // Properties for ViewModel instances
        public ProjectOverviewViewModel ProjectOverview { get; }
        public ProjectViewModel ProjectManagement { get; }
        public ProjectDetailsViewModel ProjectDetails { get; }
        public BuildingViewModel BuildingManagement { get; }
        public ClientViewModel ClientManagement { get; }
        public TechnicalIssueViewModel TechnicalIssueManagement { get; }

        // Property used for highlighting the selected navigation button in the UI
        public NavigationPage? ActivePage => CurrentViewModel switch
        {
            ProjectOverviewViewModel => NavigationPage.ProjectOverview,
            ProjectViewModel => NavigationPage.ProjectManagement,
            ProjectDetailsViewModel => NavigationPage.ProjectDetails,
            BuildingViewModel => NavigationPage.BuildingManagement,
            ClientViewModel => NavigationPage.ClientManagement,
            TechnicalIssueViewModel => NavigationPage.TechnicalIssueManagement,
            _ => null
        };

        // Navigation commands 
        public RelayCommand ShowProjectOverviewCommand { get; }
        public RelayCommand ShowProjectCommand { get; }
        // Shows the selected project's details in the bottom panel of Project Overview.
        public RelayCommand ShowProjectDetailsCommand { get; }
        public RelayCommand CloseProjectDetailsCommand { get; }
        public RelayCommand OpenProjectFormForEditingCommand { get; }
        public RelayCommand ShowBuildingCommand { get; }
        public RelayCommand ShowClientCommand { get; }
        public RelayCommand ShowTechnicalIssueCommand { get; }

        public MainViewModel(
            ClientService clientService,
            BuildingService buildingService,
            TechnicalIssueService technicalIssueService,
            ProjectService projectService)
        {
            ProjectOverview = new ProjectOverviewViewModel(projectService);
            ProjectManagement = new ProjectViewModel(projectService);
            ProjectDetails = new ProjectDetailsViewModel(projectService);
            BuildingManagement = new BuildingViewModel(buildingService);
            ClientManagement = new ClientViewModel(clientService);
            TechnicalIssueManagement = new TechnicalIssueViewModel(technicalIssueService);

            ShowProjectOverviewCommand =
                new RelayCommand(_ =>
                {
                    ProjectOverview.Refresh();
                    CurrentViewModel = ProjectOverview;
                });

            ShowProjectCommand =
                new RelayCommand(_ =>
                {
                    ProjectManagement.Refresh();
                    CurrentViewModel = ProjectManagement;
                });

            ShowProjectDetailsCommand =
                new RelayCommand(parameter =>
                {
                    ProjectDetails.LoadProjectDetailsCommand.Execute(parameter);
                    CurrentViewModel = ProjectOverview;
                });

            CloseProjectDetailsCommand =
                new RelayCommand(_ =>
                {
                    // Clear selection to hide the panel and allow the same project to reopen!
                    ProjectOverview.SelectedProjectRow = null;
                });

            OpenProjectFormForEditingCommand = new RelayCommand(
                parameter =>                                            // Execute: Open the selected project for editing
                {
                    if (parameter is int projectId && projectId > 0)
                    {
                        OpenProjectFormForEditing(projectId);
                    }
                },
                parameter => parameter is int projectId && projectId > 0); // CanExecute: Only allow if parameter is a valid projectId

            ShowBuildingCommand =
                new RelayCommand(_ =>
                {
                    BuildingManagement.Refresh();
                    CurrentViewModel = BuildingManagement;
                });

            ShowClientCommand = new RelayCommand(_ =>
            {
                ClientManagement.Refresh();
                CurrentViewModel = ClientManagement;
            });

            ShowTechnicalIssueCommand =
                new RelayCommand(_ =>
                {
                    TechnicalIssueManagement.Refresh();
                    CurrentViewModel = TechnicalIssueManagement;
                });

            // Listen for project selection changes in the overview
            // After an edit is saved, go back to the overview and show the updated project
            ProjectOverview.PropertyChanged += ProjectOverviewPropertyChanged;
            ProjectManagement.ProjectUpdated += ProjectManagementProjectUpdated;

            // The event gives (sender, projectId); the lambda passes on the projectId plus the strings to display in the status message.
            ProjectDetails.ProjectArchived += (sender, projectId) =>
                RefreshOverviewAfterArchiveChanged(projectId, "Project archived");
            ProjectDetails.ProjectUnarchived += (sender, projectId) =>
                RefreshOverviewAfterArchiveChanged(projectId, "Project unarchived");

            ShowProjectOverviewCommand.Execute(null); // Set the initial view to ProjectOverview
        }

        public void OpenProjectFormForEditing(int projectId)
        {
            if (!ProjectManagement.LoadProjectForEditing(projectId))
            {
                ProjectDetails.ShowError(ProjectManagement.StatusMessage);
                return; // If loading the project for editing failed, do not switch views.
            }
            CurrentViewModel = ProjectManagement;
        }

        // Event handler for when ProjectOverview's SelectedProjectRow property changes!!
        private void ProjectOverviewPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProjectOverviewViewModel.SelectedProjectRow))
            {
                var selectedRow = ProjectOverview.SelectedProjectRow;

                if (selectedRow != null)
                {
                    ShowProjectDetailsCommand.Execute(
                        selectedRow.Project.ProjectId);
                }
            }
        }

        private void ProjectManagementProjectUpdated(object? sender, int projectId)
        {
            try
            {
                bool loadedSuccessfully = ProjectOverview.LoadProjects(null); // Refresh the overview to reflect the updated project

                CurrentViewModel = ProjectOverview;

                if (!loadedSuccessfully) // If the refresh fails, clear the selection and show a message
                {
                    ProjectOverview.SelectedProjectRow = null; // Clear selection to avoid showing the previous project details
                    ProjectOverview.ShowMessage("The project was updated, but the overview could not be refreshed.");
                    return; // Prevent further processing since the refresh failed
                }

                // Search the filtered list. The result is null if the current search or filters hide the updated project.
                ProjectOverviewRowViewModel? updatedRow =
                    ProjectOverview.Projects.FirstOrDefault(row => row.Project.ProjectId == projectId);

                // Keep the user's filters, but tell them why the updated project is not shown.
                if (updatedRow == null)
                {
                    // Clear selection so panel doesn't show the previously open project pre-edit
                    ProjectOverview.SelectedProjectRow = null;
                    ProjectOverview.ShowMessage(
                        "The project was updated, but it is hidden by the current search or filters.");
                }
                else
                {
                    ProjectOverview.SelectedProjectRow = updatedRow;
                }
            }
            catch (Exception)
            {
                CurrentViewModel = ProjectOverview; // Go back to the overview even if the refresh fails
                // The save already worked. If the refresh fails, say that, instead of reporting a save error.
                ProjectOverview.ShowMessage("The project was updated, but the overview could not be refreshed.");
            }
        }



        // Shared by archive and unarchive: both need the same reload/reselect as UC04's ProjectManagementProjectUpdated.
        // The archive or unarchive already succeeded before this runs, so every message starts with what was done,
        // and a failed reload is reported as a failed refresh, never as a failed archive.
        private void RefreshOverviewAfterArchiveChanged(int projectId, string doneMessage)
        {
            try
            {
                bool loadedSuccessfully = ProjectOverview.LoadProjects(null); // Refresh the overview to reflect the updated project

                CurrentViewModel = ProjectOverview;

                if (!loadedSuccessfully) // If the refresh fails, clear the selection and show a message
                {
                    ProjectOverview.SelectedProjectRow = null; // Clear selection to avoid showing the previous project details
                    ProjectOverview.ShowMessage($"{doneMessage}, but the overview could not be refreshed.");
                    return; // Prevent further processing since the refresh failed
                }

                // Search the filtered list. The result is null if the current search or filters hide the updated project.
                ProjectOverviewRowViewModel? updatedRow =
                    ProjectOverview.Projects.FirstOrDefault(row => row.Project.ProjectId == projectId);

                // Keep the user's filters, but tell them why the updated project is not shown.
                if (updatedRow == null)
                {
                    // Clear selection so panel doesn't show the previously open project pre-edit
                    ProjectOverview.SelectedProjectRow = null;
                    ProjectOverview.ShowMessage($"{doneMessage}, but it is hidden by the current search or filters.");
                }
                else
                {
                    ProjectOverview.SelectedProjectRow = updatedRow;
                    ProjectOverview.ShowMessage($"{doneMessage}.");
                }
            }
            catch (Exception)
            {
                CurrentViewModel = ProjectOverview; // Go back to the overview even if the refresh fails
                // The save already worked. If the refresh fails, say that, instead of reporting a save error.
                ProjectOverview.ShowMessage($"{doneMessage}, but the overview could not be refreshed.");
            }
        }
    }
}