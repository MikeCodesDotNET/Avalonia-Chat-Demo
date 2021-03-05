using Bellow.Shared.Models;

namespace Bellow.Client.Models
{
    public class TextMessage : MessageBase
    {
        public string Content { get; init; }

        public TextMessage(string content, string authorUsername)
        {
            AuthorUsername = authorUsername;
            Content = content;
        }



        internal override MessagePayload ToMessagePayload()
        {
            return new MessagePayload(ObjectToString(Content), Shared.MessageTypes.MessageType.Text) { AuthorUsername = AuthorUsername };
        }

        internal TextMessage(MessagePayload payload)
        {
            AuthorUsername = payload.AuthorUsername;
            Content = StringToObject<string>(payload.Base64Payload);
        }
             
    }
}
