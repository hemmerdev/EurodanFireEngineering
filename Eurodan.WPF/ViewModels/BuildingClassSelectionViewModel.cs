using Eurodan.Core.Models;

// This represents one building class choice in the project creation form. 

namespace Eurodan.WPF.ViewModels
{
    public class BuildingClassSelectionViewModel : ViewModelBase
    {
        private bool _isSelected;

        public BuildingClass BuildingClass { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public BuildingClassSelectionViewModel(BuildingClass buildingClass)
        {
            BuildingClass = buildingClass ??
                throw new ArgumentNullException(nameof(buildingClass));
        }
    }
}
