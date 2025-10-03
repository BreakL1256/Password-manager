using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_manager.Entities
{
    class AdditionalClasses
    {
    }
    public class PasswordItem
    {
        public PasswordItem(string Title, string Username, string Password)
        {
            this.Title = Title;
            this.Username = Username;
            this.Password = Password;
        }
        public string Title { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
