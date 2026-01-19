using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Chess
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }

    public class AuthHelper
    {
        private static string connStr =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ChessGameDB;Integrated Security=True";

        public static User CurrentUser { get; set; }

        public static string HashPassword(string pass)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(pass));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static (bool success, string message, int userId) Register(string user, string pass, string mail)
        {
            if (string.IsNullOrWhiteSpace(user) || user.Length < 3)
            {
                return (false, "Username must be at least 3 characters", -1);
            }

            if (string.IsNullOrWhiteSpace(pass) || pass.Length < 6)
            {
                return (false, "Password must be at least 6 characters", -1);
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_UserRegister", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Username", user);
                        cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(pass));
                        cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(mail) ? DBNull.Value : (object)mail);

                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                int id = r.GetInt32(0);
                                if (id == -1)
                                {
                                    return (false, "Username already taken", -1);
                                }
                                return (true, "Registration successful!", id);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, "Database not setup: " + ex.Message, -1);
            }

            return (false, "Registration failed", -1);
        }

        public static (bool success, string message, User user) Login(string user, string pass)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                return (false, "Please enter username and password", null);
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_UserLogin", conn))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Username", user);
                        cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(pass));

                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                if (r.IsDBNull(0))
                                {
                                    return (false, "Invalid username or password", null);
                                }

                                User u = new User
                                {
                                    UserID = r.GetInt32(0),
                                    Username = r.GetString(1),
                                    Email = r.IsDBNull(2) ? "" : r.GetString(2)
                                };

                                CurrentUser = u;
                                return (true, "Login successful!", u);
                            }
                        }
                    }
                }
            }
            catch
            {
                return (false, "Database not setup. Please run SQL script first.", null);
            }

            return (false, "Login failed", null);
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static bool IsLoggedIn()
        {
            return CurrentUser != null;
        }
    }
}