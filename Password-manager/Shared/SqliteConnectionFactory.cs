using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_manager.Shared
{
    public class SqliteConnectionFactory
    {
        public SQLiteAsyncConnection CreateConnection()
        {
            return new SQLiteAsyncConnection(
                Path.Combine(FileSystem.AppDataDirectory, "Data-storage.db3"), 
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        }
    }
}
