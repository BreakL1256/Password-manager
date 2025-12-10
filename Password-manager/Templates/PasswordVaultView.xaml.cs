using Password_manager.Entities;
using Password_manager.Templates;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Password_manager.Services;

namespace Password_manager.Templates;

public partial class PasswordVaultView : ContentView, INotifyPropertyChanged
{
    public IAsyncRelayCommand<PasswordItem> DeleteCommand { get; }
    private readonly IServiceProvider _services;
    private readonly RequestHandler _handler;
    private PasswordItem? _selectedPassword;
    public ObservableCollection<PasswordItem> PasswordList { get; set; } = new ObservableCollection<PasswordItem>();
    public PasswordVaultView(IServiceProvider services, RequestHandler handler)
	{
		InitializeComponent();
        _services = services;
        _handler = handler;

        DeleteCommand = new AsyncRelayCommand<PasswordItem>(DeleteSelectedData);

        BindingContext = this;

        this.Loaded += OnViewLoaded;
    }

    private async void OnViewLoaded(object sender, EventArgs e)
    {
        await LoadData();
    }

    private void OnShowAddView(object sender, EventArgs e)
    {
        var view = _services.GetService<AddNewDataView>();
        view.OnDataAdded = async () => await LoadData();
        DynamicContentView.Content = view;
    }

    private async Task DeleteSelectedData(PasswordItem? Item)
    {
        if (Item == null)
        {
            return;
        }

        await _handler.DeleteDataFromAccount(Item);

        await LoadData();
    }

    public async Task LoadData()
    {
        var data = await _handler.GetAccountSavedData();

        PasswordList.Clear();

        foreach (var item in data)
        {
            PasswordList.Add(item);
        }

    }

    private void OnShowDataView(object sender, TappedEventArgs e)
    {
        if (e.Parameter is PasswordItem _selectedPassword)
        {
            DynamicContentView.Content = new ViewDataView(_selectedPassword);
        }
    }

    public PasswordItem? SelectedPassword
    {
        get => _selectedPassword;
        set
        {
            _selectedPassword = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}