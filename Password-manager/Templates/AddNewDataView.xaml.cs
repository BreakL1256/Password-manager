using System.Text;
using Password_manager.Entities;
using Password_manager.Services;
namespace Password_manager.Templates;

public partial class AddNewDataView : ContentView
{
    private readonly RequestHandler _handler;
    private readonly Random _random = new Random();

    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string DigitChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";

    private string _selectedCategory = "General";

    public AddNewDataView(RequestHandler handler)
    {
        InitializeComponent();

        _handler = handler;

        UpdateCategorySelection("General");
    }

    public Func<Task> OnDataAdded { get; set; }

    private async void OnNewDataSubmit(object sender, EventArgs e)
    {
        if (_handler != null && TitleField.Text != "" && UsernameField.Text != "" && PasswordField.Text != "")
        {
            PasswordItem newItem = new PasswordItem(TitleField.Text, UsernameField.Text, PasswordField.Text, _selectedCategory);
            try
            {
                await _handler.SaveDataToAccount(newItem);

                if (OnDataAdded != null)
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

    private void OnGeneratePasswordClicked(object sender, EventArgs e)
    {
        PasswordField.Text = GenerateSafePassword();
    }

    private void OnCategoryButtonClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        UpdateCategorySelection(button.Text);
    }

    private void ResetCategoryButtons()
    {
        var defaultBackground = Colors.Transparent; // Set to transparent
        var defaultBorder = Application.Current.RequestedTheme == AppTheme.Light ?
            Color.FromArgb("#C6C6C8") : Color.FromArgb("#636366");
        var defaultTextColor = Application.Current.RequestedTheme == AppTheme.Light ?
            Color.FromArgb("#000000") : Color.FromArgb("#FFFFFF");

        GeneralCategoryButton.BackgroundColor = defaultBackground;
        GeneralCategoryButton.BorderColor = defaultBorder;
        GeneralCategoryButton.TextColor = defaultTextColor;

        WorkCategoryButton.BackgroundColor = defaultBackground;
        WorkCategoryButton.BorderColor = defaultBorder;
        WorkCategoryButton.TextColor = defaultTextColor;

        OtherCategoryButton.BackgroundColor = defaultBackground;
        OtherCategoryButton.BorderColor = defaultBorder;
        OtherCategoryButton.TextColor = defaultTextColor;
    }

    private void UpdateCategorySelection(string category)
    {
        _selectedCategory = category;

        ResetCategoryButtons();

        switch (category)
        {
            case "General":
                GeneralCategoryButton.BackgroundColor = Colors.Transparent;
                GeneralCategoryButton.BorderColor = Color.FromArgb("#007AFF");
                GeneralCategoryButton.TextColor = Color.FromArgb("#007AFF");
                break;
            case "Work":
                WorkCategoryButton.BackgroundColor = Colors.Transparent;
                WorkCategoryButton.BorderColor = Color.FromArgb("#FFCC00");
                WorkCategoryButton.TextColor = Color.FromArgb("#FF9500");
                break;
            case "Other":
                OtherCategoryButton.BackgroundColor = Colors.Transparent;
                OtherCategoryButton.BorderColor = Color.FromArgb("#FF3B30");
                OtherCategoryButton.TextColor = Color.FromArgb("#FF3B30");
                break;
        }
    }


    private string GenerateSafePassword(int length = 20)
    {
        if (length < 8)
            throw new ArgumentException("Password length should be at least 8 characters for security");

        var password = new StringBuilder();
        var allChars = LowercaseChars + UppercaseChars + DigitChars + SpecialChars;

        password.Append(GetRandomChar(LowercaseChars));
        password.Append(GetRandomChar(UppercaseChars));
        password.Append(GetRandomChar(DigitChars));
        password.Append(GetRandomChar(SpecialChars));

        for (int i = password.Length; i < length; i++)
        {
            password.Append(GetRandomChar(allChars));
        }
        return ShuffleString(password.ToString());
    }

    private char GetRandomChar(string characterSet)
    {
        return characterSet[_random.Next(characterSet.Length)];
    }

    private string ShuffleString(string input)
    {
        var chars = input.ToCharArray();

        // Fisher-Yates shuffle algorithm
        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}