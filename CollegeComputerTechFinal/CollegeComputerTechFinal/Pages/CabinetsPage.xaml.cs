using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OfficeOpenXml;
using CollegeComputerTechFinal.DAL;

namespace CollegeComputerTechFinal.Pages
{
    public partial class CabinetsPage : Page
    {
        public CabinetsPage()
        {
            InitializeComponent();
            LoadCabinets();
        }

        private void LoadCabinets()
        {
            try
            {
                List<CabinetItem> cabinets = new List<CabinetItem>();

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"SELECT к.номер_кабинета, к.местонахождение, к.ответственный,
                                            COUNT(а.код_арм) as КоличествоАРМ
                                     FROM Кабинет к
                                     LEFT JOIN АРМ а ON к.код_кабинета = а.код_кабинета
                                     GROUP BY к.номер_кабинета, к.местонахождение, к.ответственный
                                     ORDER BY CAST(к.номер_кабинета AS INT)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cabinets.Add(new CabinetItem
                            {
                                Number = reader.GetString(0),
                                Location = reader.GetString(1),
                                Responsible = reader.GetString(2),
                                ArmsCount = reader.GetInt32(3)
                            });
                        }
                    }
                }

                CabinetsList.ItemsSource = cabinets;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки кабинетов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CabinetsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CabinetsList.SelectedItem is CabinetItem selected)
            {
                int cabinetId = GetCabinetIdByNumber(selected.Number);
                CabinetDetailWindow detailWindow = new CabinetDetailWindow(cabinetId, selected.Number);
                detailWindow.ShowDialog();
            }
        }

        private int GetCabinetIdByNumber(string cabinetNumber)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT код_кабинета FROM Кабинет WHERE номер_кабинета = @number";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@number", cabinetNumber);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Установка лицензии для EPPlus
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            к.номер_кабинета AS 'Номер',
                            к.местонахождение AS 'Местонахождение',
                            к.ответственный AS 'Ответственный',
                            COUNT(а.код_арм) AS 'Кол-во ПК',
                            SUM(CASE WHEN а.статус = 'Работает' THEN 1 ELSE 0 END) AS 'Работает',
                            SUM(CASE WHEN а.статус = 'Ремонт' THEN 1 ELSE 0 END) AS 'Ремонт',
                            SUM(CASE WHEN а.статус = 'Не работает' THEN 1 ELSE 0 END) AS 'Не работает'
                        FROM Кабинет к
                        LEFT JOIN АРМ а ON к.код_кабинета = а.код_кабинета
                        GROUP BY к.код_кабинета, к.номер_кабинета, к.местонахождение, к.ответственный
                        ORDER BY CAST(к.номер_кабинета AS INT)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        using (ExcelPackage package = new ExcelPackage())
                        {
                            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Кабинеты");

                            // Заголовки
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                worksheet.Cells[1, i + 1].Value = reader.GetName(i);
                                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                                worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(211, 211, 211));
                                worksheet.Cells[1, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            }

                            // Данные
                            int row = 2;
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    worksheet.Cells[row, i + 1].Value = reader[i]?.ToString() ?? "0";
                                    worksheet.Cells[row, i + 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                                }
                                row++;
                            }

                            // Автоширина
                            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                            SaveFileDialog saveDialog = new SaveFileDialog
                            {
                                Filter = "Excel файлы (*.xlsx)|*.xlsx",
                                FileName = $"Отчет_по_кабинетам_{DateTime.Now:dd.MM.yyyy}.xlsx"
                            };

                            if (saveDialog.ShowDialog() == true)
                            {
                                File.WriteAllBytes(saveDialog.FileName, package.GetAsByteArray());
                                MessageBox.Show($"Отчёт сохранён!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class CabinetItem
    {
        public string Number { get; set; }
        public string Location { get; set; }
        public string Responsible { get; set; }
        public int ArmsCount { get; set; }
    }
}