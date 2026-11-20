using Eurodan.Core.Services;

namespace Eurodan.WPF.ViewModels
{
    public class ProjectViewModel : ViewModelBase
    {

        private readonly ProjectService _projectService;

        public ProjectViewModel(ProjectService projectService)
        {
            _projectService = projectService;
        }



    }
}
