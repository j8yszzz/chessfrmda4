using System;
using System.Data;
using System.Data.SqlClient;
using ChessLogic;

namespace Chess
{
    public class DatabaseHelper
    {
        private static string connStr =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ChessGameDB;Integrated Security=True";

        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static GameStats LoadStatsForUser(int userId)
        {
            GameStats stats = new GameStats();

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string q = "SELECT GamesPlayed, Wins, Losses, Draws FROM GameStats WHERE UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                stats.GamesPlayed = r.GetInt32(0);
                                stats.Wins = r.GetInt32(1);
                                stats.Losses = r.GetInt32(2);
                                stats.Draws = r.GetInt32(3);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Return empty
            }

            return stats;
        }

        public static void SaveStatsForUser(int userId, GameStats stats)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string q = @"UPDATE GameStats 
                               SET GamesPlayed = @games, 
                                   Wins = @wins, 
                                   Losses = @losses, 
                                   Draws = @draws,
                                   LastUpdated = GETDATE()
                               WHERE UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@games", stats.GamesPlayed);
                        cmd.Parameters.AddWithValue("@wins", stats.Wins);
                        cmd.Parameters.AddWithValue("@losses", stats.Losses);
                        cmd.Parameters.AddWithValue("@draws", stats.Draws);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Ignore
            }
        }

        public static void RecordGameForUser(int userId, Player winner, Player humanPlayer,
                                           int aiDiff, string reason, int moves)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string result = winner == Player.None ? "Draw" :
                                  (winner == humanPlayer ? "Win" : "Loss");

                    string q = @"INSERT INTO GameHistory 
                               (UserID, PlayerColor, AIDifficulty, Result, EndReason, MoveCount)
                               VALUES (@UserID, @color, @diff, @result, @reason, @moves)";

                    using (SqlCommand cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@color", humanPlayer.ToString());
                        cmd.Parameters.AddWithValue("@diff", aiDiff);
                        cmd.Parameters.AddWithValue("@result", result);
                        cmd.Parameters.AddWithValue("@reason", reason);
                        cmd.Parameters.AddWithValue("@moves", moves);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Ignore
            }
        }

        public static (int games, int wins, int losses, int draws, double winRate) GetStatisticsForUser(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string q = "SELECT * FROM vw_UserStatistics WHERE UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                return (
                                    r.GetInt32(2),
                                    r.GetInt32(3),
                                    r.GetInt32(4),
                                    r.GetInt32(5),
                                    r.GetDouble(6)
                                );
                            }
                        }
                    }
                }
            }
            catch { }

            return (0, 0, 0, 0, 0.0);
        }

        public static void SaveSettingsForUser(int userId, string color, int diff)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string q = @"UPDATE PlayerSettings 
                               SET PreferredColor = @color, 
                                   PreferredDifficulty = @diff,
                                   LastUpdated = GETDATE()
                               WHERE UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@color", color);
                        cmd.Parameters.AddWithValue("@diff", diff);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        public static (string color, int difficulty) LoadSettingsForUser(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string q = "SELECT PreferredColor, PreferredDifficulty FROM PlayerSettings WHERE UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(q, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                return (r.GetString(0), r.GetInt32(1));
                            }
                        }
                    }
                }
            }
            catch { }

            return ("White", 3);
        }
    }
}