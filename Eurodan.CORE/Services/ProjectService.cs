using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Core.Services
{
    public class ProjectService(
        IProjectRepository projectRepository,
        IReadOnlyRepository<Client> clientRepository,
        IReadOnlyRepository<Building> buildingRepository,
        IReadOnlyRepository<BuildingClass> buildingClassRepository,
        IReadOnlyRepository<TechnicalIssue> technicalIssueRepository)
    {
        private readonly IProjectRepository _projectRepository = projectRepository;
        private readonly IReadOnlyRepository<Client> _clientRepository = clientRepository;
        private readonly IReadOnlyRepository<Building> _buildingRepository = buildingRepository;
        private readonly IReadOnlyRepository<BuildingClass> _buildingClassRepository = buildingClassRepository;
        private readonly IReadOnlyRepository<TechnicalIssue> _technicalIssueRepository = technicalIssueRepository;


        private static void ValidateDecimalAmount(decimal? amount, string propertyName)
        {
            if (!amount.HasValue)
            {
                return;
            }

            decimal value = amount.Value;
            if (value < -99_999_999.99m || value > 99_999_999.99m)
            {
                throw new ArgumentException(
                    $"{propertyName} must be between -99,999,999.99 and 99,999,999.99.");
            }
            if (value != decimal.Round(value, 2))
            {
                throw new ArgumentException(
                    $"{propertyName} cannot have more than two decimal places.");
            }
        }

        public int CreateProject(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project), "Project could not be created.");
            }

            if (string.IsNullOrWhiteSpace(project.ProjectNumber) ||
                string.IsNullOrWhiteSpace(project.ProjectName))
            {
                throw new ArgumentException("Project number and name are required.");
            }

            project.ProjectNumber = project.ProjectNumber.Trim();
            project.ProjectName = project.ProjectName.Trim();

            if (project.ProjectNumber.Length > 30)
            {
                throw new ArgumentException("Project number cannot exceed 30 characters.");
            }

            if (project.ProjectName.Length > 200)
            {
                throw new ArgumentException("Project name cannot exceed 200 characters.");
            }

            if (project.ProjectManager is not null &&
                project.ProjectManager.Length > 100)
            {
                throw new ArgumentException("Project manager name cannot exceed 100 characters.");
            }

            if (project.BuildingSurveyor is not null &&
                project.BuildingSurveyor.Length > 100)
            {
                throw new ArgumentException("Building surveyor name cannot exceed 100 characters.");
            }

            ValidateDecimalAmount(project.FeeProposal, "Fee proposal amount");
            ValidateDecimalAmount(project.OutstandingAmount, "Outstanding amount");
            ValidateDecimalAmount(project.InvoicedAmount, "Invoiced amount");

            // TODO: Confirm the rules for past payment due dates,
            // including historical projects, before adding date restrictions.

            // Check if the project number is unique
            if (_projectRepository.GetAll()
                .Any(existingProject => string.Equals(
                    existingProject.ProjectNumber, project.ProjectNumber,
                    StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(
                    $"A project with this number ({project.ProjectNumber}) already exists.");
            }

            // Validate ClientId
            if (_clientRepository.GetById(project.ClientId) is null)
            {
                throw new ArgumentException(
                    "Select an existing client before creating the project.");
            }

            // Validate buildings
            if (project.Buildings is null || project.Buildings.Count == 0)
            {
                throw new ArgumentException(
                    "Select at least one building before creating the project.");
            }

            List<int> validatedBuildingIds = new List<int>();
            foreach (var building in project.Buildings)
            {
                if (validatedBuildingIds.Contains(building.BuildingId))
                {
                    throw new ArgumentException(
                        "The same building cannot be selected more than once.");
                }

                if (_buildingRepository.GetById(building.BuildingId) == null)
                {
                    throw new ArgumentException(
                        "One of the selected buildings could not be found. " +
                        "Select an existing building.");
                }

                validatedBuildingIds.Add(building.BuildingId);
            }

            // Validate building classes
            if (project.BuildingClasses is null)
            {
                project.BuildingClasses = new List<BuildingClass>();
            }

            List<int> validatedBuildingClassIds = new List<int>();
            foreach (var buildingClass in project.BuildingClasses)
            {
                if (validatedBuildingClassIds.Contains(
                    buildingClass.BuildingClassId))
                {
                    throw new ArgumentException(
                        "The same building class cannot be selected more than once.");
                }

                if (_buildingClassRepository.GetById(
                    buildingClass.BuildingClassId) == null)
                {
                    throw new ArgumentException(
                        "One of the selected building classes could not be found. " +
                        "Select an existing building class.");
                }

                validatedBuildingClassIds.Add(buildingClass.BuildingClassId);
            }

            // Validate technical issues
            if (project.TechnicalIssues is null)
            {
                project.TechnicalIssues = new List<TechnicalIssue>();
            }

            List<int> validatedTechnicalIssueIds = new List<int>();
            foreach (var technicalIssue in project.TechnicalIssues)
            {
                if (validatedTechnicalIssueIds.Contains(
                    technicalIssue.TechnicalIssueId))
                {
                    throw new ArgumentException(
                        "The same technical issue cannot be selected more than once.");
                }
                if (_technicalIssueRepository.GetById(
                    technicalIssue.TechnicalIssueId) == null)
                {
                    throw new ArgumentException(
                        "One of the selected technical issues could not be found. " +
                        "Select an existing technical issue.");
                }

                validatedTechnicalIssueIds.Add(technicalIssue.TechnicalIssueId);
            }

            // All validations passed, add the project to the repository
            _projectRepository.Add(project);
            return project.ProjectId;

        }

        public List<Project> ViewProjectOverview()
        {
            return _projectRepository.GetAll().ToList();
        }

        public List<Client> GetAllClients()
        {
            return _clientRepository.GetAll().ToList();
        }

        public List<Building> GetAllBuildings()
        {
            return _buildingRepository.GetAll().ToList();
        }

        public List<BuildingClass> GetAllBuildingClasses()
        {
            return _buildingClassRepository.GetAll().ToList();
        }

        public List<TechnicalIssue> GetAllTechnicalIssues()
        {
            return _technicalIssueRepository.GetAll().ToList();
        }
    }
}
