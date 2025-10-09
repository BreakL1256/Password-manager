using SQLite;
using Password_manager.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Password_manager.Entities
{
    public class RequestHandler
    {
        private readonly SqliteConnectionFactory _connectionFactory;
        public RequestHandler(SqliteConnectionFactory connectionFactory) 
        { 
            _connectionFactory = connectionFactory;
        }
        public async Task<List<PasswordItem>> GetAccountSavedData()
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
   
            try
            {
                var dtos = await database.QueryAsync<ProgramDto>("SELECT * FROM Accounts;");

                var result = dtos.Select(dto => new PasswordItem(
                    dto.Title,
                    dto.Username,
                    dto.Password
                )).ToList();

                Debug.WriteLine($"inputs in the db {result.Count}");

                foreach (var item in result)
                {
                    Debug.WriteLine($"Title: {item.Title}, Username: {item.Username}");
                }

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
            try
            { 
                await database.ExecuteAsync("INSERT INTO Accounts (Title, Username, Password) VALUES (?, ?, ?);", Item.Title, Item.Username, Item.Password);

            } catch (Exception e)
            {
                Debug.WriteLine("Failed to insert data into database" + e);
            }
        }

        public async Task DeleteDataFromAccount(PasswordItem Item)
        {
            ISQLiteAsyncConnection database = _connectionFactory.CreateConnection();
            try
            {
                await database.ExecuteAsync("DELETE FROM Accounts WHERE Title = ?", Item.Title);
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
                var users = await database.QueryAsync<UserAccounts>("SELECT Username, Password FROM UserAccounts AS ua WHERE ua.Username = ?", username);

                if(users == null || !users.Any())
                {
                    return false;
                }

                foreach (var user in users)
                {
                    if (user.Username == username && user.Password == password)
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

            try
            {
                await database.ExecuteAsync("INSERT INTO UserAccounts (Username, Password) VALUES (?, ?)", username, password);

            }catch(Exception ex)
            {
                Debug.WriteLine("Failed to register new user: " + ex);
            }
        }
    }
}
