using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.Tests.Fakes;
using Eurodan.Tests.ProjectTests;

namespace Eurodan.Tests;

[TestClass]
public class ProjectServiceTests
{

    private readonly FakeProjectRepository _projectRepository;
    private readonly ProjectService _projectService;
    private readonly Project _validProject;

    public ProjectServiceTests()
    {
        _projectRepository = new FakeProjectRepository();

        var clientRepository = new FakeClientRepository();
        var buildingRepository = new FakeBuildingRepository();
        var buildingClassRepository = new FakeBuildingClassRepository();
        var technicalIssueRepository = new FakeTechnicalIssueRepository();

        var client = new Client
        {
            ClientId = 1,
            CompanyName = "Test Client",
        };

        var firstBuilding = new Building
        {
            BuildingId = 1,
            Name = "Test Building 1",
            Address = "Test Address 1",
        };
        var secondBuilding = new Building
        {
            BuildingId = 2,
            Name = "Test Building 2",
            Address = "Test Address 2",
        };

        var buildingClass = new BuildingClass
        {
            BuildingClassId = 1,
            ClassCode = "5"
        };

        var technicalIssue = new TechnicalIssue
        {
            TechnicalIssueId = 1,
            Name = "Test Technical Issue name",
            Description = "Test Technical Issue description",
        };

        clientRepository.Clients.Add(client);
        buildingRepository.Buildings.Add(firstBuilding);
        buildingRepository.Buildings.Add(secondBuilding);
        buildingClassRepository.BuildingClasses.Add(buildingClass);
        technicalIssueRepository.Issues.Add(technicalIssue);

        _projectService = new ProjectService(
            _projectRepository,
            clientRepository,
            buildingRepository,
            buildingClassRepository,
            technicalIssueRepository
        );

        _validProject = new Project
        {
            ProjectNumber = "1",
            ProjectName = "Test Project",
            ClientId = client.ClientId,
            Buildings = new() { firstBuilding, secondBuilding },
            BuildingClasses = new() { buildingClass },
            TechnicalIssues = new() { technicalIssue }
        };
    }

    // Traceability: UC01 requires saving the project and selected links.
    [TestMethod]
    public void CreateProject_ValidProject_SavesSelectedRelationships()
    {

        // Act
        int returnedProjectId = _projectService.CreateProject(_validProject);

        // Assert
        Assert.HasCount(1, _projectRepository.Projects);

        Project savedProject = _projectRepository.Projects[0];

        Assert.AreEqual(savedProject.ProjectId, returnedProjectId);
        Assert.AreEqual("1", savedProject.ProjectNumber);
        Assert.AreEqual("Test Project", savedProject.ProjectName);
        Assert.AreEqual(1, savedProject.ClientId);

        CollectionAssert.AreEquivalent(
            new[] { 1, 2 }, savedProject.Buildings.Select(b => b.BuildingId).ToList());
        CollectionAssert.AreEquivalent(
            new[] { 1 }, savedProject.BuildingClasses.Select(bc => bc.BuildingClassId).ToList());
        CollectionAssert.AreEquivalent(
            new[] { 1 }, savedProject.TechnicalIssues.Select(ti => ti.TechnicalIssueId).ToList());

    }

    // Traceability: the domain model requires at least one Building.
    [TestMethod]
    public void CreateProject_NoBuildings_ThrowsWithoutSaving()
    {
        // Arrange: change only the rule being tested.
        _validProject.Buildings.Clear();

        // Act and Assert
        ArgumentException exception =
            Assert.ThrowsExactly<ArgumentException>(
                () => _projectService.CreateProject(_validProject));

        StringAssert.Contains(
            exception.Message, "at least one building");

        Assert.HasCount(0, _projectRepository.Projects);
    }

    // UC01: The project must reference an existing client.
    [TestMethod]
    public void CreateProject_UnknownClient_ThrowsWithoutSaving()
    {
        // Arrange: everything else remains valid.
        _validProject.ClientId = 999;

        // Act and Assert
        ArgumentException exception =
            Assert.ThrowsExactly<ArgumentException>(
                () => _projectService.CreateProject(_validProject));

        StringAssert.Contains(exception.Message, "existing client");

        Assert.HasCount(0, _projectRepository.Projects);
    }

