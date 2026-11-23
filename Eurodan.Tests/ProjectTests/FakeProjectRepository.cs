using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Tests.ProjectTests
{
    public class FakeProjectRepository : IProjectRepository
    {
        public List<Project> Projects { get; } = new();

        public IEnumerable<Project> GetAll()
        {
            return Projects;
        }

        public void Add(Project entity)
        {
            entity.ProjectId = 12;
            Projects.Add(entity);
        }

        public void Update(Project entity)
        {
            throw new NotImplementedException();
        }

        public void Archive(int projectId)
        {
            throw new NotImplementedException();
        }

        public Project? GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
