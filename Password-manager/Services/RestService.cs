using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Password_manager.Entities;
using Password_manager.Shared;
using SQLite;


namespace Password_manager.Services
{
    public class RestService
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ILogger<RestService> _logger;
        private readonly RestServiceHelper _restServiceHelper;
        private readonly EncryptionAndHashingMethods _tool;
        private readonly RequestHandler _handler;
        private readonly SqliteConnectionFactory _connectionFactory;
        public RestService(ILogger<RestService> logger,
            RestServiceHelper restServiceHelper,
            RequestHandler handler,
            SqliteConnectionFactory connectionFactory,
            HttpClient client) 
        {
            _logger = logger;
            _client = client;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
            _restServiceHelper = restServiceHelper;
            _tool = new EncryptionAndHashingMethods();
            _handler = handler;
            _connectionFactory = connectionFactory;
        }

        private async Task SetAuthenticationToken()
        {
            string? token = await _restServiceHelper.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("TOKEN FOR AUTH: {token}", token);
                _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("Token set in authorization header");
            }
            else
            {
                _logger.LogWarning("Token wasn't set to authorization header");
            }
        }

        public async Task<bool> RegisterNewCloudAccount(LoginDTO registerCredentials)
        {
            Uri uri = new Uri(string.Format(Constants.RestUrl, "AccountsItems/register"));
            registerCredentials.UserIdentifier = await _restServiceHelper.GetUserIdentifier();

            try
            {
                string json = JsonSerializer.Serialize(registerCredentials, _serializerOptions);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(uri, content);

                var body = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Request to register new account on cloud has been succesfull: {StatusCode} - {content}",
                        response.StatusCode, body);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Request to register wasn't succesfull: {StatusCode} - {content}", response.StatusCode, body);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Request to register new account on cloud failed: {ex}", ex);
                return false;
            }
        }

        public async Task<bool> LoginToCloudAccount(LoginDTO loginCredentials)
        {
            Uri uri = new Uri(string.Format(Constants.RestUrl, "AccountsItems/login"));
            loginCredentials.UserIdentifier = await _restServiceHelper.GetUserIdentifier();

            try
            {
                string json = JsonSerializer.Serialize(loginCredentials, _serializerOptions);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(uri, content);

                var body = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Request to login new account on cloud has been succesfull: {StatusCode} - {content}",
                        response.StatusCode, body);

                    var res = JsonSerializer.Deserialize<JsonElement>(body, _serializerOptions);

                    long cloudId = res.GetProperty("userId").GetInt64();
                    string email = res.GetProperty("email").GetString();
                    string token = res.GetProperty("token").GetString();
                    bool isFirstBackup = res.GetProperty("isFirstbackup").GetBoolean();

                    Preferences.Set("IsFirstBackup", isFirstBackup);

                    await _restServiceHelper.SaveCloudData(cloudId, email, token, loginCredentials.Password);

                    await SetAuthenticationToken();

                    return true;
                }
                else
                {
                    _logger.LogWarning("Request to login wasn't succesfull: {StatusCode} - {content}", response.StatusCode, body);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Request to login new account on cloud failed: {ex}", ex);
                return false;
            }
        }

        public async Task<bool> RestoreVault()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            
            int userId = Preferences.Get("CurrentUserId", -1);

            await SetAuthenticationToken();
            try
            {
                string? userIdentifier = await _restServiceHelper.GetUserIdentifier();

                if (userIdentifier == null)
                {
                    throw new Exception("User identifier could not be found");
                }

                Uri uri = new Uri(string.Format(Constants.RestUrl, $"VaultBackups/{userIdentifier}"));
                
                HttpResponseMessage response = await _client.GetAsync(uri);

                var body = await response.Content.ReadAsStringAsync();
                var res = JsonSerializer.Deserialize<JsonElement>(body, _serializerOptions);
                if (response.IsSuccessStatusCode)
                {

                    string encryptedVaultBlob = res.GetProperty("encryptedvaultblob").GetString();

                    _logger.LogInformation("Works I GOT A BLOB IN MY APP: {blob}", encryptedVaultBlob);

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

                    string blob = await Task.Run(() => _tool.Decrypt(encryptedVaultBlob, DEK));

                    var blobObject = JsonSerializer.Deserialize<List<ProgramDto>>(blob);

                    await Upsert(blobObject, DEK);

                    _logger.LogInformation("Vault has been succesfully restored");

                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to Restore Vault: {StatusCode} - {body}", response.StatusCode, body);
                    return false;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to restore vault: {ex}", ex);
                return false;
            }
        }

        private async Task Upsert(List<ProgramDto> restoredVault, byte[] DEK)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            int userId = Preferences.Get("CurrentUserId", -1);
            try
            { 
                var existingVault = await database.Table<ProgramDto>().Where(item => item.UserId == userId).ToListAsync();

                if (existingVault == null)
                {
                    throw new Exception("Vault doesn't exist");
                }

                foreach (var item in existingVault)
                {
                    item.Password = await Task.Run(() => _tool.Decrypt(item.Password, DEK));
                }

                foreach (var restoredItem in restoredVault)
                {
                    restoredItem.UserId = userId;

                    var existingItem = existingVault.FirstOrDefault(item => item.Title == restoredItem.Title &&
                    item.Username == restoredItem.Username && item.Password == restoredItem.Password);

                    if (existingItem != null)
                    {
                        continue;
                    }

                    restoredItem.Id = 0;
                    restoredItem.Password = await Task.Run(() => _tool.Encrypt(restoredItem.Password, DEK));
                    await database.InsertAsync(restoredItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update values from restored blob: {ex}", ex);
                throw;
            }
        }

        public async Task<bool> BackupVault()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            int userId = Preferences.Get("CurrentUserId", -1);
            bool isFirstBackup = Preferences.Get("IsFirstBackup", false);

            await SetAuthenticationToken();
            if (isFirstBackup)
            {
                Uri uri = new Uri(string.Format(Constants.RestUrl, "VaultBackups"));
                try
                {
                    string? userIdentifier = await _restServiceHelper.GetUserIdentifier();

                    if (userIdentifier == null)
                    {
                        throw new Exception("User identifier could not be found");
                    }

                    var passwordVault = await _handler.GetAccountSavedData();

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

                    string PasswordVaultJson = JsonSerializer.Serialize(passwordVault);

                    string encriptedBlob = await Task.Run(() => _tool.Encrypt(PasswordVaultJson, DEK));

                    VaultBackupDTO transferObject = new VaultBackupDTO
                    { 
                        EncryptedVaultBlob = encriptedBlob,
                        VaultOwnerId = userIdentifier
                    };

                    string json = JsonSerializer.Serialize(transferObject, _serializerOptions);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _client.PostAsync(uri, content);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Vault has been succesfully backed up for the first time");
                        return true;
                    }

                    _logger.LogWarning("Failed to make a vault backup (first backup): {statusCode}", response.StatusCode);
                    return false;

                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to make a vault backup (first backup): {ex}", ex);
                    return false;
                }
            }
            else
            {
                try
                {
                    string? userIdentifier = await _restServiceHelper.GetUserIdentifier();

                    if(userIdentifier == null)
                    {
                        throw new Exception("User identifier could not be found");
                    }

                    Uri uri = new Uri(string.Format(Constants.RestUrl, $"VaultBackups/{userIdentifier}"));

                    var passwordVault = await _handler.GetAccountSavedData();

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

                    string PasswordVaultJson = JsonSerializer.Serialize(passwordVault);

                    string encriptedBlob = await Task.Run(() => _tool.Encrypt(PasswordVaultJson, DEK));

                    VaultBackupDTO transferObject = new VaultBackupDTO
                    {
                        EncryptedVaultBlob = encriptedBlob,
                    };

                    string json = JsonSerializer.Serialize(transferObject, _serializerOptions);
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _client.PutAsync(uri, content);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Vault has been succesfully backed up");
                        return true;
                    }

                    _logger.LogWarning("Failed to make a vault backup: {statusCode}", response.StatusCode);
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to make a vault backup: {ex}", ex);
                    return false;
                }
            }
        }

    }
}
