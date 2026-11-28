using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using Eurodan.WPF.ViewModels.enums;
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
        private bool _showArchivedProjects;
        private bool _showActiveProjects = true; // Default to showing active projects
        private List<ProjectOverviewRowViewModel> _allProjectRows = new();
        private ProjectOverviewRowViewModel? _selectedProjectRow;
        private string _technicalIssueSearchText = string.Empty;
        private string _buildingClassSearchText = string.Empty;
        private PaymentStatusFilter _paymentStatusFilter = PaymentStatusFilter.All;
        private DateTime? _paymentDueDateFrom = null;
        private DateTime? _paymentDueDateTo = null;

        public ObservableCollection<ProjectOverviewRowViewModel> Projects { get; } = new();
        public ObservableCollection<TechnicalIssueSelectionViewModel> TechnicalIssueOptions { get; } = new();
        public ObservableCollection<BuildingClassSelectionViewModel> BuildingClassOptions { get; } = new();


        // Holds the row selected in the Project Overview table.
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

        public string TechnicalIssueSearchText
        {
            get => _technicalIssueSearchText;
            set
            {
                if (_technicalIssueSearchText != value)
                {
                    _technicalIssueSearchText = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredTechnicalIssueOptions));
                }
            }
        }

        public string BuildingClassSearchText
        {
            get => _buildingClassSearchText;
            set
            {
                if (_buildingClassSearchText != value)
                {
                    _buildingClassSearchText = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredBuildingClassOptions));
                }
            }
        }

        public bool ShowArchivedProjects
        {
            get => _showArchivedProjects;
            set
            {
                if (_showArchivedProjects != value)
                {
                    _showArchivedProjects = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public bool ShowActiveProjects
        {
            get => _showActiveProjects;
            set
            {
                if (_showActiveProjects != value)
                {
                    _showActiveProjects = value;
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

        public List<TechnicalIssueSelectionViewModel> FilteredTechnicalIssueOptions
        {
            get
            {
                string searchText = TechnicalIssueSearchText?.Trim() ?? string.Empty;
                return searchText.Trim().Length == 0
                    ? TechnicalIssueOptions.ToList()
                    : TechnicalIssueOptions
                        .Where(option =>
                           option.TechnicalIssue.Name.Contains(
                               searchText, StringComparison.OrdinalIgnoreCase))
                        .ToList();
            }
        }

        public List<BuildingClassSelectionViewModel> FilteredBuildingClassOptions
        {
            get
            {
                string searchText = BuildingClassSearchText?.Trim() ?? string.Empty;
                return searchText.Trim().Length == 0
                    ? BuildingClassOptions.ToList()
                    : BuildingClassOptions
                        .Where(option =>
                           option.BuildingClass.ClassCode.Contains(
                               searchText, StringComparison.OrdinalIgnoreCase))
                        .ToList();
            }
        }

        public PaymentStatusFilter SelectedStatusFilter
        {
            get => _paymentStatusFilter;
            set
            {
                if (_paymentStatusFilter != value)
                {
                    _paymentStatusFilter = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public List<PaymentStatusFilter> PaymentStatusFilters { get; } = new()
        {
            PaymentStatusFilter.All,
            PaymentStatusFilter.Paid,
            PaymentStatusFilter.NotPaid,
            PaymentStatusFilter.NotSpecified
        };

        public DateTime? PaymentDueDateFrom
        {
            get => _paymentDueDateFrom;
            set
            {
                if (_paymentDueDateFrom != value)
                {
                    _paymentDueDateFrom = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }

        public DateTime? PaymentDueDateTo
        {
            get => _paymentDueDateTo;
            set
            {
                if (_paymentDueDateTo != value)
                {
                    _paymentDueDateTo = value;
                    OnPropertyChanged();
                    ApplyFilters();
                }
            }
        }


        public RelayCommand ResetFiltersCommand { get; }

        public ProjectOverviewViewModel(ProjectService projectService)
        {
            _projectService = projectService;

            ResetFiltersCommand = new RelayCommand(ResetFitlers);
        }

        private void ResetFitlers(object? parameter)
        {
            ProjectSearchText = "";
            ClientSearchText = "";
            TechnicalIssueSearchText = "";
            BuildingClassSearchText = "";
            PaymentDueDateFrom = null;
            PaymentDueDateTo = null;
            SelectedStatusFilter = PaymentStatusFilter.All;
            ShowActiveProjects = true;
            ShowArchivedProjects = false;

            foreach (var option in TechnicalIssueOptions)
            {
                option.IsSelected = false;
            }

            foreach (var option in BuildingClassOptions)
            {
                option.IsSelected = false;
            }

            ApplyFilters();
        }

        // Lets MainViewModel show a message on the overview, because StatusMessage has a private setter.
        public void ShowMessage(string message)
        {
            StatusMessage = message;
        }

        // Event handler for when a TechnicalIssueOption's IsSelected property changes
        private void TechnicalIssueOptionPropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TechnicalIssueSelectionViewModel.IsSelected))
            {
                ApplyFilters();
            }
        }

        private void BuildingClassOptionPropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BuildingClassSelectionViewModel.IsSelected))
            {
                ApplyFilters();
            }
        }

        public bool LoadProjects(object? parameter)
        {
            try
            {
                var projects = _projectService.ViewProjectOverview();
                var clients = _projectService.GetAllClients();
                var technicalIssues = _projectService.GetAllTechnicalIssues();
                var buildingClasses = _projectService.GetAllBuildingClasses();
                var rows = new List<ProjectOverviewRowViewModel>();

                var selectedTechnicalIssueIds = new List<int>();
                var selectedBuildingClassIds = new List<int>();
                foreach (var issue in TechnicalIssueOptions)
                {
                    if (issue.IsSelected)
                    {
                        selectedTechnicalIssueIds.Add(
                            issue.TechnicalIssue.TechnicalIssueId);
                    }

                    issue.PropertyChanged -= TechnicalIssueOptionPropertyChanged;
                }

                foreach (var buildingClass in BuildingClassOptions)
                {
                    if (buildingClass.IsSelected)
                    {
                        selectedBuildingClassIds.Add(
                            buildingClass.BuildingClass.BuildingClassId);
                    }

                    buildingClass.PropertyChanged -= BuildingClassOptionPropertyChanged;
                }

                BuildingClassOptions.Clear();
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

                foreach (var buildingClass in buildingClasses)
                {
                    var option = new BuildingClassSelectionViewModel(buildingClass)
                    {
                        IsSelected = selectedBuildingClassIds.Contains(
                            buildingClass.BuildingClassId)
                    };

                    BuildingClassOptions.Add(option);
                    option.PropertyChanged += BuildingClassOptionPropertyChanged;
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

                OnPropertyChanged(nameof(FilteredTechnicalIssueOptions));
                OnPropertyChanged(nameof(FilteredBuildingClassOptions));

                _allProjectRows = rows; // Store the complete list of rows for future filtering
                ApplyFilters();

                return true;
            }
            catch (DbException)
            {
                StatusMessage = "Could not load projects. Please try again later.";
                return false;
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
            List<int> selectedBuildingClassIds = BuildingClassOptions
                .Where(option => option.IsSelected)
                .Select(option => option.BuildingClass.BuildingClassId)
                .ToList();

            // Convert nullable DateTime to DateOnly for filtering
            DateOnly? paymentDueDateFrom = null;
            DateOnly? paymentDueDateTo = null;
            if (PaymentDueDateFrom.HasValue)
            {
                paymentDueDateFrom = DateOnly.FromDateTime(PaymentDueDateFrom.Value);
            }
            if (PaymentDueDateTo.HasValue)
            {
                paymentDueDateTo = DateOnly.FromDateTime(PaymentDueDateTo.Value);
            }

            // If both dates are provided, ensure that the From date is not after the To date
            if ((paymentDueDateFrom.HasValue && paymentDueDateTo.HasValue) &&
                paymentDueDateFrom > paymentDueDateTo)
            {
                StatusMessage = "The 'From' date must be on or before the 'To' date. Results have not been updated.";
                return;
            }

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

                // A row passes if user has active projects selected and the project is not archived,
                // or if the user has archived projects selected and the project is archived.
                bool matchesArchiveFilter = (ShowActiveProjects && !row.Project.IsArchived) ||
                                            (ShowArchivedProjects && row.Project.IsArchived);

                // Check if the row matches the selected payment status filter
                bool hasPaymentAmounts = row.Project.InvoicedAmount.HasValue &&
                                         row.Project.OutstandingAmount.HasValue;

                bool matchesPaymentStatusFilter = SelectedStatusFilter switch
                {
                    PaymentStatusFilter.All => true,                                            // All projects match the "All" filter
                    PaymentStatusFilter.Paid => row.Project.IsPaid,                             // Only projects that are paid match the "Paid" filter
                    PaymentStatusFilter.NotPaid => hasPaymentAmounts && !row.Project.IsPaid,    // Only projects that have invoiced and outstand amount and are not paid match the "NotPaid" filter
                    PaymentStatusFilter.NotSpecified => !hasPaymentAmounts,                     // Only projects that do not have invoiced or outstanding amounts match the "NotSpecified" filter
                    _ => false
                };

                // Check if the row matches the payment due date range filter                
                bool hasPaymentDueDateFilter =
                    paymentDueDateFrom.HasValue || paymentDueDateTo.HasValue;

                bool matchesPaymentDueDateFilter = true;
                if (hasPaymentDueDateFilter &&
                    !row.Project.PaymentDueDate.HasValue)
                {
                    matchesPaymentDueDateFilter = false; // Project has no payment due date, so it doesn't match the filter
                }

                if (row.Project.PaymentDueDate.HasValue)
                {
                    DateOnly dueDate = row.Project.PaymentDueDate.Value;

                    if (paymentDueDateFrom.HasValue &&
                        dueDate < paymentDueDateFrom.Value)
                    {
                        matchesPaymentDueDateFilter = false; // Project's due date is before the "From" date
                    }

                    if (paymentDueDateTo.HasValue &&
                        dueDate > paymentDueDateTo.Value)
                    {
                        matchesPaymentDueDateFilter = false; // Project's due date is after the "To" date
                    }
                }

                // Check if the row matches the selected building class filter.
                // This filter includes projects that have at least one of the selected building classes.
                bool matchesBuildingClasses = selectedBuildingClassIds.Count == 0; // If no building classes are selected, all projects match

                foreach (var id in selectedBuildingClassIds)
                {

                    if (row.Project.BuildingClasses
                        .Any(bc => bc.BuildingClassId == id))
                    {
                        matchesBuildingClasses = true; // Project has at least one of the selected building classes
                        break;
                    }
                }

                // Only add the row to the displayed list if it matches all filters
                if (matchesProjectSearch &&
                    matchesClientSearch &&
                    matchesTechnicalIssues &&
                    matchesArchiveFilter &&
                    matchesPaymentStatusFilter &&
                    matchesPaymentDueDateFilter &&
                    matchesBuildingClasses)
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

        public void Refresh()
        {
            SelectedProjectRow = null;
            StatusMessage = "";

            try
            {
                LoadProjects(null); // Load projects already catches DbException and sets StatusMessage, so we don't need to handle it here.
            }
            catch (Exception)
            {
                StatusMessage = "An unexpected error occurred while refreshing the project list.";
            }
        }
    }
}
