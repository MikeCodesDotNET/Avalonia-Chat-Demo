using Bellow.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Bellow.Client.Services
{
    public interface IChatService
    {
        IObservable<HubConnectionState> ConnectionState { get; }

        IObservable<string> ParticipantLoggedIn { get; }

        IObservable<string> ParticipantLoggedOut { get; }

        Task<SuccessfulLoginResponse> LoginAsync(string username, string passcode);

        Task<SuccessfulLoginResponse> RegisterAndLogIn(string username, string passcode);

        Task LogoutAsync();

        Task ConnectAsync();

        IObservable<MessagePayload> MessageReceived { get; }

        Task SendMessageAsync(MessagePayload message);
    }
}
