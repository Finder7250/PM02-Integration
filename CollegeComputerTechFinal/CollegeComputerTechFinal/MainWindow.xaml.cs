using System.Windows;
using System.Windows.Controls;
using CollegeComputerTechFinal.Pages;

namespace CollegeComputerTechFinal
{
    public partial class MainWindow : Window
    {
        // Статические поля для хранения данных текущего пользователя
        public static int LoggedUserId { get; set; }
        public static string LoggedUserFullName { get; set; }
        public static string LoggedUserRole { get; set; }
        public static int? LoggedUserCabinetId { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            txtStatus.Text = $"Добро пожаловать, {LoggedUserFullName}!";
            MainFrame.Navigate(new NewsPage());
        }

        private void News_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new NewsPage());
            SetActiveMenuItem(sender as MenuItem);
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilePage());
            SetActiveMenuItem(sender as MenuItem);
        }

        private void Cabinets_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CabinetsPage());
            SetActiveMenuItem(sender as MenuItem);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Метод для выделения активного пункта меню
        private void SetActiveMenuItem(MenuItem activeItem)
        {
            // Сброс всех цветов
            foreach (var item in ((Menu)this.FindName("MainMenu")).Items)
            {
                if (item is MenuItem menuItem)
                {
                    menuItem.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
                    menuItem.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80));
                }
            }

            // Выделение активного элемента
            if (activeItem != null)
            {
                activeItem.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219));
                activeItem.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(41, 57, 75));
            }
        }
    }
}