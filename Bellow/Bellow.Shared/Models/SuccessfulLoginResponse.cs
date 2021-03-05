using System.Collections.Generic;

namespace Bellow.Shared.Models
{
    public class SuccessfulLoginResponse
    {
        public User User { get; set; }

        public List<MessagePayload> PreviousMessages { get; set; }

        public List<User> Users { get; set; }

        public AccessToken AccessToken { get; init; }
    }
}
