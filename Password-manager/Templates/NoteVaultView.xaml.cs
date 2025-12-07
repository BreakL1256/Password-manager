using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;

namespace Password_manager.Templates;

public partial class NoteVaultView : ContentView
{
	public IAsyncRelayCommand AddNewNoteCommand { get; }
    private readonly IServiceProvider _services;
    public NoteVaultView(IServiceProvider service)
	{
		InitializeComponent();

		_services = service;

		AddNewNoteCommand = new AsyncRelayCommand(AddNewNote);

		BindingContext = this;
	}

	public async Task AddNewNote()
	{
		var popup = _services.GetService<PopupAddNoteView>();
		var popupMessage = await Shell.Current.ShowPopupAsync(popup);
	}
}