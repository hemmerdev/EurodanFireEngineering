using Eurodan.Data;
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

                DatabaseConnectionTester connectionTester = new(connectionString);

                connectionTester.TestConnection();
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
