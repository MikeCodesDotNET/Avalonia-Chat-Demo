using Bellow.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bellow.Server.Services
{
    /// <summary>
    /// 
    ///                                         !HERE BE DRAGONS! 
    ///                                           !PLEASE READ!
    /// 
    /// This is DEMO CODE.
    /// Do NOT use this in production. In fact, just avoid using it anywhere. 
    /// It's DIRTY. Dirtier than the communal shower of my local rugby team after the players have washed off after a game on a wet winters day.
    /// 
    /// This squalid, festering collection of characters just so happens to be accepted by the C# compiler, but you should not accept it.     
    /// It should repulse you. You're better than the compiler; you're meant to have a sense of taste. 
    /// You should understand that code isn't a binary of valid/invalid, but instead can be 'valid' but entirely 
    /// wrong at the same time. 
    /// A good friend once said to me, "Mike, you're not wrong, but that doesn't make you right". 
    /// This is the code equivalent to that. It's valid, it will compile, but it's wrong in so many ways.
    /// Anyone with experience assessing code quality should feel both nauseous and an impending sense of doom 
    /// when studying this rancid mess. 
    /// 
    /// This code wasn't written to be secure or production-ready. 
    /// It was written to fake a critical piece of a system to allow me to focus on the main demo. I decided to forgo using a real auth provider because
    /// I didn't want to read and digest the necessary documentation to consume a real service. 
    /// In a caffeine-induced state of delirium, creating this horror seems like the logical decision. 
    /// Don't adopt this dragon thinking it can be tamed. It'll always been a beast and you're going to get burnt. 

    /// So don't say you've not been warned. 
    /// 
    /// TDLR: Don't copy this. It's not demonstrating anything other than stupidity.
    /// </summary>

    public class UserService
    {
        ChatContext chatData;

        public UserService(ChatContext chatContext)
        {
            chatData = chatContext;

            //throw new Exception("You appear to have ignored the warning not to use this. Are you ok?!");

            var signalRKey = "";
            ParseConnectionString(signalRKey, out endpoint, out accessKey);
        }


        public async Task<(User, AccessToken)> CreateUser(HubCallerContext context, string username, string passcode)
        {
            if ((string.IsNullOrEmpty(username)) || string.IsNullOrEmpty(passcode))
                throw new ArgumentNullException();


            if (chatData.Users.SingleOrDefault(u => u.UserName == username) != null)
                throw new Exception("Username already used");


            if (passcode.Length < 4)
                throw new Exception("Passcode too short. Must be 4 digits");


            var salt = GetFreshSalt();

            var userId = Guid.NewGuid().ToString();
            var passCode = new PassCode(userId, GenerateSaltedHash(passcode, salt), salt);
            var user = new User(userId, username, passCode);

            var activeConnection = new Connection() { Id = context.ConnectionId };
            user.ActiveConnections.Add(activeConnection);

            await chatData.Users.AddAsync(user);

            var accessToken = new AccessToken(userId, GenerateAccessToken());
            await chatData.Tokens.AddAsync(accessToken);

            await chatData.SaveChangesAsync();
            return (user, accessToken);
        }


        public async Task<(User, AccessToken)> TryPassCodeValidation(HubCallerContext context, string username, string passcode)
        {
            if ((string.IsNullOrEmpty(username)) || string.IsNullOrEmpty(passcode))
                throw new ArgumentNullException();

            var user = chatData.Users.SingleOrDefault(u => u.UserName == username);
            if (user == null)
                throw new Exception($"User {username} not found");

            var userId = user.Id;

            var activeConnection = new Connection() { Id = context.ConnectionId };
            user.ActiveConnections.Add(activeConnection);

            var passCode = chatData.PassCodes.SingleOrDefault(p => p.OwnerId == userId);
            if (passCode == null)
                throw new Exception($"User {username} not found");

            var inputPassCodeHash = GenerateSaltedHash(passcode, passCode.Salt);
            if (CompareByteArrays(inputPassCodeHash, passCode.SaltedPassCode))
            {
                var accessToken = new AccessToken(userId, GenerateAccessToken());
                await chatData.Tokens.AddAsync(accessToken);
                await chatData.SaveChangesAsync();
                return (user, accessToken);
            }
            else
            {
                throw new Exception("Invalid passcode");
            }
        }


        public async Task<User> SignOutCurrentUser(HubCallerContext context)
        {
            var user = GetUser(context);
            var connection = user.ActiveConnections.SingleOrDefault(c => c.Id == context.ConnectionId);
            if (connection != null)
            {
                user.ActiveConnections.Remove(connection);
                await chatData.SaveChangesAsync();
                return user;
            }

            return null;
        }


        public User GetUser(string username)
        {
            var user = chatData.Users.SingleOrDefault((c) => c.UserName == username);
            if (user == null)
                throw new NullReferenceException();

            return user;
        }

        public User GetUser(HubCallerContext context)
        {
            var user = chatData.Users.Include(c => c.ActiveConnections).SingleOrDefault((c) => c.ActiveConnections.FirstOrDefault(x => x.Id == context.ConnectionId) != null);

            if (user == null)
                throw new NullReferenceException();

            return user;
        }


        private string GenerateAccessToken(DateTime? expiration = null)
        {
            if (expiration.HasValue)
                return GenerateJwtBearer(null, GetClientHubUrl("chat"), null, expiration, accessKey);
            else
                return GenerateJwtBearer(null, GetClientHubUrl("chat"), null, DateTime.UtcNow.AddMinutes(30), accessKey);

        }


        private string GenerateJwtBearer(string issuer, string audience, ClaimsIdentity subject, DateTime? expires, string signingKey)
        {
            SigningCredentials credentials = null;
            if (!string.IsNullOrEmpty(signingKey))
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
                credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            }

            var token = jwtTokenHandler.CreateJwtSecurityToken(
                issuer: issuer,
                audience: audience,
                subject: subject,
                expires: expires,
                signingCredentials: credentials);
            return jwtTokenHandler.WriteToken(token);
        }


        private void ParseConnectionString(string connectionString, out string endpoint, out string accessKey)
        {
            var dict = connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Split(new[] { '=' }, 2)).ToDictionary(p => p[0].Trim().ToLower(), p => p[1].Trim());
            if (!dict.TryGetValue("endpoint", out endpoint)) throw new ArgumentException("Invalid connection string, missing endpoint.");
            if (!dict.TryGetValue("accesskey", out accessKey)) throw new ArgumentException("Invalid connection string, missing access key.");
        }


        public string GetClientHubUrl(string hubName)
        {
            return $"{endpoint}/client/?hub={hubName}";
        }


        private byte[] GenerateSaltedHash(string passcode, byte[] salt)
        {
            var data = Convert.FromBase64String(Base64Encode(passcode));
            HashAlgorithm algorithm = new SHA256Managed();

            byte[] plainTextWithSaltBytes = new byte[data.Length + salt.Length];

            for (int i = 0; i < data.Length; i++)
            {
                plainTextWithSaltBytes[i] = data[i];
            }
            for (int i = 0; i < salt.Length; i++)
            {
                plainTextWithSaltBytes[data.Length + i] = salt[i];
            }

            return algorithm.ComputeHash(plainTextWithSaltBytes);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private byte[] GetFreshSalt()
        {
            var stringInput = RandomString(60);
            return Convert.FromBase64String(stringInput);
        }

        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }


        private readonly string endpoint;
        private readonly string accessKey;
        private readonly JwtSecurityTokenHandler jwtTokenHandler = new JwtSecurityTokenHandler();
        private Random random = new Random();

    }

}
