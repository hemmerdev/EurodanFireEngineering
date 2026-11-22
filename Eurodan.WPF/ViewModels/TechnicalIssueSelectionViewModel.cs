using Eurodan.Core.Models;

// This represents one technical issue choice in the project creation form. 

namespace Eurodan.WPF.ViewModels
{
    public class TechnicalIssueSelectionViewModel : ViewModelBase
    {
        private bool _isSelected;

        public TechnicalIssue TechnicalIssue { get; }

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

        public TechnicalIssueSelectionViewModel(TechnicalIssue technicalIssue)
        {
            TechnicalIssue = technicalIssue ??
                throw new ArgumentNullException(nameof(technicalIssue));
        }
    }
}
