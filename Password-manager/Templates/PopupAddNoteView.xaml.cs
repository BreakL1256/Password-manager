using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using Password_manager.Services;

namespace Password_manager.Templates;
public partial class PopupAddNoteView : Popup
{
	public IAsyncRelayCommand SaveNewNoteToDatabaseCommand { get; }
	public readonly RequestHandler _handler;
    public PopupAddNoteView(RequestHandler handler)
	{
		InitializeComponent();
		
		_handler = handler;

		SaveNewNoteToDatabaseCommand = new AsyncRelayCommand(
			execute: SaveNewNoteToDatabase,
			canExecute: CheckContent
		);

		contentField.TextChanged += (s, e) => SaveNewNoteToDatabaseCommand.NotifyCanExecuteChanged();

		BindingContext = this;
	}

	public async Task SaveNewNoteToDatabase()
	{
		await _handler.CreateNote(contentField.Text);
	}

	public bool CheckContent()
	{
		return !String.IsNullOrWhiteSpace(contentField.Text);
	}
}