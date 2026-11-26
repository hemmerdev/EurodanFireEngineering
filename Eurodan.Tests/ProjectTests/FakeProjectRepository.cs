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
            // Same effect as SqlProjectRepository.Update (the stored project is replaced), but without a database
            for (var index = 0; index < Projects.Count; index++)
            {
                if (Projects[index].ProjectId == entity.ProjectId)
                {
                    Projects[index] = entity;
                }
            }
        }

        public void Archive(int projectId)
        {
            throw new NotImplementedException();
        }

        // Let the service check that a project exists without a database
        public Project? GetById(int id)
        {
            foreach (Project project in Projects)
            {
                if (project.ProjectId == id)
                {
                    return project;
                }
            }

            return null;
        }
    }
}
