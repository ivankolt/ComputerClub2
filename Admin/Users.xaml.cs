using ComputerClub.BD;
using ComputerClub.Manager;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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



        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = (DataGrid)sender;
            var point = e.GetPosition(grid);

            var hitTestResult = VisualTreeHelper.HitTest(grid, point);
            var cell = FindParent<DataGridCell>(hitTestResult.VisualHit);

            if (cell != null && cell.Column != null && cell.Column.DisplayIndex == 3)
            {
                var row = FindParent<DataGridRow>(cell);
                if (row?.Item is DataRowView dataRow)
                {
                    try
                    {

                        string value = dataRow.Row[3].ToString();
                        Clipboard.SetText(value);
                        MessageBox.Show($"Скопировано: {value}", "Успех",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка копирования: {ex.Message}", "Ошибка",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {

                var row = ItemsControl.ContainerFromElement(dataGrid, e.OriginalSource as DependencyObject) as DataGridRow;
                if (row?.Item is DataRowView dataRow)
                {
                    try
                    {

                        int userId = Convert.ToInt32(dataRow.Row["id"]);

                        DatabaseManager dbManager = new DatabaseManager();
                        var userDetails = dbManager.GetUserDetails(userId);

                        var detailsWindow = new UserDetailsWindow(userDetails);
                        detailsWindow.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var blockWindow = new BlockUserWindow();
            blockWindow.Owner = Application.Current.MainWindow;
            blockWindow.ShowDialog();
            LoadUsers();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var topUpWindow = new TopUpBalanceWindow();
            topUpWindow.Owner = Application.Current.MainWindow;
            topUpWindow.ShowDialog();
            LoadUsers();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                DatabaseManager dbManager = new DatabaseManager();
                DataTable dt = dbManager.GetAllActionUsers();

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                using (var workbook = new ClosedXML.Excel.XLWorkbook())
                {
                
                    var worksheet = workbook.Worksheets.Add("UserActions");

         
                    worksheet.Cell(1, 1).Value = "ID";
                    worksheet.Cell(1, 2).Value = "Action Type";
                    worksheet.Cell(1, 3).Value = "Action Time";
                    worksheet.Cell(1, 4).Value = "User ID";

                    var headerRange = worksheet.Range(1, 1, 1, 4);
                    headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.DarkBlue;
                    headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                    headerRange.SetAutoFilter();

              
                    worksheet.SheetView.FreezeRows(1);

         
                    int row = 2;
                    foreach (DataRow dr in dt.Rows)
                    {
                        worksheet.Cell(row, 1).Value = dr["id"] != DBNull.Value ? dr["id"].ToString() : "";
                        worksheet.Cell(row, 2).Value = dr["action_type"] != DBNull.Value ? dr["action_type"].ToString() : "";
                        worksheet.Cell(row, 3).Value = dr["action_time"] != DBNull.Value ? dr["action_time"].ToString() : "";
                        worksheet.Cell(row, 4).Value = dr["user_id"] != DBNull.Value ? dr["user_id"].ToString() : "";

                        row++;
                    }

                
                    worksheet.Columns().AdjustToContents();

                 
                    string fileName = $"UserActions_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx";
                    string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                    workbook.SaveAs(path);

              
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });

                    MessageBox.Show($"Отчёт успешно сохранён: {path}", "Экспорт завершён", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}