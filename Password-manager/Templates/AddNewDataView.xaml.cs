using Password_manager.Entities;
using Password_manager.Services;
namespace Password_manager.Templates;

public partial class AddNewDataView : ContentView
{
	private readonly RequestHandler _handler;
	public AddNewDataView(RequestHandler handler)
	{
		InitializeComponent();

		_handler = handler;
	}

	public Func<Task> OnDataAdded { get; set; }

	private async void OnNewDataSubmit(object sender, EventArgs e)
	{
		if (_handler != null && TitleField.Text != "" && UsernameField.Text != "" && PasswordField.Text != "")
		{
			PasswordItem newItem = new PasswordItem(TitleField.Text, UsernameField.Text, PasswordField.Text);
			try
			{
				await _handler.SaveDataToAccount(newItem);

				if(OnDataAdded != null)
				{
					await OnDataAdded();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Data submission failed: " + ex);
			}
		}
	}
}