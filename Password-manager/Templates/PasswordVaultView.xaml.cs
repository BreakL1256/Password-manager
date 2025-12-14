using Password_manager.Entities;
using Password_manager.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using System.Linq;

namespace Password_manager.Templates;

public partial class PasswordVaultView : ContentView, INotifyPropertyChanged
{
    public IAsyncRelayCommand<PasswordItem> DeleteCommand { get; }
    public IAsyncRelayCommand<PasswordItem> RestoreCommand { get; }

    private readonly IServiceProvider _services;
    private readonly RequestHandler _handler;
    private PasswordItem? _selectedPassword;

    public ObservableCollection<PasswordItem> PasswordList { get; set; } = new ObservableCollection<PasswordItem>();
    private List<PasswordItem> _allPasswords = new List<PasswordItem>();

    private string _currentSearchText = string.Empty;
    private string _currentCategory = "All";
    private bool _isTrashMode = false;

    public PasswordVaultView(IServiceProvider services, RequestHandler handler)
    {
        InitializeComponent();
        _services = services;
        _handler = handler;

        DeleteCommand = new AsyncRelayCommand<PasswordItem>(DeleteSelectedData);
        RestoreCommand = new AsyncRelayCommand<PasswordItem>(RestoreSelectedData);

        // Initialize the button colors immediately
        UpdateFilterButtonsUI();

        BindingContext = this;
        this.Loaded += OnViewLoaded;
    }

    private async void OnViewLoaded(object sender, EventArgs e) => await LoadData();

    private void OnShowAddView(object sender, EventArgs e)
    {
        var view = _services.GetService<AddNewDataView>();
        if (view != null)
        {
            view.OnDataAdded = async () => await LoadData();
            DynamicContentView.Content = view;
        }
    }

    private async Task DeleteSelectedData(PasswordItem? Item)
    {
        if (Item == null) return;

        bool confirm = true;
        if (_isTrashMode)
        {
            confirm = await Application.Current.MainPage.DisplayAlert("Delete Permanently?",
                "This action cannot be undone.", "Delete", "Cancel");
        }

        if (confirm)
        {
            await _handler.DeleteDataFromAccount(Item);
            await LoadData();

            if (DynamicContentView.Content is ViewDataView v && v.BindingContext == Item)
            {
                DynamicContentView.Content = null;
            }
        }
    }

    private async Task RestoreSelectedData(PasswordItem? Item)
    {
        if (Item == null) return;
        await _handler.RestorePassword(Item);
        await LoadData();
    }

    public async Task LoadData()
    {
        var data = await _handler.GetAccountSavedData(_isTrashMode);
        _allPasswords = data ?? new List<PasswordItem>();
        FilterPasswords();

        TrashToggleBtn.Text = _isTrashMode ? "Back" : "Trash";
        AddPasswordBtn.IsVisible = !_isTrashMode;
    }

    private void OnToggleTrash(object sender, EventArgs e)
    {
        _isTrashMode = !_isTrashMode;
        _ = LoadData();
        DynamicContentView.Content = null;
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        _currentSearchText = e.NewTextValue ?? "";
        FilterPasswords();
    }

    private void OnFilterAll(object sender, EventArgs e) => SetCategory("All");
    private void OnFilterGeneral(object sender, EventArgs e) => SetCategory("General");
    private void OnFilterWork(object sender, EventArgs e) => SetCategory("Work");
    private void OnFilterSocial(object sender, EventArgs e) => SetCategory("Social");
    private void OnFilterOther(object sender, EventArgs e) => SetCategory("Other");

    private void SetCategory(string category)
    {
        _currentCategory = category;
        UpdateFilterButtonsUI();
        FilterPasswords();
    }

    private void UpdateFilterButtonsUI()
    {
        var blueColor = Color.FromArgb("#007AFF");
        var orangeColor = Color.FromArgb("#FF9500");
        var purpleColor = Color.FromArgb("#AF52DE");
        var redColor = Color.FromArgb("#FF3B30");

        void ApplyStyle(Button btn, string name, Color color)
        {
            bool active = _currentCategory == name;
            btn.BackgroundColor = active ? color : Colors.Transparent;
            btn.TextColor = active ? Colors.White : color;
            btn.BorderColor = color;
        }

        ApplyStyle(BtnAll, "All", blueColor);
        ApplyStyle(BtnGeneral, "General", blueColor);
        ApplyStyle(BtnWork, "Work", orangeColor);
        ApplyStyle(BtnSocial, "Social", purpleColor);
        ApplyStyle(BtnOther, "Other", redColor);
    }

    private void FilterPasswords()
    {
        PasswordList.Clear();
        if (_allPasswords == null) return;

        var filtered = _allPasswords.Where(item =>
        {
            if (_currentCategory != "All")
            {
                if (!(item.Category ?? "General").Equals(_currentCategory, StringComparison.OrdinalIgnoreCase)) return false;
            }
            if (string.IsNullOrWhiteSpace(_currentSearchText)) return true;
            return (item.Title != null && item.Title.Contains(_currentSearchText, StringComparison.OrdinalIgnoreCase)) ||
                   (item.Username != null && item.Username.Contains(_currentSearchText, StringComparison.OrdinalIgnoreCase));
        });

        foreach (var item in filtered) PasswordList.Add(item);
    }

    private void OnShowDataView(object sender, TappedEventArgs e)
    {
        if (e.Parameter is PasswordItem selected)
        {
            ShowViewData(selected);
        }
    }

    private void ShowViewData(PasswordItem item)
    {
        var view = new ViewDataView(item);

        view.EditRequested += (s, itemToEdit) =>
        {
            var editView = new EditDataView(_handler, itemToEdit);

            editView.Cancelled += (s2, e2) => ShowViewData(itemToEdit);

            editView.Saved += async (s2, e2) =>
            {
                await LoadData();

                var updatedItem = _allPasswords.FirstOrDefault(p => p.Id == itemToEdit.Id);

                if (updatedItem != null)
                    ShowViewData(updatedItem);
                else
                    DynamicContentView.Content = null;
            };

            DynamicContentView.Content = editView;
        };

        DynamicContentView.Content = view;
    }

    public PasswordItem? SelectedPassword
    {
        get => _selectedPassword;
        set { _selectedPassword = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}