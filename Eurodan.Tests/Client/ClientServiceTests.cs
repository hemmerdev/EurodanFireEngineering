using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.Tests.Fakes;

namespace Eurodan.Tests
{
    [TestClass]
    public class ClientServiceTests
    {
        [TestMethod]
        public void RegisterClient_MissingCompanyName_ThrowsArgumentException()
        {
            var repository = new FakeClientRepository();
            var service = new ClientService(repository);

            var client = new Client
            {
                CompanyName = "",
                ContactName = "This Client"
            };

            Assert.ThrowsExactly<ArgumentException>(() => service.RegisterClient(client));
            Assert.IsFalse(repository.AddCalled);
        }

        [TestMethod]
        public void RegisterClient_NullContactName_ValidClient_ReturnsClientId()
        {
            var repository = new FakeClientRepository();
            var service = new ClientService(repository);

            var client = new Client
            {
                CompanyName = "This Client",
                ContactName = null
            };

            int id = service.RegisterClient(client);

            Assert.IsTrue(repository.AddCalled);
            Assert.AreEqual(12, id);
        }

        [TestMethod]
        public void RegisterClient_ValidClient_ReturnsClientId()
        {
            
            var repository = new FakeClientRepository();
            var service = new ClientService(repository);


            var client = new Client
            {
                CompanyName = "This Client",
                ContactName = "Client Clienton"
            };

            int id = service.RegisterClient(client);

            Assert.IsTrue(repository.AddCalled);
            Assert.AreEqual(12, id);
        }
    }
}
