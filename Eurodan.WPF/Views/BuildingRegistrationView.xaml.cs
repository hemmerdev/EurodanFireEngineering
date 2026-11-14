using Eurodan.WPF.ViewModels;
using System.Windows;

namespace Eurodan.WPF.Views
{
    /// <summary>
    /// Interaction logic for BuildingRegistrationView.xaml
    /// </summary>
    public partial class BuildingRegistrationView : Window
    {
        public BuildingRegistrationView()
        {
            InitializeComponent();
        }

        public BuildingRegistrationView(BuildingRegistrationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
