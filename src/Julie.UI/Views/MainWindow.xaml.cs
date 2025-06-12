using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Julie.UI.ViewModels;

namespace Julie.UI.Views
{
    /// <summary>
    /// Main window for the Julie AI Assistant application
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow(MainViewModel viewModel)        {
            this.InitializeComponent();
            ViewModel = viewModel;
            
            // Navigate to chat view by default
            ContentFrame.Navigate(typeof(ChatView), ViewModel.ChatViewModel);
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(ChatView), ViewModel.ChatViewModel);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(SettingsView), ViewModel.SettingsViewModel);
        }
    }
}
