using Eurodan.Core.Services;

namespace Eurodan.WPF.ViewModels
{
    public class BuildingViewModel : ViewModelBase
    {
        private readonly BuildingService _buildingService;

        public BuildingViewModel(BuildingService buildingService)
        {
            _buildingService = buildingService;
        }

        public BuildingRegistrationViewModel CreateBuildingRegistrationViewModel()
        {
            BuildingRegistrationViewModel buildingRegistrationViewModel = new(_buildingService);
            return buildingRegistrationViewModel;
        }
    }
}
