using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using Password_manager.Entities;
using Password_manager.Services;

namespace Password_manager.Templates;

public partial class NoteVaultView : ContentView, INotifyPropertyChanged
{
	public IAsyncRelayCommand AddNewNoteCommand { get; }
	public IAsyncRelayCommand DeleteNoteCommand { get; }
    public IAsyncRelayCommand OpenNoteEditorCommand { get; }
    private readonly IServiceProvider _services;
    private readonly RequestHandler _handler;

    private NoteItem? _selectedNote;
    public ObservableCollection<NoteItem> NoteList { get; set; } = new ObservableCollection<NoteItem>();
    public NoteVaultView(IServiceProvider service, RequestHandler handler)
	{
		InitializeComponent();

		_services = service;
        _handler = handler;

		AddNewNoteCommand = new AsyncRelayCommand(AddNewNote);
		DeleteNoteCommand = new AsyncRelayCommand<NoteItem>(DeleteNote);
        OpenNoteEditorCommand = new AsyncRelayCommand(OpenNoteEditor);

        BindingContext = this;

        this.Loaded += OnViewLoaded;
	}

    private async void OnViewLoaded(object sender, EventArgs e)
    {
        await LoadNotes();
    }

    private async Task LoadNotes()
    {
        var notes = await _handler.GetNotesByUser();

        NoteList.Clear();

        foreach (var note in notes)
        {
            NoteList.Add(note);
        }
    }

    public async Task OpenNoteEditor()
    {
        if (SelectedNote == null) return;
        var popup = ActivatorUtilities.CreateInstance<PopupEditNoteView>(_services, SelectedNote);
        var popupMessage = await Shell.Current.ShowPopupAsync(popup);
        if(popupMessage?.ToString() == "Note_saved_succesfully")
        {
            await LoadNotes();
        }
    }


    public async Task AddNewNote()
	{
		var popup = _services.GetService<PopupAddNoteView>();
		var popupMessage = await Shell.Current.ShowPopupAsync(popup);

        if (popupMessage?.ToString() == "Note_added_succesfully")
        {
            await LoadNotes();
        }

    }

	public async Task DeleteNote(NoteItem? item)
	{
        if(item == null)
        {
            return;
        }
        
        await _handler.DeleteNote(item.Id);

        await LoadNotes();
    }

    public NoteItem? SelectedNote
    {
        get => _selectedNote;
        set
        {
            _selectedNote = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}