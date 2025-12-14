using Password_manager.Entities;
using Password_manager.Services;
using System.Text;

namespace Password_manager.Templates;

public partial class EditDataView : ContentView
{
    private readonly RequestHandler _handler;
    private readonly PasswordItem _originalItem;
    private string _selectedCategory = "General";
    private readonly Random _random = new Random();

    public event EventHandler? Cancelled;
    public event EventHandler? Saved;

    public EditDataView(RequestHandler handler, PasswordItem item)
    {
        InitializeComponent();
        _handler = handler;
        _originalItem = item;


        TitleField.Text = item.Title;
        UsernameField.Text = item.Username;
        PasswordField.Text = item.Password;
        UpdateCategorySelection(item.Category ?? "General");
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleField.Text) ||
            string.IsNullOrWhiteSpace(UsernameField.Text) ||
            string.IsNullOrWhiteSpace(PasswordField.Text))
            return;

        var newItem = new PasswordItem(
            TitleField.Text,
            UsernameField.Text,
            PasswordField.Text,
            _selectedCategory
        );

        try
        {
            await _handler.UpdateDataInAccount(_originalItem, newItem);
            Saved?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update failed: {ex}");
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

    private void OnCategoryClicked(object sender, EventArgs e)
    {
        if (sender is Button btn) UpdateCategorySelection(btn.Text);
    }

    private void UpdateCategorySelection(string category)
    {
        _selectedCategory = category;

        var btns = new[] { BtnGeneral, BtnWork, BtnSocial, BtnOther };
        foreach (var btn in btns)
        {
            btn.BackgroundColor = Colors.Transparent;

            btn.SetAppThemeColor(Button.BorderColorProperty, Color.FromArgb("#C6C6C8"), Color.FromArgb("#636366"));
            btn.SetAppThemeColor(Button.TextColorProperty, Colors.Black, Colors.White);
        }

        Button selectedBtn = category switch
        {
            "Work" => BtnWork,
            "Social" => BtnSocial,
            "Other" => BtnOther,
            _ => BtnGeneral
        };

        var color = category switch
        {
            "Work" => Color.FromArgb("#FF9500"),
            "Social" => Color.FromArgb("#AF52DE"),
            "Other" => Color.FromArgb("#FF3B30"),
            _ => Color.FromArgb("#007AFF")
        };

        selectedBtn.BorderColor = color;
        selectedBtn.TextColor = color;
    }

    private void OnGeneratePasswordClicked(object sender, EventArgs e)
    {
        PasswordField.Text = GenerateSafePassword();
    }

    private string GenerateSafePassword(int length = 20)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()";
        var result = new StringBuilder();
        for (int i = 0; i < length; i++) result.Append(chars[_random.Next(chars.Length)]);
        return result.ToString();
    }
}