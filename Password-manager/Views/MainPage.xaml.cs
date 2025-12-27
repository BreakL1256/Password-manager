using Microsoft.Maui.Controls;
using Password_manager.ViewModels;
using CommunityToolkit.Maui.Views;
using Password_manager.Templates;

namespace Password_manager
{
    public partial class MainPage : ContentPage
    {
        private readonly IServiceProvider _services;
        public MainPage(MainPageViewModel vm, IServiceProvider services)
        {
            InitializeComponent();
            BindingContext = vm;

            vm.OpenSettingsPage = async page => await Navigation.PushModalAsync(page);

            vm.ShowLoginPopup = async () =>
            {
                var loginPopup = _services.GetService<PopupLoginView>();
                return await this.ShowPopupAsync(loginPopup);
            };

            vm.ShowRegisterPopup = async popup =>
            {
                return await this.ShowPopupAsync(popup);
            };

            vm.SetTabContent = view =>
            {
                TabView.Content = view;
            };

            vm.NavigateToPage = async (route) =>
            {
                await Shell.Current.GoToAsync(route);
            };
            _services = services;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var vm = (MainPageViewModel)BindingContext;
            await vm.CheckCloudConnection();
        }
    }
}
