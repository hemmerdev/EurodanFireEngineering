using Eurodan.Core.Interfaces;
using Eurodan.Core.Models;

namespace Eurodan.Core.Services
{
    public class BuildingService
    {
        private readonly IRepository<Building> _buildingRepository;
        private readonly IReadOnlyRepository<Project> _projectRepository;

        public BuildingService(IRepository<Building> buildingRepository, IReadOnlyRepository<Project> projectRepository)
        {
            _buildingRepository = buildingRepository;
            _projectRepository = projectRepository;
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

        public bool DeleteBuilding(int buildingId)
        {
            if (_buildingRepository.GetById(buildingId) == null)
            {
                return false;
            }

            if (_projectRepository.GetAll()
                .Any(project => project.Buildings
                .Any(building => building.BuildingId == buildingId)))
            {
                throw new InvalidOperationException("The building cannot be deleted, as it is assigned to a project.");
            }

            _buildingRepository.Delete(buildingId);
            return true;

        }
    }
}
 