using Eurodan.Core.Models;
using Eurodan.Core.Services;
using Eurodan.Tests.Fakes;
using Eurodan.Tests.ProjectTests;

namespace Eurodan.Tests
{
    [TestClass]
    public class ClientServiceTests
    {
        [TestMethod]
        public void RegisterClient_MissingCompanyName_ThrowsArgumentException()
        {
            var clientRepository = new FakeClientRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new ClientService(clientRepository, projectRepository);

            var client = new Client
            {
                CompanyName = "",
                ContactName = "This Client"
            };

            Assert.ThrowsExactly<ArgumentException>(() => service.RegisterClient(client));
            Assert.IsFalse(clientRepository.AddCalled);
        }

        [TestMethod]
        public void RegisterClient_NullContactName_ValidClient_ReturnsClientId()
        {
            var clientRepository = new FakeClientRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new ClientService(clientRepository, projectRepository);

            var client = new Client
            {
                CompanyName = "This Client",
                ContactName = null
            };

            int id = service.RegisterClient(client);

            Assert.IsTrue(clientRepository.AddCalled);
            Assert.AreEqual(12, id);
        }

        [TestMethod]
        public void RegisterClient_ValidClient_ReturnsClientId()
        {
            
            var clientRepository = new FakeClientRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new ClientService(clientRepository, projectRepository);


            var client = new Client
            {
                CompanyName = "This Client",
                ContactName = "Client Clienton"
            };

            int id = service.RegisterClient(client);

            Assert.IsTrue(clientRepository.AddCalled);
            Assert.AreEqual(12, id);
        }

        //Tests whether deletion can be completed when Client is still assigned to project.
        [TestMethod]
        public void DeleteClient_Still_Assigned()
        {
            var clientRepository = new FakeClientRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new ClientService(clientRepository, projectRepository);

            var project = new Project
            {
                ProjectId = 23,
                ClientId = 5,
                IsArchived = true
            };

            var client = new Client
            {
                ClientId = 5,
                CompanyName = "Smoke Control"
            };


            clientRepository.Clients.Add(client);
            projectRepository.Projects.Add(project);

            Assert.ThrowsExactly<InvalidOperationException>(() => service.DeleteClient(5));
            Assert.IsFalse(clientRepository.DeleteCalled);
        }

        //Tests whether deletion can be completed when Client is still assigned to project.
        [TestMethod]
        public void DeleteClient_Unknown_Id()
        {
            var clientRepository = new FakeClientRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new ClientService(clientRepository, projectRepository);

            var client = new Client
            {
                ClientId = 5,
                CompanyName = "Smoke Control"
            };

            clientRepository.Clients.Add(client);

            bool deleted = service.DeleteClient(999);

            Assert.IsFalse(deleted);
            Assert.IsFalse(clientRepository.DeleteCalled);
        }

        //Tests whether deletion can be completed when Client is not assigned to a project. 
        [TestMethod]
        public void DeleteClient_Not_Assigned()
        {
            var clientRepository = new FakeClientRepository();
            var projectRepository = new FakeProjectRepository();

            var service = new ClientService(clientRepository, projectRepository);

            var project = new Project
            {
                ClientId = 6,
                ProjectId = 12,
                IsArchived = true
            };

            var client = new Client
            {
                ClientId = 5,
                CompanyName = "Smoke Control"
            };

            clientRepository.Clients.Add(client);
            projectRepository.Projects.Add(project);

            bool deleted = service.DeleteClient(5);

            Assert.IsTrue(deleted);
            Assert.IsTrue(clientRepository.DeleteCalled);
        }
    }
}
