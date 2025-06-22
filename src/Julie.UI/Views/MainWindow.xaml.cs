using System;
using System.Windows;
using System.Windows.Controls;
using Julie.UI.ViewModels;

namespace Julie.UI.Views
{
    /// <summary>
    /// Main window for the Julie AI Assistant application
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;
            
            // Show chat view by default
            ShowChatView();
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            ShowChatView();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsView();
        }

        private void ShowChatView()
        {
            var chatView = new ChatView();
            chatView.DataContext = ViewModel.ChatViewModel;
            ContentFrame.Content = chatView;
        }

        private void ShowSettingsView()
        {
            var settingsView = new SettingsView();
            settingsView.DataContext = ViewModel.SettingsViewModel;
            ContentFrame.Content = settingsView;
        }
    }
}