    // UC01: Project numbers must be unique.
    [TestMethod]
    public void CreateProject_DuplicateProjectNumber_ThrowsWithoutSaving()
    {
        // Arrange: an existing project already uses this number.
        var existingProject = new Project
        {
            ProjectId = 99,
            ProjectNumber = _validProject.ProjectNumber,
            ProjectName = "Existing project",
            ClientId = _validProject.ClientId,
            Buildings = new() { _validProject.Buildings[0] }
        };

        _projectRepository.Projects.Add(existingProject);

        // Act and Assert
        ArgumentException exception =
            Assert.ThrowsExactly<ArgumentException>(
                () => _projectService.CreateProject(_validProject));

        StringAssert.Contains(exception.Message, "already exists");

        // The existing project remains; no second project was saved.
        Assert.HasCount(1, _projectRepository.Projects);
        Assert.AreEqual(99, _projectRepository.Projects[0].ProjectId);
    }

    // UC04: An edit replaces attributes and links, and may keep its own project number.
    [TestMethod]
    public void UpdateProject_ValidChanges_ReplacesValuesAndLinks()
    {
        // Arrange
        var existingProject = new Project
        {
            ProjectId = 5,
            ProjectNumber = "1",
            ProjectName = "Old name",
            ClientId = 1,
            Buildings = new() { _validProject.Buildings[0] }
        };

        _projectRepository.Projects.Add(existingProject);

        // Stored and edited are separate objects. If the test changed the stored object directly,
        // it would pass even if Update did nothing.
        _validProject.ProjectId = 5;

        // Act
        _projectService.UpdateProject(_validProject);

        // Assert
        Assert.HasCount(1, _projectRepository.Projects);

        Project savedProject = _projectRepository.Projects[0];

        Assert.AreEqual(5, savedProject.ProjectId);
        Assert.AreEqual("1", savedProject.ProjectNumber);
        Assert.AreEqual("Test Project", savedProject.ProjectName);
        Assert.AreEqual(1, savedProject.ClientId);

        CollectionAssert.AreEquivalent(
            new[] { 1, 2 }, savedProject.Buildings.Select(b => b.BuildingId).ToList());
    }

    // UC04: Changed links must not contain duplicates.
    [TestMethod]
    public void UpdateProject_DuplicateBuilding_ThrowsWithoutSaving()
    {
        // Arrange
        var existingProject = new Project
        {
            ProjectId = 5,
            ProjectNumber = "1",
            ProjectName = "Old name",
            ClientId = 1,
            Buildings = new() { _validProject.Buildings[0] }
        };

        _projectRepository.Projects.Add(existingProject);

        // Stored and edited are separate objects. If the test changed the stored object directly,
        // it would pass even if Update did nothing.
        _validProject.ProjectId = 5;

        // The same building twice is a duplicate link.
        _validProject.Buildings.Add(_validProject.Buildings[0]);

        // Act and Assert
        ArgumentException exception =
            Assert.ThrowsExactly<ArgumentException>(
                () => _projectService.UpdateProject(_validProject));

        StringAssert.Contains(exception.Message, "more than once");

        Assert.AreEqual("Old name", _projectRepository.Projects[0].ProjectName);
    }

    // UC04: A project cannot take another project's number.
    [TestMethod]
    public void UpdateProject_NumberUsedByAnotherProject_ThrowsWithoutSaving()
    {
        // Arrange
        var existingProject = new Project
        {
            ProjectId = 5,
            ProjectNumber = "1",
            ProjectName = "Old name",
            ClientId = 1,
            Buildings = new() { _validProject.Buildings[0] }
        };

        _projectRepository.Projects.Add(existingProject);

        var otherProject = new Project
        {
            ProjectId = 6,
            ProjectNumber = "2",
            ProjectName = "Other project",
            ClientId = 1,
            Buildings = new() { _validProject.Buildings[0] }
        };

        _projectRepository.Projects.Add(otherProject);

        // Stored and edited are separate objects. If the test changed the stored object directly,
        // it would pass even if Update did nothing.
        _validProject.ProjectId = 5;

        _validProject.ProjectNumber = "2";

        // Act and Assert
        ArgumentException exception =
            Assert.ThrowsExactly<ArgumentException>(
                () => _projectService.UpdateProject(_validProject));

        StringAssert.Contains(exception.Message, "already exists");

        Assert.AreEqual("Old name", _projectRepository.Projects[0].ProjectName);
    }

    // UC04: Editing a project that no longer exists fails with a clear message.
    [TestMethod]
    public void UpdateProject_UnknownProject_Throws()
    {
        // Arrange
        
        // No project has this ID, like one that was deleted while the form was open.
        _validProject.ProjectId = 999;

        // Act and Assert
        ArgumentException exception =
            Assert.ThrowsExactly<ArgumentException>(
                () => _projectService.UpdateProject(_validProject));

        StringAssert.Contains(exception.Message, "could not be found");

        Assert.HasCount(0, _projectRepository.Projects);
    }
}
