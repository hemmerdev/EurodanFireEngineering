using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Core.Services
{
    public class BuildingService
    {
        private readonly IRepository<Building> _repository;

        public BuildingService(IRepository<Building> repository)
        {
            _repository = repository;
        }

        public int RegisterBuilding(Building building)
        {
            ArgumentNullException.ThrowIfNull(building);
            if (string.IsNullOrWhiteSpace(building.Name) && string.IsNullOrWhiteSpace(building.Address))
            {
                throw new ArgumentException("Enter a building name or address.");
            }

            if (building.Name != null && building.Name.Length > 150)
            {
                throw new ArgumentException("Building name cannot exceed 150 characters.");
            }

            if (building.Address != null && building.Address.Length > 250)
            {
                throw new ArgumentException("Address cannot exceed 250 characters.");
            }

            _repository.Add(building);
            return building.BuildingId;
        }
    }
}
