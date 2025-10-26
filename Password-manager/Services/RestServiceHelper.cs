using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Password_manager.Shared;
using SQLite;
using Password_manager.Entities;

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

        public async Task SaveCloudData(long cloudId, string email, string token)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            int UserId = Preferences.Get("CurrentUserId", -1);
            try
            {
                var users = await database.QueryAsync<UserAccounts>("SELECT * FROM UserAccounts WHERE Id = ?", UserId);

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

                await database.ExecuteAsync(
                    "INSERT INTO UserAccounts (CloudLinked, CloudAccountId, CloudEmail, CloudTokenEncrypted, CloudTokenExpiry, LastCloudSync) VALUES (?, ?, ?, ?, ?, ?)",
                    true, cloudId, email, EncryptedJWTTokenInBase64);

            }
            catch (Exception ex)
            {
                _logger.LogError("Couldn't save cloud data to user account: {0}", ex);
            }
        }
    }
}
