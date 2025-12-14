using Password_manager.Entities;

namespace Password_manager.Templates;

public partial class ViewDataView : ContentView
{
    private string _actualPassword;
    private bool _isPasswordVisible = false;
    private PasswordItem _currentItem;

    public event EventHandler<PasswordItem>? EditRequested;

    public ViewDataView(PasswordItem Item)
    {
        InitializeComponent();
        _currentItem = Item;

        TitleLabel.Text = Item.Title; // Set Title
        UsernameField.Text = Item.Username;
        CategoryField.Text = Item.Category;

        _actualPassword = Item.Password;
        UpdatePasswordDisplay();
    }

    private void OnEditClicked(object sender, EventArgs e)
    {
        EditRequested?.Invoke(this, _currentItem);
    }

    private void UpdatePasswordDisplay()
    {
        if (_isPasswordVisible)
        {
            PasswordField.Text = _actualPassword;
            TogglePasswordButton.Text = "HIDE";
        }
        else
        {
            PasswordField.Text = new string('•', _actualPassword.Length);
            TogglePasswordButton.Text = "SHOW";
        }
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e) => TogglePasswordVisibility();
    private void OnPasswordTapped(object sender, EventArgs e) => TogglePasswordVisibility();

    private void TogglePasswordVisibility()
    {
        _isPasswordVisible = !_isPasswordVisible;
        UpdatePasswordDisplay();
    }
}