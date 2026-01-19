
using ChessLogic;
using Microsoft.VisualBasic;
using System;
using System.Data.SqlClient;
using System.Windows;

namespace Chess
{
    public partial class HomePage : Window
    {
        private int diff;
        private Player colour;
        private GameStats stats;

        public HomePage()
        {
            InitializeComponent();
            diff = 8;
            colour = Player.White;

            stats = GameStats.Load();
            ShowStats();

            if (AuthHelper.IsLoggedIn())
            {
                try
                {
                    var settings = DatabaseHelper.LoadSettingsForUser(AuthHelper.CurrentUser.UserID);
                    colour = settings.color == "White" ? Player.White : Player.Black;
                    diff = settings.difficulty;
                }
                catch
                {
                    // Use defaults if database fails
                }

                if (diff == 2)
                    RbEasy.IsChecked = true;
                else if (diff == 5)
                    RbMedium.IsChecked = true;
                else if (diff == 8)
                RbHard.IsChecked = true;

                if (colour == Player.White)
                    RbWhite.IsChecked = true;
                else
                    RbBlack.IsChecked = true;

                TxtWelcome.Text = "Welcome, " + AuthHelper.CurrentUser.Username + "!";
            }
            else
            {
                TxtWelcome.Text = "Welcome!";
            }
        }

        private void ShowStats()
        {
            TxtGamesPlayed.Text = stats.GamesPlayed.ToString();
            TxtWins.Text = stats.Wins.ToString();
            TxtLosses.Text = stats.Losses.ToString();
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow game = new MainWindow(colour, diff, stats);
                game.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error starting game: " + ex.Message);
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            PnlSettings.Visibility = Visibility.Visible;
        }

        private void BtnCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            if (diff == 2)
                RbEasy.IsChecked = true;
            else if (diff == 4)  
                RbMedium.IsChecked = true;
            else if (diff == 7)  
                RbHard.IsChecked = true;

            colour = RbWhite.IsChecked == true ? Player.White : Player.Black;

            if (AuthHelper.IsLoggedIn())
            {
                try
                {
                    DatabaseHelper.SaveSettingsForUser(AuthHelper.CurrentUser.UserID, colour.ToString(), diff);
                }
                catch
                {
                    // Ignore save errors
                }
            }

            PnlSettings.Visibility = Visibility.Collapsed;
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            PnlHelp.Visibility = Visibility.Visible;
        }

        private void BtnCloseHelp_Click(object sender, RoutedEventArgs e)
        {
            PnlHelp.Visibility = Visibility.Collapsed;
        }

        private void BtnStats_Click(object sender, RoutedEventArgs e)
        {
            if (AuthHelper.IsLoggedIn())
            {
                try
                {
                    var data = DatabaseHelper.GetStatisticsForUser(AuthHelper.CurrentUser.UserID);

                    StGames.Text = data.games.ToString();
                    StWins.Text = data.wins.ToString();
                    StLosses.Text = data.losses.ToString();
                    StDraws.Text = data.draws.ToString();
                    StRate.Text = data.winRate.ToString("F1") + "%";
                }
                catch
                {
                    // Use local stats if database fails
                    StGames.Text = stats.GamesPlayed.ToString();
                    StWins.Text = stats.Wins.ToString();
                    StLosses.Text = stats.Losses.ToString();
                    StDraws.Text = stats.Draws.ToString();
                    StRate.Text = stats.WinRate().ToString("F1") + "%";
                }
            }
            else
            {
                StGames.Text = stats.GamesPlayed.ToString();
                StWins.Text = stats.Wins.ToString();
                StLosses.Text = stats.Losses.ToString();
                StDraws.Text = stats.Draws.ToString();
                StRate.Text = stats.WinRate().ToString("F1") + "%";
            }

            PnlStats.Visibility = Visibility.Visible;
        }

        private void BtnCloseStats_Click(object sender, RoutedEventArgs e)
        {
            PnlStats.Visibility = Visibility.Collapsed;
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AuthHelper.Logout();
            LoginPage login = new LoginPage();
            login.Show();
            this.Close();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}