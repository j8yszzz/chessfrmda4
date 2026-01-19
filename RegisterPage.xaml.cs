using System;

using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using System.Windows.Media;


namespace Chess
{
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Window
    {
        public RegisterPage()
        {
            InitializeComponent();
            TxtUsername.Focus();
        }
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            AttemptRegister();
        }

        private void AttemptRegister()
        {
            string username = TxtUsername.Text.Trim();
            string email = TxtEmail.Text.Trim();
            string password = TxtPassword.Password;
            string confirm = TxtConfirm.Password;

            if (password != confirm)
            {
                ShowMsg("Passwords do not match", false);
                TxtConfirm.Clear();
                TxtConfirm.Focus();
                return;
            }

            var result = AuthHelper.Register(username, password, email);

            if (result.success)
            {
                ShowMsg("Registration successful! Redirecting...", true);

                System.Threading.Tasks.Task.Delay(1500).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        LoginPage login = new LoginPage();
                        login.Show();
                        this.Close();
                    });
                });
            }
            else
            {
                ShowMsg(result.message, false);
            }
        }

        private void ShowMsg(string msg, bool success)
        {
            TxtMsg.Text = msg;
            TxtMsg.Foreground = new SolidColorBrush(success ?
                (Color)ColorConverter.ConvertFromString("#4ecca3") :
                (Color)ColorConverter.ConvertFromString("#e94560"));
            TxtMsg.Visibility = Visibility.Visible;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            LoginPage login = new LoginPage();
            login.Show();
            this.Close();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AttemptRegister();
            }
        }
    }
}
