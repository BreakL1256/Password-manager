using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Password_manager
{
    public partial class SettingsPage : ContentPage
    {
        private const string ThemeKey = "app_theme";

        public string CurrentTheme
        {
            get => GetCurrentThemeName();
        }

        public SettingsPage()
        {
            InitializeComponent();
            LoadCurrentTheme();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadCurrentTheme();
        }

        private void LoadCurrentTheme()
        {
            var savedTheme = Preferences.Get(ThemeKey, "System Default");

            ThemePicker.SelectedItem = savedTheme;
            OnPropertyChanged(nameof(CurrentTheme));
        }

        private void OnThemeChanged(object sender, System.EventArgs e)
        {
            if (ThemePicker.SelectedItem == null) return;

            var selectedTheme = ThemePicker.SelectedItem.ToString();
            Preferences.Set(ThemeKey, selectedTheme);

            ApplyTheme(selectedTheme);
            OnPropertyChanged(nameof(CurrentTheme));
        }

        private void ApplyTheme(string theme)
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                switch (theme)
                {
                    case "Light Mode":
                        Application.Current.UserAppTheme = AppTheme.Light;
                        break;
                    case "Dark Mode":
                        Application.Current.UserAppTheme = AppTheme.Dark;
                        break;
                    case "System Default":
                    default:
                        Application.Current.UserAppTheme = AppTheme.Unspecified;
                        break;
                }
            });
        }

        private string GetCurrentThemeName()
        {
            var savedTheme = Preferences.Get(ThemeKey, "System Default");
            return savedTheme;
        }

        private async void OnCloseClicked(object sender, System.EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}