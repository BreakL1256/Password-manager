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
        public PasswordItem(string Title, string Username, string Password, string Category)
        {
            this.Title = Title ?? "";
            this.Username = Username;
            this.Password = Password;
            this.Category = Category ?? "General";
        }
        public string Title { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Category { get; set; }
    }
    public class NoteItem
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string ContentPreview
        {
            get
            {
                // Strip HTML tags and truncate
                var plainText = System.Text.RegularExpressions.Regex.Replace(
                    Content ?? "",
                    "<.*?>",
                    string.Empty
                );

                // Decode HTML entities like &nbsp; &amp;
                plainText = System.Net.WebUtility.HtmlDecode(plainText);

                return plainText.Length > 80
                    ? plainText.Substring(0, 80) + "..."
                    : plainText;
            }
        }
    }
}
