using Microsoft.UI.Xaml.Controls;
using Julie.UI.ViewModels;

namespace Julie.UI.Views
{
    /// <summary>
    /// Settings view for configuring Julie AI Assistant
    /// </summary>
    public sealed partial class SettingsView : Page
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (e.Parameter is SettingsViewModel viewModel)
            {
                DataContext = viewModel;
            }
            base.OnNavigatedTo(e);
        }
    }
}
