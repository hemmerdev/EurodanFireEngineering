using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Core.Services
{
    public class TechnicalIssueService
    {
        private readonly IRepository<TechnicalIssue> _technicalIssueRepository;

        public TechnicalIssueService(IRepository<TechnicalIssue> technicalIssueRepository)
        {
            _technicalIssueRepository = technicalIssueRepository;
        }

        public List<TechnicalIssue> GetAllTechnicalIssues()
        {
            return _technicalIssueRepository.GetAll().ToList();
        }

        public int CreateTechnicalIssue(TechnicalIssue technicalIssue)
        {
            ArgumentNullException.ThrowIfNull(technicalIssue);
            
            if (string.IsNullOrWhiteSpace(technicalIssue.Name))
            {
                throw new ArgumentException("Name is required.");
            }
            technicalIssue.Name = technicalIssue.Name.Trim();

            if (_technicalIssueRepository.GetAll().Any(issue => string.Equals(issue.Name, technicalIssue.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Name must be unique.");
            }

            if (technicalIssue.Name != null && technicalIssue.Name.Length > 150)
            {
                throw new ArgumentException("Name cannot exceed 150 characters.");
            }

            if (technicalIssue.Description != null && technicalIssue.Description.Length > 500)
            {
                throw new ArgumentException("Description cannot exceed 500 characters.");
            }

            _technicalIssueRepository.Add(technicalIssue);
            return technicalIssue.TechnicalIssueId;
        }

        public bool UpdateTechnicalIssue(TechnicalIssue technicalIssue)
        {
            ArgumentNullException.ThrowIfNull(technicalIssue);

            if (string.IsNullOrWhiteSpace(technicalIssue.Name))
            {
                throw new ArgumentException("Name is required.");
            }
            technicalIssue.Name = technicalIssue.Name.Trim();

            if (_technicalIssueRepository.GetAll().Any(issue => string.Equals(issue.Name, technicalIssue.Name, StringComparison.OrdinalIgnoreCase) && issue.TechnicalIssueId != technicalIssue.TechnicalIssueId))
            {
                throw new ArgumentException("Name must be unique.");
            }

            if (technicalIssue.Name != null && technicalIssue.Name.Length > 150)
            {
                throw new ArgumentException("Name cannot exceed 150 characters.");
            }

            if (technicalIssue.Description != null && technicalIssue.Description.Length > 500)
            {
                throw new ArgumentException("Description cannot exceed 500 characters.");
            }

            //TODO: Replace GetAll() with GetById() and make relevant changes to implement it properly. 
            if (_technicalIssueRepository.GetAll().Any(currentId => currentId.TechnicalIssueId == technicalIssue.TechnicalIssueId))
            {
                _technicalIssueRepository.Update(technicalIssue);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DeleteTechnicalIssue(int issueId)
        {
            if (_technicalIssueRepository.GetAll().Any(currentId => currentId.TechnicalIssueId == issueId))
            {
                _technicalIssueRepository.Delete(issueId);
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
