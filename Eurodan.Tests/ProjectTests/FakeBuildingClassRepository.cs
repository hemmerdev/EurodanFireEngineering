using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Tests.ProjectTests
{
    public class FakeBuildingClassRepository : IReadOnlyRepository<BuildingClass>
    {
        public List<BuildingClass> BuildingClasses { get; } = new();

        public IEnumerable<BuildingClass> GetAll()
        {
            return BuildingClasses;
        }
        public BuildingClass? GetById(int id)
        {
            foreach (BuildingClass buildingClass in BuildingClasses)
            {
                if (buildingClass.BuildingClassId == id)
                {
                    return buildingClass;
                }
            }
            return null;
        }
    }
}
