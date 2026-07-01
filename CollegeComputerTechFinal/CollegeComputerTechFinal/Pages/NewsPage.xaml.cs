using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using CollegeComputerTechFinal.DAL;

namespace CollegeComputerTechFinal.Pages
{
    public partial class NewsPage : Page
    {
        public NewsPage()
        {
            InitializeComponent();
            LoadNews();
        }

        private void LoadNews()
        {
            List<NewsItem> newsList = new List<NewsItem>();

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"SELECT заголовок, текст, дата_публикации 
                                 FROM Новость 
                                 WHERE актуальна = 1 
                                 ORDER BY дата_публикации DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        newsList.Add(new NewsItem
                        {
                            Title = reader.GetString(0),
                            Text = reader.GetString(1),
                            Date = reader.GetDateTime(2).ToString("dd.MM.yyyy")
                        });
                    }
                }
            }

            NewsList.ItemsSource = newsList;
        }

        // НОВЫЙ МЕТОД ДЛЯ ДОБАВЛЕНИЯ НОВОСТИ
        private void AddNews_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, что пользователь - администратор
            if (MainWindow.LoggedUserRole != "admin")
            {
                MessageBox.Show("Только администратор может добавлять новости!", "Доступ запрещён",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создаём простое окно для добавления новости
            Window addWindow = new Window
            {
                Title = "Добавление новости",
                Width = 450,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            Grid grid = new Grid { Margin = new Thickness(15) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Заголовок
            TextBlock lblTitle = new TextBlock { Text = "Заголовок:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 5) };
            Grid.SetRow(lblTitle, 0);
            grid.Children.Add(lblTitle);

            TextBox txtTitle = new TextBox { FontSize = 14, Margin = new Thickness(0, 0, 0, 10) };
            Grid.SetRow(txtTitle, 1);
            grid.Children.Add(txtTitle);

            // Текст новости
            TextBlock lblText = new TextBlock { Text = "Текст новости:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 5, 0, 5) };
            Grid.SetRow(lblText, 2);
            grid.Children.Add(lblText);

            TextBox txtText = new TextBox { FontSize = 14, Height = 100, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true };
            Grid.SetRow(txtText, 3);
            grid.Children.Add(txtText);

            // Кнопки
            StackPanel panelButtons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 15, 0, 0) };

            Button btnSave = new Button
            {
                Content = "Сохранить",
                Width = 100,
                Height = 30,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96)),
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 0, 10, 0)
            };

            Button btnCancel = new Button
            {
                Content = "Отмена",
                Width = 100,
                Height = 30,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(149, 165, 166)),
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
                BorderThickness = new Thickness(0)
            };

            panelButtons.Children.Add(btnSave);
            panelButtons.Children.Add(btnCancel);

            Grid.SetRow(panelButtons, 4);
            grid.Children.Add(panelButtons);

            addWindow.Content = grid;

            btnSave.Click += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtText.Text))
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    using (SqlConnection conn = DatabaseHelper.GetConnection())
                    {
                        conn.Open();
                        string query = @"INSERT INTO Новость (заголовок, текст, дата_публикации, актуальна, код_автора) 
                                         VALUES (@title, @text, @date, 1, @author)";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@title", txtTitle.Text.Trim());
                            cmd.Parameters.AddWithValue("@text", txtText.Text.Trim());
                            cmd.Parameters.AddWithValue("@date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@author", MainWindow.LoggedUserId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Новость добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    addWindow.Close();
                    LoadNews();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении новости: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            btnCancel.Click += (s, args) => addWindow.Close();

            addWindow.ShowDialog();
        }
    }

    public class NewsItem
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public string Date { get; set; }
    }
}