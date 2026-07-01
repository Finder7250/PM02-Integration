using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using CollegeComputerTechFinal.DAL;

namespace CollegeComputerTechFinal.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadProfile();
            LoadStatistics();
            LoadNotifications();
            LoadUsers();
        }

        private void LoadProfile()
        {
            txtFullName.Text = MainWindow.LoggedUserFullName;

            string roleName = MainWindow.LoggedUserRole == "admin" ? "Администратор" :
                              MainWindow.LoggedUserRole == "technic" ? "Техник" : "Преподаватель";
            txtRole.Text = roleName;

            if (MainWindow.LoggedUserCabinetId.HasValue)
            {
                txtCabinet.Text = $"🏫 Кабинет: {MainWindow.LoggedUserCabinetId.Value}";
            }
            else
            {
                txtCabinet.Text = "🏫 Кабинет: не привязан";
            }
        }

        private void LoadStatistics()
        {
            try
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string totalQuery = "SELECT COUNT(*) FROM АРМ";
                    using (SqlCommand cmd = new SqlCommand(totalQuery, conn))
                    {
                        txtTotalComputers.Text = cmd.ExecuteScalar().ToString();
                    }

                    string workingQuery = "SELECT COUNT(*) FROM АРМ WHERE статус = 'Работает'";
                    using (SqlCommand cmd = new SqlCommand(workingQuery, conn))
                    {
                        txtWorkingComputers.Text = cmd.ExecuteScalar().ToString();
                    }

                    string brokenQuery = "SELECT COUNT(*) FROM АРМ WHERE статус = 'Не работает' OR статус = 'Ремонт'";
                    using (SqlCommand cmd = new SqlCommand(brokenQuery, conn))
                    {
                        txtBrokenComputers.Text = cmd.ExecuteScalar().ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                txtTotalComputers.Text = "0";
                txtWorkingComputers.Text = "0";
                txtBrokenComputers.Text = "0";
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadNotifications()
        {
            try
            {
                List<NotificationItem> notifications = new List<NotificationItem>();

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT TOP 5 заголовок, сообщение, дата_создания 
                                     FROM Уведомления 
                                     WHERE для_кого = @role OR для_кого = 'all'
                                     ORDER BY дата_создания DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@role", MainWindow.LoggedUserRole);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                notifications.Add(new NotificationItem
                                {
                                    Title = reader.GetString(0),
                                    Message = reader.GetString(1),
                                    Date = reader.GetDateTime(2).ToString("dd.MM.yyyy HH:mm")
                                });
                            }
                        }
                    }
                }

                NotificationsList.ItemsSource = notifications;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки уведомлений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadUsers()
        {
            try
            {
                if (MainWindow.LoggedUserRole != "admin")
                {
                    UsersBorder.Visibility = Visibility.Collapsed;
                    return;
                }

                List<UserItem> users = new List<UserItem>();

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT п.фио, р.наименование_роли, 
                               CASE WHEN к.номер_кабинета IS NOT NULL THEN к.номер_кабинета ELSE 'не привязан' END as кабинет,
                               CASE WHEN EXISTS(SELECT 1 FROM АРМ WHERE код_кабинета = п.код_кабинета) 
                                    THEN 'активен' ELSE 'ожидает' END as статус
                        FROM Пользователь п
                        LEFT JOIN Роль р ON п.код_роли = р.код_роли
                        LEFT JOIN Кабинет к ON п.код_кабинета = к.код_кабинета
                        WHERE п.код_пользователя > 1
                        ORDER BY п.фио";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new UserItem
                            {
                                FullName = reader.GetString(0),
                                Role = reader.GetString(1),
                                Cabinet = reader.GetString(2),
                                Status = reader.GetString(3)
                            });
                        }
                    }
                }

                UsersList.ItemsSource = users;
                UsersBorder.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                UsersBorder.Visibility = Visibility.Collapsed;
            }
        }
    }

    public class NotificationItem
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }
    }

    public class UserItem
    {
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Cabinet { get; set; }
        public string Status { get; set; }
    }
}