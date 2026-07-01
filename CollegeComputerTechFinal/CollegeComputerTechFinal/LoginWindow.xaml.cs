using System;
using System.Data.SqlClient;
using System.Windows;
using CollegeComputerTechFinal.DAL;

namespace CollegeComputerTechFinal
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                txtError.Text = "Введите логин и пароль";
                return;
            }

            if (!DatabaseHelper.TestConnection())
            {
                txtError.Text = "Нет подключения к базе данных!";
                return;
            }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"SELECT u.код_пользователя, u.фио, r.наименование_роли, u.код_кабинета
                                 FROM Пользователь u
                                 LEFT JOIN Роль r ON u.код_роли = r.код_роли
                                 WHERE u.логин = @login AND u.пароль_hash = @password";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@password", password);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Сохраняем данные пользователя в MainWindow
                            MainWindow.LoggedUserId = reader.GetInt32(0);
                            MainWindow.LoggedUserFullName = reader.GetString(1);
                            MainWindow.LoggedUserRole = reader.GetString(2);
                            MainWindow.LoggedUserCabinetId = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3);

                            MessageBox.Show($"Добро пожаловать, {reader.GetString(1)}!", "Успех");

                            MainWindow mainWindow = new MainWindow();
                            mainWindow.Show();
                            this.Close();
                        }
                        else
                        {
                            txtError.Text = "Неверный логин или пароль";
                        }
                    }
                }
            }
        }
    }
}