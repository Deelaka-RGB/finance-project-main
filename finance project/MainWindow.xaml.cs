using System.Windows;

namespace finance_project
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Simple demo handlers — you can later navigate to pages/windows
        private void Home_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Home is active ✅", "Finance Manager");
        }

        private void Expenses_Click(object sender, RoutedEventArgs e)
        {
            var win = new ExpensesWindow { Owner = this };
            win.Show();    // or ShowDialog() if you want it modal
        }

        private void Budgets_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Open: Budgets module (set monthly limits, alerts).", "Finance Manager");
        }

        private void Goals_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Open: Savings Goals module (progress tracking).", "Finance Manager");
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Open: Reports (monthly spend, category split, trend).", "Finance Manager");
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Open: Settings (theme, DB connections, backup).", "Finance Manager");
        }
    }
}
