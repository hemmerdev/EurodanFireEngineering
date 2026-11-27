using Eurodan.Core.Models;

namespace Eurodan.Core.Interfaces
{
    public interface IProjectRepository : IReadOnlyRepository<Project>
    {
        void Add(Project project);
        void Update(Project project);
        void Archive(int projectId);
        void Restore(int projectId); // Opposite of Archive: sets IsArchived back to false
    }
}
