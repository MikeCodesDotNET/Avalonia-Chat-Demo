using Bellow.Shared.Models;

namespace Bellow.Client.Models
{
    public class ImageMessage : MessageBase
    {
        public ImageUrls Image { get; init; }

        public ImageMessage(ImageUrls image, string authorUsername)
        {
            AuthorUsername = authorUsername;
            Image = image;
        }

        internal ImageMessage(MessagePayload payload)
        {
            AuthorUsername = payload.AuthorUsername;
            Image = StringToObject<ImageUrls>(payload.Base64Payload);
        }

        internal override MessagePayload ToMessagePayload()
        {
            return new MessagePayload(ObjectToString(Image), Shared.MessageTypes.MessageType.Image);
        }

    }
}
