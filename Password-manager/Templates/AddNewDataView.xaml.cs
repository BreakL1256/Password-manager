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

    public AddNewDataView(RequestHandler handler)
    {
        InitializeComponent();

        _handler = handler;
    }

    public Func<Task> OnDataAdded { get; set; }

    private async void OnNewDataSubmit(object sender, EventArgs e)
    {
        if (_handler != null && TitleField.Text != "" && UsernameField.Text != "" && PasswordField.Text != "" && CategoryField.Text != "")
        {
            PasswordItem newItem = new PasswordItem(TitleField.Text, UsernameField.Text, PasswordField.Text, CategoryField.Text);
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