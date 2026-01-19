using Chess;
using System.Windows;
using System.Windows.Input;

namespace Chess
{
    public partial class LoginPage : Window
    {
        public LoginPage()
        {
            InitializeComponent();
            TxtUsername.Focus();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            DoLogin();
        }

        private void DoLogin()
        {
            string user = TxtUsername.Text.Trim();
            string pass = TxtPassword.Password;

            var result = AuthHelper.Login(user, pass);

            if (result.success)
            {
                HomePage home = new HomePage();
                home.Show();
                this.Close();
            }
            else
            {
                TxtError.Text = result.message;
                TxtError.Visibility = Visibility.Visible;
                TxtPassword.Clear();
                TxtPassword.Focus();
            }
        }

        private void LinkRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterPage register = new RegisterPage();
            register.Show();
            this.Close();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoLogin();
            }
        }
    }
}