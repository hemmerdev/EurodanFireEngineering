using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.Tests.Fakes;
using Eurodan.Tests.ProjectTests;

namespace Eurodan.Tests
{
    [TestClass]
    public class TechnicalIssueServiceTests
    {
        [TestMethod]
        public void CreateTechnicalIssue_DuplicateName_ThrowsArgumentException()
        {
            // Arrange
            var technicalIssueRepository = new FakeTechnicalIssueRepository();
            var projectRepository = new FakeProjectRepository();

            technicalIssueRepository.Issues.Add(new TechnicalIssue
            {
                TechnicalIssueId = 1,
                Name = "Smoke control"
            });

            var service = new TechnicalIssueService(technicalIssueRepository, projectRepository);

            var newIssue = new TechnicalIssue
            {
                Name = " smoke CONTROL "
            };

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(
                () => service.CreateTechnicalIssue(newIssue));

            Assert.IsFalse(technicalIssueRepository.AddCalled);
        }

        [TestMethod]
        public void UpdateTechnicalIssue_UnknownId_ReturnsFalse()
        {
            // Arrange
            var technicalIssueRepository = new FakeTechnicalIssueRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new TechnicalIssueService(technicalIssueRepository, projectRepository);

            var issue = new TechnicalIssue
            {
                TechnicalIssueId = 999,
                Name = "Smoke control"
            };

            // Act
            bool updated = service.UpdateTechnicalIssue(issue);

            // Assert
            Assert.IsFalse(updated);
            Assert.IsFalse(technicalIssueRepository.UpdateCalled);
        }

        //Tests whether deletion can be completed when TechnicalIssue is still assigned to project.
        [TestMethod]
        public void DeleteTechnicalIssue_Still_Assigned()
        {
            var technicalIssueRepository = new FakeTechnicalIssueRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new TechnicalIssueService(technicalIssueRepository, projectRepository);

            var project = new Project
            {
                ProjectId = 12,
                IsArchived = true
            };

            var issue = new TechnicalIssue
            {
                TechnicalIssueId = 5,
                Name = "Smoke Control"
            };

            technicalIssueRepository.Issues.Add(issue);
            project.TechnicalIssues.Add(issue);

            projectRepository.Projects.Add(project);

            Assert.ThrowsExactly<InvalidOperationException>(() => service.DeleteTechnicalIssue(5));
            Assert.IsFalse(technicalIssueRepository.DeleteCalled);
        }

        //Tests whether deletion can be completed with an ID that does not exist.
        [TestMethod]
        public void DeleteTechnicalIssue_Unknown_Id()
        {
            var technicalIssueRepository = new FakeTechnicalIssueRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new TechnicalIssueService(technicalIssueRepository, projectRepository);

            var issue = new TechnicalIssue
            {
                TechnicalIssueId = 5,
                Name = "Smoke Control"
            };

            technicalIssueRepository.Issues.Add(issue);

            bool deleted = service.DeleteTechnicalIssue(999);

            Assert.IsFalse(deleted);
            Assert.IsFalse(technicalIssueRepository.DeleteCalled);
        }

        //Tests whether deletion can be completed when TechnicalIssue is not assigned to a project. 
        [TestMethod]
        public void DeleteTechnicalIssue_Not_Assigned()
        {
            var technicalIssueRepository = new FakeTechnicalIssueRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new TechnicalIssueService(technicalIssueRepository, projectRepository);

            var project = new Project
            {
                ProjectId = 12,
                IsArchived = true
            };

            var issue = new TechnicalIssue
            {
                TechnicalIssueId = 5,
                Name = "Smoke Control"
            };

            technicalIssueRepository.Issues.Add(issue);
            projectRepository.Projects.Add(project);

            bool deleted = service.DeleteTechnicalIssue(5);

            Assert.IsTrue(deleted);
            Assert.IsTrue(technicalIssueRepository.DeleteCalled);
        }
    }
}