using Eurodan.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Eurodan.WPF.Views
{
    /// <summary>
    /// Interaction logic for ClientRegistrationView.xaml
    /// </summary>
    public partial class ClientRegistrationView : UserControl
    {
        public ClientRegistrationView()
        {
            InitializeComponent();
        }

        public ClientRegistrationView(ClientRegistrationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
