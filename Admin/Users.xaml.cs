using ComputerClub.BD;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ComputerClub.Admin
{
    public partial class Users : UserControl
    {
        private DataView _dataView;

        public Users()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                DatabaseManager dbManager = new DatabaseManager();
                DataTable users = dbManager.GetUsers();

                if (users != null && users.Rows.Count > 0)
                {
                    _dataView = users.DefaultView;
                    dataGrid.ItemsSource = _dataView;

                    searchBox.TextChanged += SearchBox_TextChanged;
                }
                else
                {
                    MessageBox.Show("Нет данных о пользователях.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_dataView == null) return; 

            string filterText = searchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(filterText))
            {
                ApplyFilter();
            }
            else
            {
                var filters = _dataView.Table.Columns.Cast<DataColumn>()
                    .Where(col => col.DataType == typeof(string))
                    .Select(col => $"[{col.ColumnName}] LIKE '%{filterText}%'");

                string activeClientsFilter = string.Empty;

                if (filterActiveClientsCheckbox.IsChecked == true)
                {
                    activeClientsFilter = "IsActive = True";
                }
       

                if (!string.IsNullOrEmpty(activeClientsFilter))
                {
                    _dataView.RowFilter = $"{string.Join(" OR ", filters)} AND {activeClientsFilter}";  
                }
                else
                {
                    _dataView.RowFilter = string.Join(" OR ", filters);
                }
            }
        }

        private void FilterActiveClients_Checked(object sender, RoutedEventArgs e)
        {
            ApplyFilter(); 
        }

        private void FilterActiveClients_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_dataView == null) return;

            string filterText = searchBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(filterText))
            {
                _dataView.RowFilter = filterActiveClientsCheckbox.IsChecked == true ? "activity = True" : string.Empty;
            }
            else
            {
                var filters = _dataView.Table.Columns.Cast<DataColumn>()
                    .Where(col => col.DataType == typeof(string))
                    .Select(col => $"[{col.ColumnName}] LIKE '%{filterText}%'");

                string activeClientsFilter = string.Empty;

                if (filterActiveClientsCheckbox.IsChecked == true)
                {
                    activeClientsFilter = "IsActive = True";
                }

                if (!string.IsNullOrEmpty(activeClientsFilter))
                {
                    _dataView.RowFilter = $"{string.Join(" OR ", filters)} AND {activeClientsFilter}";
                }
                else
                {
                    _dataView.RowFilter = string.Join(" OR ", filters);
                }
            }
        }
    }
}