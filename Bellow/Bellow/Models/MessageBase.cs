using Bellow.Shared.Models;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Bellow.Client.Models
{
    public abstract class MessageBase
    {
        public string AuthorUsername { get; init; }

        protected string ObjectToString(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        internal T StringToObject<T>(string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                var obj = new BinaryFormatter().Deserialize(ms);
                return (T)obj;
            }
        }

        public ChatRoleType Role { get; init; }

        public bool IsRead { get; set; }

        internal abstract MessagePayload ToMessagePayload();
    }
}
