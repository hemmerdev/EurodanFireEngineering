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
                    ProjectOverview.LoadProjectsCommand.Execute(null); // Load projects before navigating to ProjectOverview
                    // TODO: Refresh logic goes here once implemented and if needed
                    CurrentViewModel = ProjectOverview;
                });

            ShowProjectCommand =
                new RelayCommand(_ =>
                {
                    ProjectManagement.LoadProjectLookups(); // Load lookups before navigating to ProjectManagement

                    // TODO: Refresh logic goes here once implemented and if needed
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
                    // TODO: Refresh logic goes here once implemented and if needed
                    CurrentViewModel = BuildingManagement;
                });

            ShowClientCommand = new RelayCommand(_ =>
            {
                // TODO: Refresh logic goes here once implemented and if needed
                CurrentViewModel = ClientManagement;
            });

            ShowTechnicalIssueCommand =
                new RelayCommand(_ =>
                {
                    // TODO: Refresh logic goes here once implemented and if needed
                    CurrentViewModel = TechnicalIssueManagement;
                });

            // Listen for project selection changes in the overview
            // After an edit is saved, go back to the overview and show the updated project
            ProjectOverview.PropertyChanged += ProjectOverviewPropertyChanged;
            ProjectManagement.ProjectUpdated += ProjectManagementProjectUpdated;

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
            // Reloads projects from the database so the table shows the saved values, then switches to the overview
            ShowProjectOverviewCommand.Execute(null);

            // Selecting the row reuses the existing path that opens the details panel
            ProjectOverview.SelectedProjectRow = ProjectOverview.Projects.FirstOrDefault(row => row.Project.ProjectId == projectId);
        }
    }
}