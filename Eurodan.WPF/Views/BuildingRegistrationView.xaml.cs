using Eurodan.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Eurodan.WPF.Views
{
    /// <summary>
    /// Interaction logic for BuildingRegistrationView.xaml
    /// </summary>
    public partial class BuildingRegistrationView : UserControl
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
    }
}
