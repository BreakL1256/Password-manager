using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;

namespace Password_manager
{
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
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private PasswordItem? _selectedPassword;
        public List<PasswordItem> PasswordList { get; set; } = new List<PasswordItem>();
        public MainPage()
        {
            PasswordList.Add(new PasswordItem("gmail password","tom","abc"));
            PasswordList.Add(new PasswordItem("netflix password","john", "cba"));

            InitializeComponent();

            BindingContext = this;
        }

        public PasswordItem? SelectedPassword
        {
            get => _selectedPassword;
            set
            {
                _selectedPassword = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
