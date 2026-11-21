using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Tests.ProjectTests
{
    public class FakeProjectRepository : IRepository<Project>
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

        void IRepository<Project>.Update(Project entity)
        {
            throw new NotImplementedException();
        }

        void IRepository<Project>.Delete(int id)
        {
            throw new NotImplementedException();
        }

        Project? IReadOnlyRepository<Project>.GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
