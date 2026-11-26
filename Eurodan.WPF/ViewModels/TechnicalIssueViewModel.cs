using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Eurodan.WPF.ViewModels
{
    public class TechnicalIssueViewModel : ViewModelBase
    {
        private readonly TechnicalIssueService _technicalIssueService;

        public ObservableCollection<TechnicalIssue> TechnicalIssues { get; } = new();

        private TechnicalIssue? _selectedIssue;



        public TechnicalIssue? SelectedIssue
        {

            get => _selectedIssue;
            set
            {
                if (_selectedIssue != value)
                {
                    _selectedIssue = value;
                    OnPropertyChanged();
                    
                    DeleteIssueCommand.RaiseCanExecuteChanged();
                    UpdateIssueCommand.RaiseCanExecuteChanged();

                    Name = value?.Name ?? string.Empty;
                    Description = value?.Description;
                }
            }
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }

        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand AddIssueCommand { get; }
        public RelayCommand UpdateIssueCommand { get; }
        public RelayCommand DeleteIssueCommand { get; }


        public TechnicalIssueViewModel(TechnicalIssueService technicalIssueService)
        {
            _technicalIssueService = technicalIssueService;
            AddIssueCommand = new RelayCommand(AddIssue);
            UpdateIssueCommand = new RelayCommand(
                UpdateIssue,
                _ => SelectedIssue != null);
            DeleteIssueCommand = new RelayCommand(
                DeleteIssue,
                _ => SelectedIssue != null);

            LoadIssues();
        }

        private void AddIssue(object? parameter)
        {
            TechnicalIssue technicalIssue = new TechnicalIssue
            {
                Name = Name,
                Description = Description
            };

            try
            {
                _technicalIssueService.CreateTechnicalIssue(technicalIssue);

                StatusMessage = $"New technical issue {technicalIssue.Name} added.";

                // Clearing the fields to avoid double-clicking for double registering
                Name = "";
                Description = "";
                SelectedIssue = null;
            }
            catch (ArgumentException ex)
            {
                StatusMessage = ex.Message;
                return;
            }
            catch (Exception)
            {
                StatusMessage = "Could not save the technical issue.";
                return;
            }

            try
            {
                LoadIssues();

            }
            catch (Exception)
            {
                StatusMessage = "The technical issue was saved, but could not reload the list.";
                return;
            }
        }

        private void UpdateIssue(object? parameter)
        {
            if (SelectedIssue == null)
            {
                StatusMessage = "Please select a technical issue to update.";
                return;
            }

            var updatedIssue = new TechnicalIssue
            {
                TechnicalIssueId = SelectedIssue.TechnicalIssueId,
                Name = Name,
                Description = Description
            };

            try
            {
                bool updated = _technicalIssueService.UpdateTechnicalIssue(updatedIssue);

                if (!updated)
                {
                    StatusMessage = "The selected technical issue could not be updated.";
                    return;
                }
            }
            catch (ArgumentException ex)
            {
                StatusMessage = ex.Message;
                return;
            }
            catch (Exception)
            {
                StatusMessage = "Could not update the technical issue.";
                return;
            }

            // Successfully updated, clear the fields and reload the issues
            try
            {
                SelectedIssue = null;
                Name = "";
                Description = "";
                LoadIssues();
                StatusMessage = $"Technical issue {updatedIssue.Name} updated successfully.";
            }
            catch (Exception)
            {
                StatusMessage = $"The technical issue {updatedIssue.Name} was updated, but could not reload the list.";
                return;
            }
        }

        private void DeleteIssue(object? parameter)
        {
            if (SelectedIssue == null)
            {
                StatusMessage = "Please select a technical issue to delete.";
                return;
            }

            int technicalIssueId = SelectedIssue.TechnicalIssueId;
            string name = SelectedIssue.Name;

            try
            {
                bool deleted = _technicalIssueService.DeleteTechnicalIssue(technicalIssueId);

                if (!deleted)
                {
                    StatusMessage = $"The technical issue {name} could not be deleted.";
                    return;
                }
            }
            catch (ArgumentException ex)
            {
                StatusMessage = ex.Message;
                return;
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = ex.Message;
                return;
            }
            catch (Exception)
            {
                StatusMessage = "Could not delete the technical issue.";
                return;
            }

            // Successfully deleted, clear the fields and reload the issues
            try
            {
                SelectedIssue = null;
                Name = "";
                Description = "";
                LoadIssues();
                StatusMessage = $"Technical issue: {name} deleted successfully.";
            }
            catch (Exception)
            {
                StatusMessage = $"The technical issue was deleted, but could not reload the list.";
                return;
            }
        }

        private void LoadIssues()
        {
            var newIssues = _technicalIssueService.GetAllTechnicalIssues();

            TechnicalIssues.Clear();
            foreach (var issue in newIssues)
            {
                TechnicalIssues.Add(issue);
            }
        }
    }
}
