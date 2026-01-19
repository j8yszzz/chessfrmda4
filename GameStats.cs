using ChessLogic;
using System;

namespace Chess
{
    public class GameStats
    {
        public int GamesPlayed;
        public int Wins;
        public int Losses;
        public int Draws;

        public GameStats()
        {
            GamesPlayed = 0;
            Wins = 0;
            Losses = 0;
            Draws = 0;
        }

        public static GameStats Load()
        {
            GameStats s = new GameStats();

            if (AuthHelper.IsLoggedIn())
            {
                try
                {
                    s = DatabaseHelper.LoadStatsForUser(AuthHelper.CurrentUser.UserID);
                }
                catch
                {
                    // db failed, use empty
                }
            }

            return s;
        }

        public void Save()
        {
            if (!AuthHelper.IsLoggedIn()) return;

            try
            {
                DatabaseHelper.SaveStatsForUser(AuthHelper.CurrentUser.UserID, this);
            }
            catch
            {
                // ignore
            }
        }

        public void RecordGame(Player winner, Player human, int diff, int moves)
        {
            GamesPlayed++;

            string reason;
            if (winner == Player.None)
            {
                Draws++;
                reason = "Draw";
            }
            else if (winner == human)
            {
                Wins++;
                reason = "Win";
            }
            else
            {
                Losses++;
                reason = "Loss";
            }

            Save();

            if (AuthHelper.IsLoggedIn())
            {
                try
                {
                    DatabaseHelper.RecordGameForUser(
                        AuthHelper.CurrentUser.UserID,
                        winner,
                        human,
                        diff,
                        reason,
                        moves
                    );
                }
                catch
                {
                    // ignore
                }
            }
        }

        public double WinRate()
        {
            if (GamesPlayed == 0) return 0;
            return (double)Wins / GamesPlayed * 100;
        }
    }
}