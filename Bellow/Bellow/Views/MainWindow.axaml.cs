using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Bellow.Client.ViewModels;
using ReactiveUI;

namespace Bellow.Client.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                //NO OP (right now)
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
