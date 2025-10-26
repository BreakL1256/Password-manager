using SQLite;
using Password_manager.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Maui.Storage;
using Password_manager.Entities;

namespace Password_manager.Services
{
    public class RequestHandler
    {
        private readonly SqliteConnectionFactory _connectionFactory;
        private readonly EncryptionAndHashingMethods _tool;
        public RequestHandler(SqliteConnectionFactory connectionFactory) 
        { 
            _connectionFactory = connectionFactory;
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
                    dto.Password
                )).ToList();

                Debug.WriteLine($"inputs in the db {result.Count}");

                await Task.Run(() =>
                {
                    foreach (var item in result)
                    {
                        item.Password = _tool.Decrypt(item.Password, DEK);
                        Debug.WriteLine($"Title: {item.Title}, Username: {item.Username}");
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

                if(users == null || !users.Any())
                {
                    throw new Exception("Current user couldn't be found");
                }

                string? userPassword = await SecureStorage.Default.GetAsync("CurrentPassword");

                byte[] KEKSalt = Convert.FromBase64String(users[0].KEKSalt);
                byte[] KEK = await Task.Run(() => _tool.HashString(userPassword, KEKSalt));
                string DEKInBase64 = await Task.Run(() => _tool.Decrypt(users[0].EncryptedDEK, KEK));
                byte[] DEK = Convert.FromBase64String(DEKInBase64);
                string encryptedVaultPasswordInBase64 = await Task.Run(() => _tool.Encrypt(Item.Password, DEK));

                await database.ExecuteAsync("INSERT INTO Accounts (UserId, Title, Username, Password) VALUES (?, ?, ?, ?);", 
                    UserId, Item.Title, Item.Username, encryptedVaultPasswordInBase64);    

            } catch (Exception e)
            {
                Debug.WriteLine("Failed to insert data into database: " + e);
            }
        }

        public async Task DeleteDataFromAccount(PasswordItem Item)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            int UserId = Preferences.Get("CurrentUserId", -1);
            try
            {   
                if(UserId == -1)
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

        // Section for interacting with program user accounts
        public async Task<bool> CheckUserAccount(string username, string password)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();

            try
            {
                var users = await database.QueryAsync<UserAccounts>("SELECT Username, Password FROM UserAccounts WHERE Username = ?", username);

                if(users == null || !users.Any())
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

            try
            {
                await database.ExecuteAsync("INSERT INTO UserAccounts (Username, Password, KEKSalt, EncryptedDek) VALUES (?, ?, ?, ?)", 
                    username, hashedPassword, Convert.ToBase64String(KEKSalt), encryptedDEKInBase64);

            }catch(Exception ex)
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

                if(users == null || !users.Any())
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
    }
}
