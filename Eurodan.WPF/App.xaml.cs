using Eurodan.Core.Services;
using Eurodan.Data;
using Eurodan.Data.Repositories;
using Eurodan.WPF.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.IO;
using System.Windows;

namespace Eurodan.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Build configuration from appsettings.json
                IConfigurationRoot config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();

                // Retrieve the connection string from the configuration
                string? connectionString =
                    config.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    MessageBox.Show(
                        "The program could not start because a valid ConnectionString is not specified in appsettings.json. " +
                        "Check the database configuration and try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Shutdown();
                    return;
                }

                // Test the database connection
                DatabaseConnectionTester connectionTester = new(connectionString);
                connectionTester.TestConnection();

                // Create SqlRepositories
                SqlClientRepository clientRepository = new(connectionString);
                SqlBuildingRepository buildingRepository = new(connectionString);
                SqlTechnicalIssueRepository technicalIssueRepository = new(connectionString);
                SqlBuildingClassRepository buildingClassRepository = new(connectionString);
                SqlProjectRepository projectRepository = new(connectionString);

                // Create service instances
                ClientService clientService = new(
                    clientRepository,
                    projectRepository);
                BuildingService buildingService = new(buildingRepository);
                TechnicalIssueService technicalIssueService = new(
                    technicalIssueRepository,
                    projectRepository);
                ProjectService projectService = new(
                    projectRepository,
                    clientRepository,
                    buildingRepository,
                    buildingClassRepository,
                    technicalIssueRepository);

                // Initialize the MainViewModel and MainWindow
                MainViewModel mainViewModel = new(clientService, buildingService, technicalIssueService, projectService);
                MainWindow mainWindow = new(mainViewModel);
                mainWindow.Show();
            }

            catch (FileNotFoundException)
            {
                MessageBox.Show(
                    "The program could not start because appsettings.json was not found. " +
                    "Create the file from appsettings.example.json, and try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
            catch (InvalidDataException)
            {
                MessageBox.Show(
                    "The program could not start because appsettings.json could not be read. " +
                    "Check that the file contains valid JSON, and try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
            catch (DbException)
            {
                MessageBox.Show(
                    "The program could not start because the database could not be contacted or data could not be loaded. " +
                    "Check the database configuration, and try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
            }
        }
    }
}
