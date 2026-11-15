using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.Tests.Fakes;
using Microsoft.IdentityModel.Tokens;

namespace Eurodan.Tests
{
    [TestClass]
    public class BuildingServiceTests
    {
        [TestMethod]
        public void RegisterBuilding_MissingAddress_ThrowsArgumentException()
        {
            var repository = new FakeBuildingRepository();
            var service = new BuildingService(repository);

            var building = new Building
            {
                Name = "This Place",
                Address = ""
            };

            Assert.ThrowsExactly<ArgumentException>(() => service.RegisterBuilding(building));
            Assert.IsFalse(repository.AddCalled);
        }

        [TestMethod]
        public void RegisterBuilding_MissingName_ThrowsArgumentException()
        {
            var repository = new FakeBuildingRepository();
            var service = new BuildingService(repository);

            var building = new Building
            {
                Name = "",
                Address = "This Place 2, 123456 Thisplace"
            };

            Assert.ThrowsExactly<ArgumentException>(() => service.RegisterBuilding(building));
            Assert.IsFalse(repository.AddCalled);
        }

        [TestMethod]
        public void RegisterBuilding_ValidBuilding_ReturnsBuildingId()
        {
            var repository = new FakeBuildingRepository();
            var service = new BuildingService(repository);
           

            var building = new Building
            {
                Name = "This Place",
                Address = "This Place 2, 123456 Thisplace"
            };

            int id = service.RegisterBuilding(building);

            Assert.IsTrue(repository.AddCalled);
            Assert.AreEqual(42, id);
        }
    }
}
