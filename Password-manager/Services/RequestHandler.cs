using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            Task.Run(CleanupTrash);
        }


        public async Task RegisterNewUserAccount(string username, string password)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            try
            {

                var existing = await database.Table<UserAccounts>().Where(u => u.Username == username).FirstOrDefaultAsync();
                if (existing != null)
                {
                    Preferences.Set("CurrentUserId", existing.Id);
                    await SecureStorage.Default.SetAsync("CurrentPassword", password);
                    return;
                }

                byte[] kekSalt = _tool.GenerateKeys(16);

                byte[] KEK = _tool.HashString(password, kekSalt);

                byte[] DEK = _tool.GenerateKeys(32);

                string encryptedDEK = _tool.Encrypt(Convert.ToBase64String(DEK), KEK);

                var newUser = new UserAccounts
                {
                    Username = username,
                    KEKSalt = Convert.ToBase64String(kekSalt),
                    EncryptedDEK = encryptedDEK
                };

                await database.InsertAsync(newUser);

                var savedUser = await database.Table<UserAccounts>().Where(u => u.Username == username).FirstOrDefaultAsync();
                if (savedUser != null)
                {
                    Preferences.Set("CurrentUserId", savedUser.Id);
                    await SecureStorage.Default.SetAsync("CurrentPassword", password);
                    Debug.WriteLine($"✅ Local User Created: {username} (ID: {savedUser.Id})");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Register Failed: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CheckUserAccount(string username, string password)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            try
            {
                var user = await database.Table<UserAccounts>().Where(u => u.Username == username).FirstOrDefaultAsync();
                if (user == null) return false;

                byte[] kekSalt = Convert.FromBase64String(user.KEKSalt);
                byte[] KEK = _tool.HashString(password, kekSalt);

                try
                {
                    string dekStr = _tool.Decrypt(user.EncryptedDEK, KEK);

                    Preferences.Set("CurrentUserId", user.Id);
                    await SecureStorage.Default.SetAsync("CurrentPassword", password);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login Check Failed: {ex}");
                return false;
            }
        }

        public async Task<long> GetUserAccountId(string username)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            var user = await database.Table<UserAccounts>().Where(u => u.Username == username).FirstOrDefaultAsync();
            return user?.Id ?? 0;
        }

        private async Task<byte[]> GetLocalDekAsync(ISQLiteAsyncConnection database)
        {
            int userId = Preferences.Get("CurrentUserId", -1);
            if (userId == -1) throw new Exception("No user logged in.");

            var user = await database.Table<UserAccounts>().Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) throw new Exception("User record not found.");

            string? userPassword = await SecureStorage.Default.GetAsync("CurrentPassword");
            if (string.IsNullOrEmpty(userPassword)) throw new Exception("Session expired.");

            byte[] kekSalt = Convert.FromBase64String(user.KEKSalt);
            byte[] KEK = await Task.Run(() => _tool.HashString(userPassword, kekSalt));

            string dekBase64 = await Task.Run(() => _tool.Decrypt(user.EncryptedDEK, KEK));
            return Convert.FromBase64String(dekBase64);
        }

        public async Task<List<PasswordItem>> GetAccountSavedData(bool includeDeleted = false)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            int userId = Preferences.Get("CurrentUserId", -1);

            try
            {
                byte[] DEK = await GetLocalDekAsync(database);

                var dtos = await database.QueryAsync<ProgramDto>(
                    "SELECT * FROM Accounts WHERE UserId = ? AND IsDeleted = ?", userId, includeDeleted);

                var result = dtos.Select(dto => new PasswordItem(
                    dto.Title, dto.Username, dto.Password, dto.Category
                )
                {
                    Id = dto.Id,
                    IsDeleted = dto.IsDeleted,
                    DeletedAt = dto.DeletedAt
                }).ToList();

                await Task.Run(() =>
                {
                    foreach (var item in result)
                    {
                        try { item.Password = _tool.Decrypt(item.Password, DEK); }
                        catch { item.Password = "Error Decrypting"; }
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
            int userId = Preferences.Get("CurrentUserId", -1);
            try
            {
                byte[] DEK = await GetLocalDekAsync(database);
                string encryptedPass = await Task.Run(() => _tool.Encrypt(Item.Password, DEK));

                await database.ExecuteAsync(
                    "INSERT INTO Accounts (UserId, Title, Username, Password, Category, IsDeleted) VALUES (?, ?, ?, ?, ?, ?);",
                    userId, Item.Title, Item.Username, encryptedPass, Item.Category ?? "General", false);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to insert data: " + e);
            }
        }

        public async Task UpdateDataInAccount(PasswordItem oldItem, PasswordItem newItem)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            int userId = Preferences.Get("CurrentUserId", -1);
            try
            {
                byte[] DEK = await GetLocalDekAsync(database);
                string encryptedPass = await Task.Run(() => _tool.Encrypt(newItem.Password, DEK));

                if (oldItem.Id > 0)
                {
                    await database.ExecuteAsync(
                        "UPDATE Accounts SET Title = ?, Username = ?, Password = ?, Category = ? WHERE Id = ?",
                        newItem.Title, newItem.Username, encryptedPass, newItem.Category ?? "General", oldItem.Id);
                }
                else
                {
                    await database.ExecuteAsync(
                        "UPDATE Accounts SET Title = ?, Username = ?, Password = ?, Category = ? WHERE UserId = ? AND Title = ? AND Username = ?",
                        newItem.Title, newItem.Username, encryptedPass, newItem.Category ?? "General", userId, oldItem.Title, oldItem.Username);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to update: " + e);
                throw;
            }
        }

        public async Task DeleteDataFromAccount(PasswordItem Item)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            try
            {
                if (Item.IsDeleted)
                {
                    await database.ExecuteAsync("DELETE FROM Accounts WHERE Id = ?", Item.Id);
                }
                else
                {
                    await database.ExecuteAsync("UPDATE Accounts SET IsDeleted = 1, DeletedAt = ? WHERE Id = ?",
                        DateTime.UtcNow, Item.Id);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Delete failed: " + ex);
            }
        }

        public async Task RestorePassword(PasswordItem Item)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            try
            {
                await database.ExecuteAsync("UPDATE Accounts SET IsDeleted = 0, DeletedAt = NULL WHERE Id = ?", Item.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Restore failed: " + ex);
            }
        }

        public async Task<List<NoteItem>> GetNotesByUser(bool includeDeleted = false)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            long userId = Preferences.Get("CurrentUserId", -1);

            try
            {
                byte[] DEK = await GetLocalDekAsync(database);

                var notes = await database.QueryAsync<Notes>(
                    "SELECT * FROM Notes WHERE UserId = ? AND IsDeleted = ?", userId, includeDeleted);

                var noteItems = new List<NoteItem>();

                await Task.Run(() =>
                {
                    foreach (var note in notes)
                    {
                        try
                        {
                            string clearContent = _tool.Decrypt(note.Content, DEK);
                            noteItems.Add(new NoteItem
                            {
                                Id = note.Id,
                                UserId = note.UserId,
                                Content = clearContent,
                                CreatedAt = note.CreatedAt,
                                LastUpdatedAt = note.LastUpdatedAt,
                                IsDeleted = note.IsDeleted,
                                DeletedAt = note.DeletedAt // Populate DeletedAt from DB
                            });
                        }
                        catch { /* Ignore decrypt errors on notes */ }
                    }
                });
                return noteItems;
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not retrieve notes: {ex}", ex);
                return new List<NoteItem>();
            }
        }

        public async Task<bool> CreateNote(string content)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            int userId = Preferences.Get("CurrentUserId", -1);
            try
            {
                byte[] DEK = await GetLocalDekAsync(database);
                string encryptedContent = _tool.Encrypt(content, DEK);

                var note = new Notes
                {
                    UserId = userId,
                    Content = encryptedContent,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await database.InsertAsync(note);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Create Note Failed: {ex}");
                return false;
            }
        }

        public async Task<bool> UpdateNote(long id, string content)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            try
            {
                byte[] DEK = await GetLocalDekAsync(database);
                string encryptedContent = _tool.Encrypt(content, DEK);

                await database.ExecuteAsync("UPDATE Notes SET Content = ?, LastUpdatedAt = ? WHERE Id = ?",
                    encryptedContent, DateTime.UtcNow, id);
                return true;
            }
            catch { return false; }
        }

        public async Task DeleteNote(long Id, bool isAlreadyDeleted)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            try
            {
                if (isAlreadyDeleted)
                {
                    await database.ExecuteAsync("DELETE FROM Notes WHERE Id = ?", Id);
                }
                else
                {
                    await database.ExecuteAsync("UPDATE Notes SET IsDeleted = 1, DeletedAt = ? WHERE Id = ?",
                        DateTime.UtcNow, Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not delete note: {ex}", ex);
            }
        }

        public async Task RestoreNote(long Id)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            try
            {
                await database.ExecuteAsync("UPDATE Notes SET IsDeleted = 0, DeletedAt = NULL WHERE Id = ?", Id);
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not restore note: {ex}", ex);
            }
        }

        public async Task CleanupTrash()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            try
            {
                DateTime cutoff = DateTime.UtcNow.AddDays(-5);

                await database.ExecuteAsync("DELETE FROM Accounts WHERE IsDeleted = 1 AND DeletedAt < ?", cutoff);
                await database.ExecuteAsync("DELETE FROM Notes WHERE IsDeleted = 1 AND DeletedAt < ?", cutoff);

                Debug.WriteLine("Trash Cleanup Completed.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Trash cleanup failed: " + ex);
            }
        }

        public async Task<List<string>> GetUserCategories()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            int userId = Preferences.Get("CurrentUserId", -1);

            var categories = await database.QueryAsync<ProgramDto>(
                 "SELECT DISTINCT Category FROM Accounts WHERE UserId = ? AND IsDeleted = 0 ORDER BY Category", userId);

            var result = categories.Select(c => c.Category ?? "General").ToList();
            if (!result.Contains("General")) result.Insert(0, "General");

            return result;
        }
    }
}