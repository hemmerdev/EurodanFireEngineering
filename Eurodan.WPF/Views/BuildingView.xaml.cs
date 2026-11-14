using Eurodan.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Eurodan.WPF.Views
{
    /// <summary>
    /// Interaction logic for BuildingView.xaml
    /// </summary>
    public partial class BuildingView : UserControl
    {
        public BuildingView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BuildingViewModel buildingViewModel)
            {
                BuildingRegistrationViewModel registrationViewModel = buildingViewModel.CreateBuildingRegistrationViewModel();
                BuildingRegistrationView brv = new(registrationViewModel);
                brv.Show();
            }
        }
    }
}
