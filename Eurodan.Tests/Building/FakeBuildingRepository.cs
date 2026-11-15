using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Tests.Fakes
{
    public class FakeBuildingRepository : IRepository<Building>
    {
        public bool AddCalled { get; private set; }

        public void Add(Building entity)
        {
            AddCalled = true;
            entity.BuildingId = 42;
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Building> GetAll()
        {
            throw new NotImplementedException();
        }

        public Building? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(Building entity)
        {
            throw new NotImplementedException();
        }
    }
}
