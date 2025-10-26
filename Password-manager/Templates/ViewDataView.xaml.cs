using Password_manager.Entities;
namespace Password_manager.Templates;

public partial class ViewDataView : ContentView
{
	public ViewDataView(PasswordItem Item)
	{
		InitializeComponent();

		UsernameField.Text = Item.Username;
		PasswordField.Text = Item.Password;
	}
}