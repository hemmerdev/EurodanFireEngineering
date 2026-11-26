using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.Tests.Fakes;
using Microsoft.IdentityModel.Tokens;
using Eurodan.Tests.ProjectTests;

namespace Eurodan.Tests
{
    [TestClass]
    public class BuildingServiceTests
    {
        [TestMethod]
        public void RegisterBuilding_MissingAddress_ThrowsArgumentException()
        {
            var buildingRepository = new FakeBuildingRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new BuildingService(buildingRepository, projectRepository);

            var building = new Building
            {
                Name = "This Place",
                Address = ""
            };

            Assert.ThrowsExactly<ArgumentException>(() => service.RegisterBuilding(building));
            Assert.IsFalse(buildingRepository.AddCalled);
        }

        [TestMethod]
        public void RegisterBuilding_MissingName_ThrowsArgumentException()
        {
            var buildingRepository = new FakeBuildingRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new BuildingService(buildingRepository, projectRepository);

            var building = new Building
            {
                Name = "",
                Address = "This Place 2, 123456 Thisplace"
            };

            Assert.ThrowsExactly<ArgumentException>(() => service.RegisterBuilding(building));
            Assert.IsFalse(buildingRepository.AddCalled);
        }

        [TestMethod]
        public void RegisterBuilding_ValidBuilding_ReturnsBuildingId()
        {
            var buildingRepository = new FakeBuildingRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new BuildingService(buildingRepository, projectRepository);


            var building = new Building
            {
                Name = "This Place",
                Address = "This Place 2, 123456 Thisplace"
            };

            int id = service.RegisterBuilding(building);

            Assert.IsTrue(buildingRepository.AddCalled);
            Assert.AreEqual(42, id);
        }

        //Tests whether deletion can be completed when Building is still assigned to a project.
        [TestMethod]
        public void DeleteBuilding_Still_Assigned()
        {
            var buildingRepository = new FakeBuildingRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new BuildingService(buildingRepository, projectRepository);

            var project = new Project
            {
                ProjectId = 12,
                IsArchived = false
            };

            var building = new Building
            {
                BuildingId = 5,
                Name = "Smoke Control"
            };

            buildingRepository.Buildings.Add(building);
            project.Buildings.Add(building);

            projectRepository.Projects.Add(project);

            Assert.ThrowsExactly<InvalidOperationException>(() => service.DeleteBuilding(5));
            Assert.IsFalse(buildingRepository.DeleteCalled);
        }

        ////Tests whether deletion can be completed when Building is still assigned to an archived project.
        [TestMethod]
        public void DeleteBuilding_Still_Assigned_Archived()
        {
            var buildingRepository = new FakeBuildingRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new BuildingService(buildingRepository, projectRepository);

            var project = new Project
            {
                ProjectId = 12,
                IsArchived = true
            };

            var building = new Building
            {
                BuildingId = 5,
                Name = "Smoke Control"
            };

            buildingRepository.Buildings.Add(building);
            project.Buildings.Add(building);

            projectRepository.Projects.Add(project);

            Assert.ThrowsExactly<InvalidOperationException>(() => service.DeleteBuilding(5));
            Assert.IsFalse(buildingRepository.DeleteCalled);
        }

        //Tests whether deletion can be completed with an ID that doesn't exist.
        [TestMethod]
        public void DeleteBuilding_Unknown_Id()
        {
            var buildingRepository = new FakeBuildingRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new BuildingService(buildingRepository, projectRepository);

            var building = new Building
            {
                BuildingId = 5,
                Name = "Smoke Control"
            };

            buildingRepository.Buildings.Add(building);

            bool deleted = service.DeleteBuilding(999);

            Assert.IsFalse(deleted);
            Assert.IsFalse(buildingRepository.DeleteCalled);
        }

        //Tests whether deletion can be completed when Building is not assigned to a project. 
        [TestMethod]
        public void DeleteBuilding_Not_Assigned()
        {
            var buildingRepository = new FakeBuildingRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new BuildingService(buildingRepository, projectRepository);

            var project = new Project
            {
                ProjectId = 12,
                IsArchived = false
            };

            var building = new Building
            {
                BuildingId = 5,
                Name = "Smoke Control"
            };

            buildingRepository.Buildings.Add(building);
            projectRepository.Projects.Add(project);

            bool deleted = service.DeleteBuilding(5);

            Assert.IsTrue(deleted);
            Assert.IsTrue(buildingRepository.DeleteCalled);
        }

        ////Tests whether deletion can be completed when Building is not assigned to an archived project. 
        [TestMethod]
        public void DeleteBuilding_Not_Assigned_Archived()
        {
            var buildingRepository = new FakeBuildingRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new BuildingService(buildingRepository, projectRepository);

            var project = new Project
            {
                ProjectId = 12,
                IsArchived = true
            };

            var building = new Building
            {
                BuildingId = 5,
                Name = "Smoke Control"
            };

            buildingRepository.Buildings.Add(building);
            projectRepository.Projects.Add(project);

            bool deleted = service.DeleteBuilding(5);

            Assert.IsTrue(deleted);
            Assert.IsTrue(buildingRepository.DeleteCalled);
        }
    }
}
