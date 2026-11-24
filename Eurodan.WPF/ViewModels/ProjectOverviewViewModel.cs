using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;

namespace Eurodan.WPF.ViewModels
{
    public class ProjectOverviewViewModel : ViewModelBase
    {
        private readonly ProjectService _projectService;
        private string _statusMessage = string.Empty;
        private string _projectSearchText = string.Empty;
        private string _clientSearchText = string.Empty;
        private List<ProjectOverviewRowViewModel> _allProjectRows = new();

        public ObservableCollection<ProjectOverviewRowViewModel> Projects { get; } = new();
        public ObservableCollection<TechnicalIssueSelectionViewModel> TechnicalIssueOptions { get; } = new();

        private ProjectOverviewRowViewModel? _selectedProjectRow;

        public ProjectOverviewRowViewModel? SelectedProjectRow
        {
            get => _selectedProjectRow;
            set
            {
                if (_selectedProjectRow != value)
                {
                    _selectedProjectRow = value;
                    OnPropertyChanged();
                }
            }
        }


        public string ProjectSearchText
        {
            get => _projectSearchText;
            set
            {
                if (_projectSearchText != value)
                {
                    _projectSearchText = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }
        public string ClientSearchText
        {
            get => _clientSearchText;
            set
            {
                if (_clientSearchText != value)
                {
                    _clientSearchText = value;
                    OnPropertyChanged();
                    ApplyFilters();
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

    


        public RelayCommand LoadProjectsCommand { get; }

        public ProjectOverviewViewModel(ProjectService projectService)
        {
            _projectService = projectService;

            LoadProjectsCommand = new RelayCommand(LoadProjects);
        }

        // Event handler for when a TechnicalIssueOption's IsSelected property changes
        private void TechnicalIssueOptionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TechnicalIssueSelectionViewModel.IsSelected))
            {
                ApplyFilters();
            }
        }

        private void LoadProjects(object? parameter)
        {
            try
            {
                var projects = _projectService.ViewProjectOverview();
                var clients = _projectService.GetAllClients();
                var technicalIssues = _projectService.GetAllTechnicalIssues();
                var rows = new List<ProjectOverviewRowViewModel>();
                var selectedTechnicalIssueIds = new List<int>();

                foreach (var issue in TechnicalIssueOptions)
                {
                    if (issue.IsSelected)
                    {
                        selectedTechnicalIssueIds.Add(
                            issue.TechnicalIssue.TechnicalIssueId);
                    }

                    issue.PropertyChanged -= TechnicalIssueOptionPropertyChanged;
                }

                TechnicalIssueOptions.Clear();

                foreach (var issue in technicalIssues)
                {
                    var option = new TechnicalIssueSelectionViewModel(issue)
                    {
                        IsSelected = selectedTechnicalIssueIds.Contains(
                            issue.TechnicalIssueId)
                    };

                    TechnicalIssueOptions.Add(option);
                    option.PropertyChanged += TechnicalIssueOptionPropertyChanged;
                }

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

                _allProjectRows = rows; // Store the complete list of rows for future filtering
                ApplyFilters();
            }
            catch (DbException)
            {
                StatusMessage = "Could not load projects. Please try again later.";
            }
        }

        private void ApplyFilters()
        {
            string projectSearchText = ProjectSearchText?.Trim() ?? string.Empty;
            string clientSearchText = ClientSearchText?.Trim() ?? string.Empty;
            List<int> selectedTechnicalIssueIds = TechnicalIssueOptions
                .Where(option => option.IsSelected)
                .Select(option => option.TechnicalIssue.TechnicalIssueId)
                .ToList();

            Projects.Clear();

            foreach (var row in _allProjectRows)
            {
                // Check if the row matches the project search text
                bool matchesProjectSearch =
                    string.IsNullOrWhiteSpace(projectSearchText) ||
                    row.Project.ProjectName.Contains(projectSearchText, StringComparison.OrdinalIgnoreCase) ||
                    row.Project.ProjectNumber.Contains(projectSearchText, StringComparison.OrdinalIgnoreCase);

                // Check if the row matches the client search text
                bool matchesClientSearch =
                    string.IsNullOrWhiteSpace(clientSearchText) ||
                    row.ClientCompanyName.Contains(clientSearchText, StringComparison.OrdinalIgnoreCase);

                // Check if the row matches the selected technical issues
                bool matchesTechnicalIssues = true;
                foreach (var issueId in selectedTechnicalIssueIds)
                {
                    bool projectHasIssue =
                        row.Project.TechnicalIssues.Any(ti => ti.TechnicalIssueId == issueId);
                    if (!projectHasIssue)
                    {
                        matchesTechnicalIssues = false;
                        break;
                    }
                }

                // Only add the row to the displayed list if it matches all filters
                if (matchesProjectSearch &&
                    matchesClientSearch &&
                    matchesTechnicalIssues)
                {
                    Projects.Add(row);
                }
            }

            if (_allProjectRows.Count == 0)
            {
                StatusMessage = "No projects found.";
            }
            else if (Projects.Count == 0)
            {
                StatusMessage = "No projects match your search";
            }
            else
            {
                StatusMessage = $"{Projects.Count} project(s) displayed.";
            }
        }
    }
}
