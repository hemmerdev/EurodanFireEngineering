using Eurodan.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Eurodan.WPF.Views
{
    /// <summary>
    /// Interaction logic for ClientView.xaml
    /// </summary>
    public partial class ClientView : UserControl
    {
        public ClientView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ClientViewModel clientViewModel)
            {
                ClientRegistrationViewModel registrationViewModel = clientViewModel.CreateClientRegistrationViewModel();
                ClientRegistrationView crv = new(registrationViewModel);
                crv.Show();
            }
        }
    }
}
