using System;
using System.Data.SqlClient;
using System.Windows;
using CollegeComputerTechFinal.DAL;
using System.Threading.Tasks;

namespace CollegeComputerTechFinal
{
    public partial class ComputerDetailsWindow : Window
    {
        private int computerId;

        public ComputerDetailsWindow(int computerId)
        {
            InitializeComponent();
            this.computerId = computerId;
            LoadComputerDetails();
            LoadPublicIP(); // ВЫЗЫВАЕМ НОВЫЙ МЕТОД
        }

        private void LoadComputerDetails()
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        а.имя_пк, а.ип_адрес, а.инвентарный_номер, а.тип_устройства, а.статус,
                        к.номер_кабинета, к.ответственный,
                        п.модель AS процессор, о.объем_гб AS озу, н.тип + ' ' + CAST(н.объем_гб AS NVARCHAR) + ' ГБ' AS накопитель
                    FROM АРМ а
                    LEFT JOIN Кабинет к ON а.код_кабинета = к.код_кабинета
                    LEFT JOIN Комплектация кп ON а.код_арм = кп.код_арм
                    LEFT JOIN Процессор п ON кп.код_процессора = п.код_процессора
                    LEFT JOIN ОперативнаяПамять о ON кп.код_озу = о.код_озу
                    LEFT JOIN Накопитель н ON кп.код_накопителя = н.код_накопителя
                    WHERE а.код_арм = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", computerId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtTitle.Text = $"💻 {reader.GetString(0)}";
                            txtName.Text = reader.GetString(0);
                            txtIp.Text = reader.GetString(1);
                            txtInv.Text = reader.IsDBNull(2) ? "—" : reader.GetString(2);
                            txtType.Text = reader.IsDBNull(3) ? "—" : reader.GetString(3);

                            string status = reader.IsDBNull(4) ? "Не указан" : reader.GetString(4);
                            txtStatus.Text = status;
                            if (status == "Работает") txtStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96));
                            else if (status == "Ремонт") txtStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 126, 34));
                            else txtStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60));

                            txtCabinet.Text = reader.IsDBNull(5) ? "—" : reader.GetString(5);
                            txtResponsible.Text = reader.IsDBNull(6) ? "—" : reader.GetString(6);
                            txtCpu.Text = reader.IsDBNull(7) ? "—" : $"Процессор: {reader.GetString(7)}";
                            txtRam.Text = reader.IsDBNull(8) ? "—" : $"ОЗУ: {reader.GetInt32(8)} ГБ";
                            txtDisk.Text = reader.IsDBNull(9) ? "—" : $"Накопитель: {reader.GetString(9)}";
                        }
                    }
                }
            }
        }

        private async void LoadPublicIP()
        {
            string publicIp = await DatabaseHelper.GetPublicIPAddressAsync();
            txtPublicIP.Text = publicIp;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}