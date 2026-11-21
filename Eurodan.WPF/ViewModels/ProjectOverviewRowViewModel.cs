using Eurodan.Core.Models;

// Represents a single row in the Project Overview view
namespace Eurodan.WPF.ViewModels
{
    public class ProjectOverviewRowViewModel
    {
        public Project Project { get; }
        public string ClientCompanyName { get; }
        public string BuildingNames =>
            string.Join(", ", Project.Buildings.Select(building => building.Name));
        public string BuildingClassCodes =>
            string.Join(", ", Project.BuildingClasses.Select(bc => bc.ClassCode));
        public string TechnicalIssueNames =>
            string.Join(", ", Project.TechnicalIssues.Select(ti => ti.Name));


        public ProjectOverviewRowViewModel(Project project, string clientCompanyName)
        {
            Project = project ??
                throw new ArgumentNullException(nameof(project));
            ClientCompanyName = clientCompanyName ??
                throw new ArgumentNullException(nameof(clientCompanyName));
        }
    }
}
