using Password_manager.Entities;
using Password_manager.Templates;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;

namespace Password_manager
{
   
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private readonly IServiceProvider _services;

        private PasswordItem? _selectedPassword;
        public List<PasswordItem> PasswordList { get; set; } = new List<PasswordItem>();
        public MainPage(IServiceProvider services)
        {
            PasswordList.Add(new PasswordItem("gmail password", "tom", "abc"));
            PasswordList.Add(new PasswordItem("netflix password", "john", "cba"));

            InitializeComponent();

            BindingContext = this;
            _services = services;
        }

        private void OnShowAddView(object sender, EventArgs e)
        {
            var view = _services.GetService<AddNewDataView>();
            DynamicContentView.Content = view;
        }

        private void OnShowDataView(object sender, TappedEventArgs e)
        {
            if (e.Parameter is PasswordItem _selectedPassword)
            {
                DynamicContentView.Content = new ViewDataView(_selectedPassword);
            }
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
