using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Bellow.Client.ViewModels;
using ReactiveUI;

namespace Bellow.Client.Views
{
    public class MainView : ReactiveUserControl<MainViewModel>
    {
        public MainView()
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
