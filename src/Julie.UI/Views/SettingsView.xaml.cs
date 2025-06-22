using System.Windows;
using System.Windows.Controls;
using Julie.UI.ViewModels;

namespace Julie.UI.Views
{
    /// <summary>
    /// Settings view for configuring Julie AI Assistant
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsViewModel? ViewModel => DataContext as SettingsViewModel;

        public SettingsView()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel?.SaveCommand?.CanExecute(null) == true)
            {
                ViewModel.SaveCommand.Execute(null);
            }
        }
    }
}
