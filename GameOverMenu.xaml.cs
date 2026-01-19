using ChessLogic;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    /// <summary>
    /// Interaction logic for GameOverMenu.xaml
    /// </summary>
    public partial class GameOverMenu : UserControl
    {
        public event Action<Option> OptionSelected;

        public GameOverMenu(GameState gameState)
        {
            InitializeComponent();

            Result result = gameState.Result;
            WinnerText.Text = GetWinnerText(result.Winner);
            ReasonText.Text = GetReasonText(result.Reason, gameState.CurrentPlayer);
        }

        private static string GetWinnerText(Player winner)
        {
            return winner switch
            {
                Player.White => "White Win!",
                Player.Black => "Black Win!",
                _ => "It's a draw"
            };
        }

        private static string PlayerString(Player player)
        {
            return player switch
            {
                Player.White => "white",
                Player.Black => "black",
                _ => ""
            };
        }

        private static string GetReasonText(EndReason reason, Player currentPlayer)
        {
            return reason switch
            {
                EndReason.Stalemate => $"Stalemate - {PlayerString(currentPlayer)} can't move",
                EndReason.Checkmate => $"Checkmate - {PlayerString(currentPlayer)} can't move",
                EndReason.FiftyMoveRule => "Fifty-Move Rule",
                EndReason.InsufficientMaterial => "Insufficient Material",
                EndReason.ThreefoldRepetition => "Threefold Repetition",
                _ => ""
            };
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Restart);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Exit);
        }
    }
}
