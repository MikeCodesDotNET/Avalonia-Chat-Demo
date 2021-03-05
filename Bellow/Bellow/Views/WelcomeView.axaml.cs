using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Avalonia.ReactiveUI;
using Bellow.Client.ViewModels;

namespace Bellow.Client.Views
{
    public class WelcomeView : ReactiveUserControl<WelcomeViewModel>
    {
        public WelcomeView()
        {
            InitializeComponent();

        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables => { /* Handle view activation etc. */ });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
