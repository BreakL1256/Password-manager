using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Password_manager.Services;
using Password_manager.ViewModels;

namespace Password_manager;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterPageViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;

		vm.NavigateToPage = async (route) =>
		{
			await Shell.Current.GoToAsync(route);
		};
    }
}