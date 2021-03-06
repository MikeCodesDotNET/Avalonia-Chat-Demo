using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bellow.Client.Services;
using Bellow.Shared.Models;
using ReactiveUI;

namespace Bellow.Client.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IScreen
    {
        // The Router associated with this Screen.
        // Required by the IScreen interface.
        public RoutingState Router { get; }
              
   
        public MainWindowViewModel()
        {
            
            Router = new RoutingState();
            Router.Navigate.Execute(new MainViewModel(Router));
        }

        private ChatService chatService;
    }
}
