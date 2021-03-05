using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Bellow.Client.ViewModels;

namespace Bellow.Client.Views
{
    public class ChatView : ReactiveUserControl<ChatViewModel>
    {
        public ChatView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
