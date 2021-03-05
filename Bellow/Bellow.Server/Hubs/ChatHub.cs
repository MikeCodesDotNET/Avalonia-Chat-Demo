using Bellow.Server.Services;
using Bellow.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace Bellow.Server.Hubs
{
    public class ChatHub : Hub
    {
        ChatContext chatContext;
        UserService userService;

        public ChatHub(ChatContext chatContext, UserService userService)
        {
            this.chatContext = chatContext;
            this.userService = userService;
        }

        #region Connection & Disconnection Overrides 

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var user = GetCurrentUser();

            if (user != null)
            {
                Clients.Others.SendAsync("UserLoggedOut", user.UserName);
                chatContext.Users.Remove(GetCurrentUser());
                chatContext.SaveChanges();

                Console.WriteLine($"{user.UserName} disconnected");
            }
            return base.OnDisconnectedAsync(exception);
        }
        
        public override Task OnConnectedAsync()
        {
            var userName = GetCurrentUser()?.UserName;
            if (userName != null)
            {
                //Clients.Others.SendAsync("ParticipantReconnection", userName);
                Console.WriteLine($"== {userName} reconnected");
            }
            return base.OnConnectedAsync();
        }

        #endregion


        #region LogIn & Logout 

        public async Task<SuccessfulLoginResponse> LogIn(string username, string passcode)
        {
            try
            {
                var result = await userService.TryPassCodeValidation(Context, username, passcode);
                await Clients.Others.SendAsync("UserLoggedIn", username);
                return new SuccessfulLoginResponse() { User = result.Item1, AccessToken = result.Item2 };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<SuccessfulLoginResponse> RegisterAndLogIn(string username, string passcode)
        {
            try
            {
                var result = await userService.CreateUser(Context, username, passcode);
                await Clients.Others.SendAsync("UserLoggedIn", username);
                return new SuccessfulLoginResponse() { User = result.Item1, AccessToken = result.Item2 };
            }
            catch (Exception ex)
            {
                //do something with it! 
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<Unit> LogOut()
        {
            try
            {
                var user = await userService.SignOutCurrentUser(Context);
                await Clients.Others.SendAsync("UserLoggedOut", user.UserName);
                return Unit.Default;
            }
            catch (Exception ex)
            {
                //do something with it! 
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        #endregion


        public void SendMessage(MessagePayload message)
        {           
            if (message.Base64Payload != null)
            {                
                Clients.All.SendAsync("NewMessage", message);
            }
        }


        User GetCurrentUser()
        {
            return chatContext.Users.Include(c => c.ActiveConnections).SingleOrDefault((c) => c.ActiveConnections.FirstOrDefault(x => x.Id == Context.ConnectionId) != null);
        }

        User GetUser(string userName)
        {
            return chatContext.Users.SingleOrDefault((c) => c.UserName == userName);
        }
    }
}
