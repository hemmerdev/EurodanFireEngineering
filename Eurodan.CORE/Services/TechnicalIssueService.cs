using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Core.Services
{
    public class TechnicalIssueService
    {
        private readonly IRepository<TechnicalIssue> _technicalIssueRepository;
        private readonly IReadOnlyRepository<Project> _projectRepository;

        public TechnicalIssueService(IRepository<TechnicalIssue> technicalIssueRepository, IReadOnlyRepository<Project> projectRepository)
        {
            _technicalIssueRepository = technicalIssueRepository;
            _projectRepository = projectRepository;
        }

        public List<TechnicalIssue> GetAllTechnicalIssues()
        {
            return _technicalIssueRepository.GetAll().ToList();
        }

        public int CreateTechnicalIssue(TechnicalIssue technicalIssue)
        {
            ValidateTechnicalIssue(technicalIssue);

            if (_technicalIssueRepository.GetAll()
                .Any(issue => string.Equals(
                    issue.Name, technicalIssue.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(
                    "A technical issue with the same name already exists. Enter a different name.");
            }

            _technicalIssueRepository.Add(technicalIssue);
            return technicalIssue.TechnicalIssueId;
        }

        public bool UpdateTechnicalIssue(TechnicalIssue technicalIssue)
        {
            ValidateTechnicalIssue(technicalIssue);

            if (_technicalIssueRepository.GetAll()
                .Any(issue => string.Equals(
                    issue.Name, technicalIssue.Name, StringComparison.OrdinalIgnoreCase) &&
                    issue.TechnicalIssueId != technicalIssue.TechnicalIssueId))
            {
                throw new ArgumentException(
                    "A technical issue with the same name already exists. Enter a different name.");
            }

            if (_technicalIssueRepository.GetById(
                technicalIssue.TechnicalIssueId) == null)
            {
                return false;
            }

            _technicalIssueRepository.Update(technicalIssue);
            return true;
        }

        public bool DeleteTechnicalIssue(int issueId)
        {
            if (_technicalIssueRepository.GetById(issueId) == null)
            {
                return false;
            }

            if (_projectRepository.GetAll()
                .Any(project => project.TechnicalIssues
                .Any(issue => issue.TechnicalIssueId == issueId)))
            {
                throw new InvalidOperationException("The technical issue tag cannot be deleted, as it is assigned to a project.");
            }

            _technicalIssueRepository.Delete(issueId);
            return true;

        }

        private void ValidateTechnicalIssue(TechnicalIssue technicalIssue)
        {
            ArgumentNullException.ThrowIfNull(technicalIssue);
            if (string.IsNullOrWhiteSpace(technicalIssue.Name))
            {
                throw new ArgumentException("Enter a technical issue name.");
            }

            technicalIssue.Name = technicalIssue.Name.Trim();

            if (technicalIssue.Name.Length > 150)
            {
                throw new ArgumentException("Technical issue name cannot exceed 150 characters.");
            }

            if (technicalIssue.Description != null)
            {
                technicalIssue.Description = technicalIssue.Description.Trim();
                if (technicalIssue.Description.Length > 500)
                {
                    throw new ArgumentException("Description cannot exceed 500 characters.");
                }
            }
        }
    }
}
