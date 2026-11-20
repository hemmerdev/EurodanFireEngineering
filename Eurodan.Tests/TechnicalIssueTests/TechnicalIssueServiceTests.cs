using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.Tests.Fakes;

namespace Eurodan.Tests
{
    [TestClass]
    public class TechnicalIssueServiceTests
    {
        [TestMethod]
        public void CreateTechnicalIssue_DuplicateName_ThrowsArgumentException()
        {
            // Arrange
            var repository = new FakeTechnicalIssueRepository();

            repository.Issues.Add(new TechnicalIssue
            {
                TechnicalIssueId = 1,
                Name = "Smoke control"
            });

            var service = new TechnicalIssueService(repository);

            var newIssue = new TechnicalIssue
            {
                Name = " smoke CONTROL "
            };

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(
                () => service.CreateTechnicalIssue(newIssue));

            Assert.IsFalse(repository.AddCalled);
        }

        [TestMethod]
        public void UpdateTechnicalIssue_UnknownId_ReturnsFalse()
        {
            // Arrange
            var repository = new FakeTechnicalIssueRepository();
            var service = new TechnicalIssueService(repository);

            var issue = new TechnicalIssue
            {
                TechnicalIssueId = 999,
                Name = "Smoke control"
            };

            // Act
            bool updated = service.UpdateTechnicalIssue(issue);

            // Assert
            Assert.IsFalse(updated);
            Assert.IsFalse(repository.UpdateCalled);
        }
    }
}