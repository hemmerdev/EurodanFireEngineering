using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Tests.Fakes
{
    public class FakeTechnicalIssueRepository : IRepository<TechnicalIssue>
    {
        public List<TechnicalIssue> Issues { get; } = new();

        public bool AddCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool DeleteCalled { get; private set; }

        public IEnumerable<TechnicalIssue> GetAll()
        {
            return Issues;
        }

        public TechnicalIssue? GetById(int id)
        {
            foreach (TechnicalIssue issue in Issues)
            {
                if (issue.TechnicalIssueId == id)
                {
                    return issue;
                }
            }

            return null;
        }

        public void Add(TechnicalIssue entity)
        {
            AddCalled = true;
            entity.TechnicalIssueId = 12;
            Issues.Add(entity);
        }

        public void Update(TechnicalIssue entity)
        {
            UpdateCalled = true;
        }

        public void Delete(int id)
        {
            DeleteCalled = true;
        }
    }
}