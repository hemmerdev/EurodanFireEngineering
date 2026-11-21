using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Core.Services
{
    public class BuildingService
    {
        private readonly IRepository<Building> _buildingRepository;

        public BuildingService(IRepository<Building> repository)
        {
            _buildingRepository = repository;
        }

        public List<Building> GetAllBuildings()
        {
            return _buildingRepository.GetAll().ToList();
        }

        public int RegisterBuilding(Building building)
        {
            ValidateBuilding(building);
            _buildingRepository.Add(building);
            return building.BuildingId;
        }

        public bool UpdateBuilding(Building building)
        {
            ValidateBuilding(building);
            Building? existingBuilding = _buildingRepository.GetById(building.BuildingId);
            if (existingBuilding == null)
            {
                return false;
            }

            _buildingRepository.Update(building);

            return true;
        }

        private void ValidateBuilding(Building building)
        {
            ArgumentNullException.ThrowIfNull(building);
            if (string.IsNullOrWhiteSpace(building.Name) || string.IsNullOrWhiteSpace(building.Address))
            {
                throw new ArgumentException("Enter both a building name and an address.");
            }

            if (building.Name != null && building.Name.Length > 150)
            {
                throw new ArgumentException("Building name cannot exceed 150 characters.");
            }

            if (building.Address != null && building.Address.Length > 250)
            {
                throw new ArgumentException("Address cannot exceed 250 characters.");
            }
        }
    }
}
 