using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Bellow.Client.Services;
using Bellow.Shared.MessageTypes;
using Bellow.Shared.Models;
using Bellow.Client.Models;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using System.Reactive.Linq;

namespace Bellow.Client.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {        
        public ObservableCollection<MessageBase> Messages { get; private set; }

        public string NewMessageContent
        {
            get => newMessageContent;
            set => this.RaiseAndSetIfChanged(ref newMessageContent, value);
        }
        
        public  ICommand DictateMessageCommand { get; private set; }

        public ICommand AttachImageCommand { get; private set; }

        public ICommand SendMessageCommand { get; private set; }

        public ChatViewModel(ChatService chatService, RoutingState router) : base(router)
        {
            this.Messages = new ObservableCollection<MessageBase>();
            this.chatService = chatService;
            this.chatService.Messages.CollectionChanged += (sender, args) =>
            {                
                foreach (MessagePayload newMsg in args.NewItems)
                {
                    ChatRoleType role = ChatRoleType.Receiver;
                    if (newMsg.AuthorUsername == chatService.CurrentUser.UserName)
                        role = ChatRoleType.Sender;

                    switch (newMsg.Type)
                    {
                        case MessageType.Text:
                            Messages.Add(new TextMessage(newMsg) { Role = role });
                            break;
                        case MessageType.Link:
                            Messages.Add(new LinkMessage(newMsg) { Role = role });
                            break;
                        case MessageType.Image:
                            Messages.Add(new ImageMessage(newMsg) { Role = role });
                            break;
                    }
                }
            };

            this.chatService.ParticipantLoggedIn.Subscribe(x => { Messages.Add(new UserConnectedMessage(x)); });
            this.chatService.ParticipantLoggedOut.Subscribe(x => { Messages.Add(new UserDisconnectedMessage(x)); });

            canSendMessage = this.WhenAnyValue(x => x.NewMessageContent).Select(x => !string.IsNullOrEmpty(x));

            SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessage, canSendMessage);
            AttachImageCommand = ReactiveCommand.CreateFromTask(AttachImage);
            DictateMessageCommand = ReactiveCommand.CreateFromTask(DictateMessage);
        }

        async Task SendMessage()
        {
            await chatService.SendMessageAsync(new TextMessage(newMessageContent, chatService.CurrentUser.UserName).ToMessagePayload());
            NewMessageContent = string.Empty;
        }

        async Task AttachImage()
        {

        }

        async Task DictateMessage()
        {
        }


        //Fields
        private ChatService chatService;
        private string newMessageContent;
        private WindowNotificationManager windowNotificationManager;
        private IObservable<bool> canSendMessage;
    }
}
