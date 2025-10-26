using Microsoft.EntityFrameworkCore.Metadata;
using Password_manager.Services;

namespace Password_manager.Templates;

public partial class PopupRegisterView : ContentView
{
	public PopupRegisterView(RequestHandler handler)
	{
		InitializeComponent();

		BindingContext = this;
	}
}