using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Password_manager.Entities;
using Password_manager.Shared;
using SQLite;

namespace Password_manager.Services
{
    public class RestServiceHelper
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ILogger<RestServiceHelper> _logger;
        private readonly SqliteConnectionFactory _connectionFactory;
        private readonly EncryptionAndHashingMethods _tool;

        public RestServiceHelper(ILogger<RestServiceHelper> logger, SqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            _client = new HttpClient();
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            _tool = new EncryptionAndHashingMethods();
        }

        public async Task SaveCloudData(long cloudId, string email, string token, string password)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            int userId = Preferences.Get("CurrentUserId", -1);
            try
            {
                var users = await database.QueryAsync<UserAccounts>("SELECT * FROM UserAccounts WHERE Id = ?", userId);

                if (users == null || !users.Any())
                {
                    throw new Exception("Current user couldn't be found");
                }

                string? userPassword = await SecureStorage.Default.GetAsync("CurrentPassword");

                byte[] KEKSalt = Convert.FromBase64String(users[0].KEKSalt);
                byte[] KEK = await Task.Run(() => _tool.HashString(userPassword, KEKSalt));
                string DEKInBase64 = await Task.Run(() => _tool.Decrypt(users[0].EncryptedDEK, KEK));
                byte[] DEK = Convert.FromBase64String(DEKInBase64);
                string EncryptedJWTTokenInBase64 = await Task.Run(() => _tool.Encrypt(token, DEK));

                string encryptedCloudPassword = await Task.Run(() => _tool.Encrypt(password, DEK));

                DateTime? tokenExpTime = GetExpiryUsingHandler(token);

                if(tokenExpTime is null)
                {
                    throw new Exception("Couldn't parse expiration time from token");
                }

                await database.ExecuteAsync(
                    "UPDATE UserAccounts SET CloudLinked = ?, CloudAccountId = ?, CloudEmail = ?, CloudPassword = ?, CloudTokenEncrypted = ?, CloudTokenExpiry = ?, LastCloudSync = ? WHERE Id = ?",
                    true, cloudId, email, encryptedCloudPassword, EncryptedJWTTokenInBase64, tokenExpTime, DateTime.UtcNow, userId);

                _logger.LogInformation("Cloud credentials have been successfully added");

            }
            catch (Exception ex)
            {
                _logger.LogError("Couldn't save cloud data to user account: {ex}", ex);
            }
        }

        public DateTime? GetExpiryUsingHandler(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
            if (expClaim == null)
                return null;

            if (long.TryParse(expClaim.Value, out long expUnix))
            {
                return DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            }

            return null;
        }

        public async Task DeleteCloudData()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            int userId = Preferences.Get("CurrentUserId", -1);
            try
            {
                await database.ExecuteAsync("UPDATE UserAccounts SET CloudLinked = ?, CloudAccountId = ?, CloudEmail = ?, CloudTokenEncrypted = ?, CloudTokenExpiry = ?, LastCloudSync = ? WHERE Id = ?",
                    false, null, null, null, null, null);

                _logger.LogInformation("Cloud credentials data has been successfully deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to delete cloud data: {ex}", ex);
            }
        }

        public async Task<string?> GetToken()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            var userId = Preferences.Get("CurrentUserId", -1);
            try
            {
                var users = await database.QueryAsync<UserAccounts>("SELECT * FROM UserAccounts WHERE Id = ?", userId);
            
                if(users == null || !users.Any())
                {
                    throw new Exception(string.Format("Couldn't find user: {0}", userId));
                }

                string? userPassword = await SecureStorage.Default.GetAsync("CurrentPassword");
                string? EncryptedJWTTokenInBase64 = users[0].CloudTokenEncrypted;

                if(EncryptedJWTTokenInBase64 == null)
                {
                    throw new Exception("Token hasn't been set");
                }

                byte[] KEKSalt = Convert.FromBase64String(users[0].KEKSalt);
                byte[] KEK = await Task.Run(() => _tool.HashString(userPassword, KEKSalt));
                string DEKInBase64 = await Task.Run(() => _tool.Decrypt(users[0].EncryptedDEK, KEK));
                byte[] DEK = Convert.FromBase64String(DEKInBase64);
                string token = await Task.Run(() => _tool.Decrypt(EncryptedJWTTokenInBase64, DEK));

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to fetch token from database: {ex}", ex);
                return null;
            }
        }

        public async Task<string?> GetUserIdentifier()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            long userId = Preferences.Get("CurrentUserId", -1);
            try
            {
                var userIdentifier = await database.ExecuteScalarAsync<string>("SELECT UserIdentifier FROM UserAccounts WHERE Id = ?", userId);

                if (userIdentifier == null)
                {
                    throw new Exception("userIdentifier is null");
                }

                return userIdentifier;
            }
            catch (Exception ex)
            {
                _logger.LogError("Filed to fetch user Identifier: {ex}", ex);
                return null;
            }
        }

        public async Task<bool> IsCloudLinked()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            var userId = Preferences.Get("CurrentUserId", -1);
            try
            {
                bool cloudLinked = await database.ExecuteScalarAsync<bool>("SELECT CloudLinked FROM UserAccounts WHERE Id = ?", userId);

                if(cloudLinked == null)
                {
                    throw new Exception("Cloud linked value is null");
                }

                return cloudLinked;
            } 
            catch(Exception ex)
            {
                _logger.LogError("Can't retrieve data on cloud information from database: {ex}", ex);
                return false;
            }
        } 

        public async Task<LoginDTO?> GetCloudCredentials()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            var userId = Preferences.Get("CurrentUserId", -1);
            try
            {
                var users = await database.QueryAsync<UserAccounts>("SELECT * FROM UserAccounts WHERE Id = ?", userId);

                if (users == null || !users.Any())
                {
                    throw new Exception("User couldn't be found");
                }

                string? userPassword = await SecureStorage.Default.GetAsync("CurrentPassword");

                byte[] KEKSalt = Convert.FromBase64String(users[0].KEKSalt);
                byte[] KEK = await Task.Run(() => _tool.HashString(userPassword, KEKSalt));
                string DEKInBase64 = await Task.Run(() => _tool.Decrypt(users[0].EncryptedDEK, KEK));
                byte[] DEK = Convert.FromBase64String(DEKInBase64);

                string password = await Task.Run(() => _tool.Decrypt(users[0].CloudPassword, DEK));
                var userIdentifier = await GetUserIdentifier();

                if(userIdentifier == null)
                {
                    throw new Exception("User identifier couldn't be retrieved");
                }

                LoginDTO trans = new LoginDTO()
                {
                    Email = users[0].CloudEmail,
                    Password = password,
                    UserIdentifier = userIdentifier
                };

                _logger.LogInformation("Cloud credentials have been succesfully retrieved");
                return trans;
            }
            catch (Exception ex)
            {
                _logger.LogError("Can't retrieve cloud credentials: {ex}", ex);
                return null;
            }
        }
    }
}
