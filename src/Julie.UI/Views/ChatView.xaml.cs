using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Julie.UI.ViewModels;

namespace Julie.UI.Views
{
    /// <summary>
    /// Chat view for interacting with Julie AI Assistant
    /// </summary>
    public partial class ChatView : UserControl
    {
        public ChatViewModel? ViewModel => DataContext as ChatViewModel;
        private const string PlaceholderText = "Type your message here...";

        public ChatView()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel?.SendMessageCommand?.CanExecute(null) == true && !IsPlaceholderActive())
            {
                ViewModel.SendMessageCommand.Execute(null);
                SetPlaceholder();
            }
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            // Show recording disclaimer
            var result = MessageBox.Show(
                "🎤 Voice Recording Disclaimer\n\n" +
                "• Your voice will be recorded and processed by AI\n" +
                "• Audio data may be sent to external services for processing\n" +
                "• Recordings are not stored permanently but processed in real-time\n" +
                "• Please ensure you're in a suitable environment for recording\n\n" +
                "Do you want to proceed with voice recording?",
                "Voice Recording - Privacy Notice",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                if (ViewModel?.StartVoiceInputCommand?.CanExecute(null) == true)
                {
                    ViewModel.StartVoiceInputCommand.Execute(null);
                }
            }
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                if (ViewModel?.SendMessageCommand?.CanExecute(null) == true && !IsPlaceholderActive())
                {
                    ViewModel.SendMessageCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void SetPlaceholder()
        {
            if (string.IsNullOrEmpty(MessageTextBox?.Text))
            {
                MessageTextBox.Text = PlaceholderText;
                MessageTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private bool IsPlaceholderActive()
        {
            return MessageTextBox?.Text == PlaceholderText;
        }
    }
}
