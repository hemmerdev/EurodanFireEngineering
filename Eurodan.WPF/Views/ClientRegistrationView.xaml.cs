using Eurodan.WPF.ViewModels;
using System.Windows;

namespace Eurodan.WPF.Views
{
    /// <summary>
    /// Interaction logic for ClientRegistrationView.xaml
    /// </summary>
    public partial class ClientRegistrationView : Window
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
