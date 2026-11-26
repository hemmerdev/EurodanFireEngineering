using Eurodan.Core.Enums;
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
        private string? _projectManager;
        private string? _buildingSurveyor;
        private string? _comments;
        private FeeProposalStatus _selectedFeeProposalStatus = FeeProposalStatus.Pending;
        private CompletionStatus? _selectedPbdbStatus;
        private CompletionStatus? _selectedFserStatus;
        private string? _feeProposalText = string.Empty;
        private string? _invoicedAmountText;
        private string? _outstandingAmountText;
        private DateTime? _selectedPaymentDueDate;
        private bool _isEditMode;
        private Project? _selectedProject;

        public ObservableCollection<Client> Clients { get; } = new();
        public ObservableCollection<BuildingSelectionViewModel> BuildingOptions { get; } = new();
        public ObservableCollection<BuildingClassSelectionViewModel> BuildingClassOptions { get; } = new();
        public ObservableCollection<TechnicalIssueSelectionViewModel> TechnicalIssueOptions { get; } = new();
        public List<FeeProposalStatus> FeeProposalStatuses { get; } = new()
        {
            FeeProposalStatus.Pending,
            FeeProposalStatus.Approved
        };
        public List<CompletionStatus> CompletionStatuses { get; } = new()
        {
            CompletionStatus.Completed,
            CompletionStatus.NotCompleted,
            CompletionStatus.NotApplicable
        };

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

        public string? ProjectManager
        {
            get => _projectManager;
            set
            {
                if (_projectManager != value)
                {
                    _projectManager = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? BuildingSurveyor
        {
            get => _buildingSurveyor;
            set
            {
                if (_buildingSurveyor != value)
                {
                    _buildingSurveyor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? FeeProposalText
        {
            get => _feeProposalText;
            set
            {
                if (_feeProposalText != value)
                {
                    _feeProposalText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? OutstandingAmountText
        {
            get => _outstandingAmountText;
            set
            {
                if (_outstandingAmountText != value)
                {
                    _outstandingAmountText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? InvoicedAmountText
        {
            get => _invoicedAmountText;
            set
            {
                if (_invoicedAmountText != value)
                {
                    _invoicedAmountText = value;
                    OnPropertyChanged();
                }
            }
        }



        public string? Comments
        {
            get => _comments;
            set
            {
                if (_comments != value)
                {
                    _comments = value;
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

        public FeeProposalStatus SelectedFeeProposalStatus
        {
            get => _selectedFeeProposalStatus;
            set
            {
                if (_selectedFeeProposalStatus != value)
                {
                    _selectedFeeProposalStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public CompletionStatus? SelectedPbdbStatus
        {
            get => _selectedPbdbStatus;
            set
            {
                if (_selectedPbdbStatus != value)
                {
                    _selectedPbdbStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public CompletionStatus? SelectedFserStatus
        {
            get => _selectedFserStatus;
            set
            {
                if (_selectedFserStatus != value)
                {
                    _selectedFserStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? SelectedPaymentDueDate
        {
            get => _selectedPaymentDueDate;
            set
            {
                if (_selectedPaymentDueDate != value)
                {
                    _selectedPaymentDueDate = value;
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


        // True when the form is editing an existing project.
        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (_isEditMode != value)
                {
                    _isEditMode = value;
                    OnPropertyChanged();
                    SaveProjectCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public RelayCommand SaveProjectCommand { get; }
        public RelayCommand ClearPbdbStatusCommand { get; }
        public RelayCommand ClearFserStatusCommand { get; }
        public RelayCommand ClearPaymentDueDateCommand { get; }
        public RelayCommand ClearProjectFormCommand { get; }
        
        // Raised after an edit is saved, carrying the ProjectId
        // MainViewModel owns navigation, so it listens and shows the updated details
        public event EventHandler<int>? ProjectUpdated;

        public ProjectViewModel(ProjectService projectService)
        {
            _projectService = projectService;

            SaveProjectCommand = new RelayCommand(SaveProject, CanSaveProject);
            ClearPbdbStatusCommand = new RelayCommand(ClearPbdbStatus);
            ClearFserStatusCommand = new RelayCommand(ClearFserStatus);
            ClearPaymentDueDateCommand = new RelayCommand(ClearPaymentDueDate);
            ClearProjectFormCommand = new RelayCommand(ClearProjectForm);
        }

        private void ClearProjectForm(object? parameter)
        {
            ResetProjectForm();
            StatusMessage = string.Empty; // Clear any status message when resetting the form
        }

        private void ClearPaymentDueDate(object? parameter)
        {
            SelectedPaymentDueDate = null;
        }

        private void ClearFserStatus(object? parameter)
        {
            SelectedFserStatus = null;
        }

        private void ClearPbdbStatus(object? parameter)
        {
            SelectedPbdbStatus = null;
        }

        // Event handler for when a BuildingOption's IsSelected property changes, to update the SaveProjectCommand's ability to execute.
        private void BuildingOptionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BuildingSelectionViewModel.IsSelected))
            {
                SaveProjectCommand.RaiseCanExecuteChanged();
            }
        }

        private bool TryParseOptionalAmount(string? text, string propertyName, out decimal? amount)
        {
            amount = null;
            if (string.IsNullOrWhiteSpace(text))
            {
                return true; // Null or empty is valid for optional amounts
            }

            if (decimal.TryParse(text, out decimal parsedAmount))
            {
                amount = parsedAmount;
                return true;
            }
            else
            {
                StatusMessage = $"Invalid {propertyName}. Please enter a valid decimal number.";
                return false;
            }
        }

        private void ResetProjectForm()
        {
            // Clear the form after successful creation
            ProjectNumber = string.Empty;
            ProjectName = string.Empty;
            ProjectManager = string.Empty;
            BuildingSurveyor = string.Empty;
            Comments = string.Empty;
            FeeProposalText = string.Empty;
            OutstandingAmountText = string.Empty;
            InvoicedAmountText = string.Empty;
            SelectedClientId = null;
            SelectedPbdbStatus = null;
            SelectedFserStatus = null;
            SelectedPaymentDueDate = null;
            SelectedFeeProposalStatus = FeeProposalStatus.Pending;
            _selectedProject = null;
            IsEditMode = false;

            foreach (var buildingOption in BuildingOptions)
            {
                buildingOption.IsSelected = false;
            }

            foreach (var buildingClassOption in BuildingClassOptions)
            {
                buildingClassOption.IsSelected = false;
            }

            foreach (var technicalIssueOption in TechnicalIssueOptions)
            {
                technicalIssueOption.IsSelected = false;
            }

            SaveProjectCommand.RaiseCanExecuteChanged();
        }

        private bool CanSaveProject(object? parameter)
        {
            // Same rule in both modes: the shared form needs a number, name, client and at least one building
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

            // Parse the optional amounts to a decimal, if provided
            if (!TryParseOptionalAmount(
                    FeeProposalText, "Fee proposal amount", out decimal? feeProposal))
            {
                return;
            }

            if (!TryParseOptionalAmount(
                    InvoicedAmountText, "Invoiced amount", out decimal? invoicedAmount))
            {
                return;
            }

            if (!TryParseOptionalAmount(
                    OutstandingAmountText, "Outstanding amount", out decimal? outstandingAmount))
            {
                return;
            }

            DateOnly? paymentDueDate = null;
            if (SelectedPaymentDueDate.HasValue)
            {
                paymentDueDate = DateOnly.FromDateTime(SelectedPaymentDueDate.Value);
            }

            Project project = new Project
            {
                // Edit mode keeps the original ProjectId so the service knows which project to update - 0 means new project
                ProjectId = _selectedProject?.ProjectId ?? 0,
                ProjectNumber = ProjectNumber,
                ProjectName = ProjectName,
                ClientId = SelectedClientId.Value,
                Buildings = BuildingOptions
                    .Where(b => b.IsSelected)
                    .Select(b => b.Building)
                    .ToList(),
                BuildingClasses = BuildingClassOptions
                    .Where(bc => bc.IsSelected)
                    .Select(bc => bc.BuildingClass)
                    .ToList(),
                TechnicalIssues = TechnicalIssueOptions
                    .Where(ti => ti.IsSelected)
                    .Select(ti => ti.TechnicalIssue)
                    .ToList(),
                ProjectManager = ProjectManager,
                BuildingSurveyor = BuildingSurveyor,
                Comments = Comments,
                FeeProposalStatus = SelectedFeeProposalStatus,
                PbdbStatus = SelectedPbdbStatus,
                FserStatus = SelectedFserStatus,
                FeeProposal = feeProposal,
                OutstandingAmount = outstandingAmount,
                InvoicedAmount = invoicedAmount,
                PaymentDueDate = paymentDueDate
            };

            // Remember edit mode now, because ResetProjectForm sets IsEditMode to false before we raise ProjectUpdated.
            bool isEditing = IsEditMode;

            // Only the save goes in this try, so a later failure is not shown as a save error
            try
            {
                // One shared form: isEditing (was IsEditMode) decides whether Save creates or updates
                if (isEditing)
                {
                    _projectService.UpdateProject(project);
                }
                else
                {
                    _projectService.CreateProject(project);
                }
            }
            catch (ArgumentException ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return;
            }
            catch (DbException)
            {
                StatusMessage = "Could not save the project. Please try again.";
                return;
            }
            catch (Exception)
            {
                StatusMessage = "An unexpected error occurred";
                return;
            }

            // Reached only when the save worked.
            // ProjectUpdated is raised outside the try, so errors in Main's handler are not shown as save errors.
            ResetProjectForm();

            if (isEditing)
            {
                StatusMessage = "Project updated.";
                ProjectUpdated?.Invoke(this, project.ProjectId);
            }
            else
            {
                StatusMessage = $"Project created successfully with ID: {project.ProjectId}";
            }
        }

        public bool LoadProjectForEditing(int projectId)
        {
            try
            {
                Project? loadedProject = _projectService.GetProjectDetails(projectId);

                if (loadedProject == null)
                {
                    StatusMessage = "Project could not be found.";
                    return false;
                }

                if (!LoadProjectLookups())
                {
                    return false; // If loading lookups failed, return false
                }

                ResetProjectForm();
                _selectedProject = loadedProject;
                IsEditMode = true;

                ProjectNumber = _selectedProject.ProjectNumber;
                ProjectName = _selectedProject.ProjectName;
                SelectedClientId = _selectedProject.ClientId;
                ProjectManager = _selectedProject.ProjectManager;
                BuildingSurveyor = _selectedProject.BuildingSurveyor;
                Comments = _selectedProject.Comments;

                SelectedFeeProposalStatus = _selectedProject.FeeProposalStatus;
                SelectedPbdbStatus = _selectedProject.PbdbStatus;
                SelectedFserStatus = _selectedProject.FserStatus;

                FeeProposalText = _selectedProject.FeeProposal?.ToString();
                InvoicedAmountText = _selectedProject.InvoicedAmount?.ToString();
                OutstandingAmountText = _selectedProject.OutstandingAmount?.ToString();

                SelectedPaymentDueDate =
                    _selectedProject.PaymentDueDate?.ToDateTime(TimeOnly.MinValue);

                // Restore the project's selected buildings, classes and technical issues! 

                foreach (var buildingOption in BuildingOptions)
                {
                    buildingOption.IsSelected = _selectedProject.Buildings
                        .Any(building => building.BuildingId == buildingOption.Building.BuildingId);
                }

                foreach (var buildingClassOption in BuildingClassOptions)
                {
                    buildingClassOption.IsSelected = _selectedProject.BuildingClasses
                        .Any(buildingClass =>
                            buildingClass.BuildingClassId == buildingClassOption.BuildingClass.BuildingClassId);
                }

                foreach (var technicalIssueOption in TechnicalIssueOptions)
                {
                    technicalIssueOption.IsSelected = _selectedProject.TechnicalIssues
                        .Any(technicalIssue =>
                            technicalIssue.TechnicalIssueId == technicalIssueOption.TechnicalIssue.TechnicalIssueId);
                }

                return true;
            }
            catch (DbException)
            {
                StatusMessage = "Could not load the project for editing.";
                return false;
            }
        }

        public bool LoadProjectLookups()
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
                List<int> selectedBuildingClassIds = new();
                List<int> selectedTechnicalIssueIds = new();

                // Store the currently selected building IDs before clearing the collection
                foreach (var buildingOption in BuildingOptions)
                {
                    if (buildingOption.IsSelected)
                    {
                        selectedBuildingIds.Add(buildingOption.Building.BuildingId);
                    }

                    buildingOption.PropertyChanged -= BuildingOptionPropertyChanged;
                }

                // Store the currently selected building class IDs before clearing the collection
                foreach (var buildingClassOption in BuildingClassOptions)
                {
                    if (buildingClassOption.IsSelected)
                    {
                        selectedBuildingClassIds.Add(
                            buildingClassOption.BuildingClass.BuildingClassId);
                    }
                }

                // Store the currently selected technical issue IDs before clearing the collection
                foreach (var technicalIssueOption in TechnicalIssueOptions)
                {
                    if (technicalIssueOption.IsSelected)
                    {
                        selectedTechnicalIssueIds.Add(
                            technicalIssueOption.TechnicalIssue.TechnicalIssueId);
                    }
                }

                // Clear existing collections and add the retrieved data
                Clients.Clear();
                BuildingOptions.Clear();
                BuildingClassOptions.Clear();
                TechnicalIssueOptions.Clear();

                // Rebuild the collections with the retrieved data
                foreach (var client in clients)
                {
                    Clients.Add(client);
                }

                foreach (var building in buildings)
                {
                    var buildingOption = new BuildingSelectionViewModel(building) // Create an option that holds this building and its checkbox state.
                    {
                        IsSelected = selectedBuildingIds.Contains(
                            building.BuildingId) // Keep this building checked if it was selected before refreshing the list.
                    };

                    BuildingOptions.Add(buildingOption); // Add the completed option to the collection displayed by the form.
                    buildingOption.PropertyChanged += BuildingOptionPropertyChanged;
                }

                foreach (var buildingClass in buildingClasses)
                {
                    var buildingClassOption = new BuildingClassSelectionViewModel(buildingClass)
                    {
                        IsSelected = selectedBuildingClassIds.Contains(
                            buildingClass.BuildingClassId)
                    };

                    BuildingClassOptions.Add(buildingClassOption);
                }

                foreach (var technicalIssue in technicalIssues)
                {
                    var technicalIssueOption = new TechnicalIssueSelectionViewModel(technicalIssue)
                    {
                        IsSelected = selectedTechnicalIssueIds.Contains(
                            technicalIssue.TechnicalIssueId)
                    };

                    TechnicalIssueOptions.Add(technicalIssueOption);
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
                return true;
            }
            catch (DbException)
            {
                StatusMessage = $"Could not load the project choices. Please try again.";
                return false;
            }
        }
    }
}