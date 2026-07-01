using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic;
using CollegeComputerTechFinal.DAL;
using System.Windows.Controls;

namespace CollegeComputerTechFinal
{
    public partial class CabinetDetailWindow : Window
    {
        private int cabinetId;
        private string cabinetNumber;

        public CabinetDetailWindow(int cabinetId, string cabinetNumber)
        {
            InitializeComponent();
            this.cabinetId = cabinetId;
            this.cabinetNumber = cabinetNumber;
            txtCabinetTitle.Text = $"Компьютеры кабинета №{cabinetNumber}";
            LoadComputers();
        }

        private void LoadComputers()
        {
            List<ComputerItem> computers = new List<ComputerItem>();

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"SELECT код_арм, имя_пк, ип_адрес, статус 
                                 FROM АРМ 
                                 WHERE код_кабинета = @cabId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@cabId", cabinetId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            computers.Add(new ComputerItem
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                IpAddress = reader.GetString(2),
                                Status = reader.IsDBNull(3) ? "Не указан" : reader.GetString(3)
                            });
                        }
                    }
                }
            }

            ComputersList.ItemsSource = computers;
        }

        private void SetStatusWorking_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int computerId = (int)button.Tag;
            UpdateComputerStatus(computerId, "Работает");
        }

        private void SetStatusRepair_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int computerId = (int)button.Tag;
            UpdateComputerStatus(computerId, "Ремонт");
        }

        private void SetStatusBroken_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int computerId = (int)button.Tag;
            UpdateComputerStatus(computerId, "Не работает");
        }

        private void UpdateComputerStatus(int computerId, string newStatus)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = "UPDATE АРМ SET статус = @status WHERE код_арм = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@status", newStatus);
                    cmd.Parameters.AddWithValue("@id", computerId);
                    cmd.ExecuteNonQuery();
                }
            }

            DatabaseHelper.AddNotification(
                $"Изменение статуса компьютера {computerId}",
                $"Статус изменён на '{newStatus}' в кабинете {cabinetNumber}",
                "admin"
            );

            MessageBox.Show($"Статус компьютера {computerId} изменён на '{newStatus}'",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadComputers();
        }
        private void Details_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int computerId = (int)button.Tag;

            ComputerDetailsWindow detailsWindow = new ComputerDetailsWindow(computerId);
            detailsWindow.ShowDialog();
        }
    }


    public class ComputerItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Status { get; set; }
    }
}