using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.WPF.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Eurodan.WPF.ViewModels
{
    public class TechnicalIssueManagementViewModel : ViewModelBase
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
        public ICommand UpdateIssueCommand { get; }
        public ICommand DeleteIssueCommand { get; }


        public TechnicalIssueManagementViewModel(TechnicalIssueService technicalIssueService)
        {
            _technicalIssueService = technicalIssueService;
            AddIssueCommand = new RelayCommand(AddIssue);
            UpdateIssueCommand = new RelayCommand(UpdateIssue);
            DeleteIssueCommand = new RelayCommand(DeleteIssue);

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
                int id = _technicalIssueService.CreateTechnicalIssue(technicalIssue);

                StatusMessage = $"New issue tag added.";

                // Clearing the fields to avoid double-clicking for double registering
                Name = "";
                Description = "";
                
            }
            catch (ArgumentException ex)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception)
            {
                StatusMessage = "Could not save the tag.";
            }

            SelectedIssue = null;
            LoadIssues();
        }

        private void UpdateIssue(object? parameter)
        {
            if (SelectedIssue == null)
            {
                StatusMessage = "Please select a tag to update.";
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

                if (updated)
                {
                    SelectedIssue = null;
                    LoadIssues();
                    StatusMessage = "Tag updated successfully.";
                }
                else
                {
                    StatusMessage = "The selected tag could not be found.";
                }
            }
            catch (ArgumentException ex)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception)
            {
                StatusMessage = "Could not update the tag.";
            }
        }

        private void DeleteIssue(object? parameter)
        {
            if (SelectedIssue == null)
            {
                StatusMessage = "Please select a tag to delete.";
                return;
            }

            try
            {
                bool deleted = _technicalIssueService.DeleteTechnicalIssue(SelectedIssue.TechnicalIssueId);

                if (deleted)
                {
                    SelectedIssue = null;
                    LoadIssues();
                    StatusMessage = "Tag deleted successfully.";
                }
                else
                {
                    StatusMessage = "The selected tag could not be found.";
                }
            }
            catch (Exception)
            {
                StatusMessage = "Could not delete the tag. It may already be in use.";
            }
        }

        private void LoadIssues()
        {
            TechnicalIssues.Clear();
            
            foreach (var issue in _technicalIssueService.GetAllTechnicalIssues())
            {
                TechnicalIssues.Add(issue);
            }
        }
    }
}
