using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Tests.Fakes
{
    public class FakeBuildingRepository : IRepository<Building>
    {
        public List<Building> Buildings { get; } = new();
        public bool AddCalled { get; private set; }
        public bool DeleteCalled { get; private set; }

        public void Add(Building entity)
        {
            AddCalled = true;
            entity.BuildingId = 42;
            Buildings.Add(entity);
        }

        public void Delete(int id)
        {
            DeleteCalled = true;
        }

        public IEnumerable<Building> GetAll()
        {
            return Buildings;
        }

        public Building? GetById(int id)
        {
            foreach (Building building in Buildings)
            {
                if (building.BuildingId == id)
                {
                    return building;
                }
            }

            return null;
        }

        public void Update(Building entity)
        {
            throw new NotImplementedException();
        }
    }
}
