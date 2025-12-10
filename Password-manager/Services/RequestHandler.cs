using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using Password_manager.Entities;
using Password_manager.Shared;
using SQLite;

namespace Password_manager.Services
{
    public class RequestHandler
    {
        private readonly SqliteConnectionFactory _connectionFactory;
        private readonly EncryptionAndHashingMethods _tool;
        private readonly ILogger<RequestHandler> _logger;
        private readonly RestServiceHelper _restServiceHelper;
        public RequestHandler(SqliteConnectionFactory connectionFactory,
            ILogger<RequestHandler> logger, 
            RestServiceHelper restServiceHelper)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            _restServiceHelper = restServiceHelper;
            _tool = new EncryptionAndHashingMethods();
            
        }

        // Section for interacting with data stored in the vault
        public async Task<List<PasswordItem>> GetAccountSavedData()
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

                var dtos = await database.QueryAsync<ProgramDto>("SELECT * FROM Accounts WHERE UserId = ?", UserId);

                var result = dtos.Select(dto => new PasswordItem(
                    dto.Title,
                    dto.Username,
                    dto.Password,
                    dto.Category
                )).ToList();

                Debug.WriteLine($"inputs in the db {result.Count}");

                await Task.Run(() =>
                {
                    foreach (var item in result)
                    {
                        item.Password = _tool.Decrypt(item.Password, DEK);
                        Debug.WriteLine($"Title: {item.Title}, Username: {item.Username}, Category: {item.Category}");
                    }
                });

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to fetch data: " + ex);
                return new List<PasswordItem>();
            }
        }

        public async Task SaveDataToAccount(PasswordItem Item)
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
                string encryptedVaultPasswordInBase64 = await Task.Run(() => _tool.Encrypt(Item.Password, DEK));

                await database.ExecuteAsync("INSERT INTO Accounts (UserId, Title, Username, Password, Category) VALUES (?, ?, ?, ?, ?);",
                    UserId, Item.Title, Item.Username, encryptedVaultPasswordInBase64, Item.Category ?? "General");

            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to insert data into database: " + e);
            }
        }
        public async Task UpdateDataInAccount(PasswordItem oldItem, PasswordItem newItem)
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

                string encryptedVaultPasswordInBase64 = await Task.Run(() => _tool.Encrypt(newItem.Password, DEK));

                await database.ExecuteAsync(
                    "UPDATE Accounts SET Title = ?, Username = ?, Password = ?, Category = ? WHERE UserId = ? AND Title = ? AND Username = ?",
                    newItem.Title,
                    newItem.Username,
                    encryptedVaultPasswordInBase64,
                    newItem.Category ?? "General",
                    UserId,
                    oldItem.Title,
                    oldItem.Username
                );

            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to update data in database: " + e);
                throw;
            }
        }
        public async Task<List<string>> GetUserCategories()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            int UserId = Preferences.Get("CurrentUserId", -1);

            try
            {
                var categories = await database.QueryAsync<ProgramDto>(
                    "SELECT DISTINCT Category FROM Accounts WHERE UserId = ? ORDER BY Category",
                    UserId
                );

                var result = categories.Select(c => c.Category ?? "General").ToList();

                if (!result.Contains("General"))
                {
                    result.Insert(0, "General");
                }

                Debug.WriteLine($"Found categories: {string.Join(", ", result)}");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to fetch categories: " + ex);
                return new List<string> { "General" };
            }
        }

        public async Task<List<PasswordItem>> GetPasswordsByCategory(string category)
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

                var dtos = await database.QueryAsync<ProgramDto>(
                    "SELECT * FROM Accounts WHERE UserId = ? AND Category = ?",
                    UserId, category
                );

                var result = dtos.Select(dto => new PasswordItem(
                    dto.Title,
                    dto.Username,
                    dto.Password,
                    dto.Category ?? "General"
                )).ToList();

                await Task.Run(() =>
                {
                    foreach (var item in result)
                    {
                        item.Password = _tool.Decrypt(item.Password, DEK);
                    }
                });

                Debug.WriteLine($"Found {result.Count} passwords in category '{category}'");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to fetch data for category '{category}': " + ex);
                return new List<PasswordItem>();
            }
        }

        public async Task DeleteDataFromAccount(PasswordItem Item)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            long UserId = Preferences.Get("CurrentUserId", -1);
            try
            {
                if (UserId == -1)
                {
                    throw new Exception("Current user hasnt been found");
                }
                await database.ExecuteAsync("DELETE FROM Accounts WHERE Title = ? AND UserId = ?",
                    Item.Title, UserId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Deleting from database failed: " + ex);
            }
        }
        public async Task<bool> CheckUserAccount(string username, string password)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            try
            {
                var users = await database.QueryAsync<UserAccounts>("SELECT Username, Password FROM UserAccounts WHERE Username = ?", username);

                if (users == null || !users.Any())
                {
                    return false;
                }

                foreach (var user in users)
                {
                    if (user.Username == username && _tool.VerifyPassword(password, user.Password))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to query user account information" + ex);
                return false;
            }
        }

        public async Task RegisterNewUserAccount(string username, string password)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            // Generates temporary Key Encryption Key
            byte[] KEKSalt = _tool.GenerateKeys(16);
            byte[] tempKEK = _tool.HashString(password, KEKSalt);

            // Generates permanent Data Encryption Key
            byte[] DEK = _tool.GenerateKeys(32);
            string DEKInBase64 = Convert.ToBase64String(DEK);

            string encryptedDEKInBase64 = _tool.Encrypt(DEKInBase64, tempKEK);

            // Password hashing for storage
            string hashedPassword = _tool.HashString(password);

            string userIdentifier = Convert.ToBase64String(_tool.GenerateKeys(32));
            try
            {
                await database.ExecuteAsync("INSERT INTO UserAccounts (Username, Password, KEKSalt, EncryptedDek, UserIdentifier) VALUES (?, ?, ?, ?, ?)",
                    username, hashedPassword, Convert.ToBase64String(KEKSalt), encryptedDEKInBase64, userIdentifier);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to register new user: " + ex);
            }
        }

        public async Task<long> GetUserAccountId(string username)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            try
            {
                var users = await database.QueryAsync<UserAccounts>("SELECT Id FROM UserAccounts WHERE Username = ?", username);

                if (users == null || !users.Any())
                {
                    throw new InvalidOperationException("The specified user account was not found.");
                }

                return users[0].Id;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to fetch user account data: " + ex);
                throw;
            }
        }

        // Section for interacting with data related to notes

        public async Task<bool> CreateNote(string content)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            long userId = Preferences.Get("CurrentUserId", -1);
            try
            {
                byte[] DEK = await _restServiceHelper.RetrieveDEK();
                if(DEK == null)
                {
                    throw new InvalidOperationException("DEK is null");
                }

                string encryptedContent = await Task.Run(() => _tool.Encrypt(content, DEK));

                await database.ExecuteAsync("INSERT INTO Notes (UserId, Content, CreatedAt, LastUpdatedAt) VALUES (?, ?, ?, ?)", userId, encryptedContent, DateTime.UtcNow, DateTime.UtcNow);

                _logger.LogInformation("New Note has been succesfully created");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not create a new note: {ex}", ex);
                return false;
            }
        }

        public async Task DeleteNote(long Id)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            long userId = Preferences.Get("CurrentUserId", -1);
            try
            {
                byte[] DEK = await _restServiceHelper.RetrieveDEK();
                if (DEK == null)
                {
                    throw new InvalidOperationException("DEK is null");
                }
                await database.ExecuteAsync("DELETE FROM Notes WHERE Id = ? AND UserId = ?", Id, userId);

                _logger.LogInformation("Note has been succesfully deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not delete note: {ex}", ex);
            }
        }

        public async Task<List<NoteItem>> GetNotesByUser()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            long userId = Preferences.Get("CurrentUserId", -1);
            try
            {
                byte[] DEK = await _restServiceHelper.RetrieveDEK();
                if (DEK == null)
                {
                    throw new InvalidOperationException("DEK is null");
                }

                var notes = await database.QueryAsync<Notes>("SELECT * FROM Notes WHERE UserId = ?", userId);

                var noteItems = new List<NoteItem>();

                await Task.Run(() =>
                {
                    foreach (var note in notes)
                    {
                        note.Content = _tool.Decrypt(note.Content, DEK);

                        var item = new NoteItem
                        {
                            Id = note.Id,
                            UserId = note.UserId,
                            Content = note.Content,
                            CreatedAt = note.CreatedAt,
                            LastUpdatedAt = note.LastUpdatedAt,
                        };

                        noteItems.Add(item);
                    }
                });

                _logger.LogInformation("Notes have been succesfully retrieved");
                return noteItems;
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not retrieve notes: {ex}", ex);
                return [];
            }
        }

        public async Task<bool> UpdateNote(long id, string content)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            long userId = Preferences.Get("CurrentUserId", -1);
            try
            {
                byte[] DEK = await _restServiceHelper.RetrieveDEK();
                if (DEK == null)
                {
                    throw new InvalidOperationException("DEK is null");
                }
                var currentNotes = await database.QueryAsync<Notes>("SELECT * FROM Notes WHERE Id = ? AND UserId = ?", id, userId);

                if(currentNotes == null || !currentNotes.Any())
                {
                    throw new Exception("Note doesn't exist");
                }

                string storedContent = await Task.Run(() => _tool.Decrypt(currentNotes[0].Content, DEK));

                if(storedContent == content)
                {
                    await database.ExecuteAsync("UPDATE Notes SET LastUpdatedAt = ? WHERE Id = ? AND UserId = ?", DateTime.UtcNow, id, userId);
                }
                else if(storedContent != content)
                {
                    string encryptedNewContent = await Task.Run(() => _tool.Encrypt(content, DEK));
                    await database.ExecuteAsync("UPDATE Notes SET Content = ?, LastUpdatedAt = ? WHERE Id = ? AND UserId = ?", encryptedNewContent, DateTime.UtcNow, id, userId);
                }

                _logger.LogInformation("Note has been succesfully updated");
                return true;
                
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not update note: {ex}", ex);
                return false;
            }
        }
    }
}