using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using Password_manager.Entities;
using Password_manager.Services;

namespace Password_manager.Templates;

public partial class PopupEditNoteView : Popup
{
	public NoteItem Note { get; set; }
	public IAsyncRelayCommand SaveChangesToNoteToDatabaseCommand { get; }
	private readonly RequestHandler _handler ;
    public PopupEditNoteView(RequestHandler handler, NoteItem selectedNote)
	{
		InitializeComponent();

		_handler = handler;

		Note = selectedNote;
		contentField.Text = Note.Content;

		SaveChangesToNoteToDatabaseCommand = new AsyncRelayCommand( 
			execute: SaveNoteChangesToDatabase,
			canExecute: CheckContent
			);

		contentField.TextChanged += (s, e) => SaveChangesToNoteToDatabaseCommand.NotifyCanExecuteChanged();

        BindingContext = this;
	}

	public async Task SaveNoteChangesToDatabase()
	{
		bool isSuccess = await _handler.UpdateNote( Note.Id, contentField.Text);
		if (isSuccess)
		{
			Close("Note_saved_succesfully");
		}
	}

	private bool CheckContent()
	{
		return !String.IsNullOrEmpty(contentField.Text);
	}
}