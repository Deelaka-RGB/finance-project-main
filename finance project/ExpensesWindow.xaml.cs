using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace finance_project
{
    public partial class ExpensesWindow : Window
    {
        private readonly ObservableCollection<Expense> _expenses = new();
        private readonly ICollectionView _view;

        private readonly string[] _categories = new[]
        {
            "General","Food & Drinks","Transport","Groceries","Utilities",
            "Entertainment","Health","Education","Rent","Other"
        };

        public ExpensesWindow()
        {
            InitializeComponent();

            // 1) Prepare data first
            _expenses.Add(new Expense { Date = DateTime.Today, Category = "Food & Drinks", Description = "Lunch", Amount = 1250 });
            _expenses.Add(new Expense { Date = DateTime.Today.AddDays(-1), Category = "Transport", Description = "Bus + Tuk", Amount = 980 });
            _expenses.Add(new Expense { Date = DateTime.Today.AddDays(-2), Category = "Utilities", Description = "Electricity Bill", Amount = 6400 });

            // 2) Create the view ASAP so event handlers can use it safely
            _view = System.Windows.Data.CollectionViewSource.GetDefaultView(_expenses);
            GridExpenses.ItemsSource = _view;

            // 3) Now set up UI sources (these can raise events)
            InpCategory.ItemsSource = _categories;
            CategoryFilter.ItemsSource = new[] { "All" }.Concat(_categories).ToList();

            // 4) Default selections (these might fire SelectionChanged/TextChanged)
            CategoryFilter.SelectedIndex = 0;
            InpDate.SelectedDate = DateTime.Today;
            InpCategory.SelectedIndex = 0;
        }


        private void Add_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            InpDate.SelectedDate = DateTime.Today;
            InpCategory.SelectedIndex = 0;
            InpDesc.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs(out var date, out var category, out var desc, out var amount))
                return;

            if (GridExpenses.SelectedItem is Expense selected)
            {
                selected.Date = date;
                selected.Category = category;
                selected.Description = desc;
                selected.Amount = amount;
                _view.Refresh();
            }
            else
            {
                _expenses.Add(new Expense
                {
                    Date = date,
                    Category = category,
                    Description = desc,
                    Amount = amount
                });
            }

            ClearForm();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (GridExpenses.SelectedItem is not Expense selected)
            {
                MessageBox.Show("Please select a row to update.", "Manage Expenses");
                return;
            }

            InpDate.SelectedDate = selected.Date;
            InpCategory.SelectedItem = selected.Category;
            InpDesc.Text = selected.Description;
            InpAmount.Text = selected.Amount.ToString("0.##");
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (GridExpenses.SelectedItem is not Expense selected)
            {
                MessageBox.Show("Please select a row to delete.", "Manage Expenses");
                return;
            }

            if (MessageBox.Show("Delete selected expense?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _expenses.Remove(selected);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            GridExpenses.SelectedItem = null;
            InpDate.SelectedDate = null;
            InpCategory.SelectedIndex = 0;
            InpDesc.Clear();
            InpAmount.Clear();
        }

        private bool ValidateInputs(out DateTime date, out string category, out string desc, out decimal amount)
        {
            date = InpDate.SelectedDate ?? DateTime.Today;
            category = (InpCategory.SelectedItem as string) ?? "General";
            desc = InpDesc.Text?.Trim() ?? "";

            if (!decimal.TryParse(InpAmount.Text, out amount) || amount <= 0)
            {
                MessageBox.Show("Enter a valid positive amount.", "Validation");
                return false;
            }
            return true;
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => ApplyFilter();
        private void CategoryFilter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => ApplyFilter();
        private void MonthFilter_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => ApplyFilter();
        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            CategoryFilter.SelectedIndex = 0;
            MonthFilter.SelectedDate = null;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_view == null || !IsLoaded) return;   // <— guard

            var text = (SearchBox.Text ?? "").Trim().ToLowerInvariant();
            var cat = CategoryFilter.SelectedItem as string;
            var selectedMonth = MonthFilter.SelectedDate;

            _view.Filter = o =>
            {
                var e = o as Expense;
                if (e == null) return false;

                bool matchText = string.IsNullOrEmpty(text)
                    || (e.Description ?? "").ToLowerInvariant().Contains(text)
                    || (e.Category ?? "").ToLowerInvariant().Contains(text);

                bool matchCat = string.IsNullOrEmpty(cat) || cat == "All" || e.Category == cat;

                bool matchMonth = !selectedMonth.HasValue
                    || (e.Date.Year == selectedMonth.Value.Year && e.Date.Month == selectedMonth.Value.Month);

                return matchText && matchCat && matchMonth;
            };

            _view.Refresh();
        }



        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"expenses_{DateTime.Now:yyyyMMdd}.csv"
            };
            if (dlg.ShowDialog(this) != true) return;

            using var sw = new StreamWriter(dlg.FileName);
            sw.WriteLine("Date,Category,Description,Amount");
            foreach (Expense ex in _view.Cast<Expense>().ToList())
            {
                var line = $"{ex.Date:yyyy-MM-dd},{Csv(ex.Category)},{Csv(ex.Description)},{ex.Amount:0.00}";
                sw.WriteLine(line);
            }

            MessageBox.Show("Exported successfully.", "Manage Expenses");
        }

        private static string Csv(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";
    }
}
