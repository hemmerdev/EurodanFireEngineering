using Eurodan.Core.Models;

// This represents one building choice in the project creation form. 

namespace Eurodan.WPF.ViewModels
{
    public class BuildingSelectionViewModel : ViewModelBase
    {
        private bool _isSelected;

        public Building Building { get; }

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

        public BuildingSelectionViewModel(Building building)
        {
            Building = building ?? throw new ArgumentNullException(nameof(building));
        }
    }
}
