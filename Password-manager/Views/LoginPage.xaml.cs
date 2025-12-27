using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using Password_manager.Services;
using Password_manager.ViewModels;

namespace Password_manager;

public partial class LoginPage : ContentPage
{	

	public LoginPage(LoginPageViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;

        vm.NavigateToPage = async (route) =>
        {
            await Shell.Current.GoToAsync(route);
        };
    }

}