using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComputerClub.BD;

namespace ComputerClub.Manager
{
    public partial class UsersControl : UserControl
    {
        private readonly DatabaseManager _db = new DatabaseManager();
        private ObservableCollection<UserInfo> _users = new ObservableCollection<UserInfo>();

        public UsersControl()
        {
            InitializeComponent();
            try
            {
                LoadUsers();
                dgUsers.ItemsSource = _users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации UsersControl: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RefreshData()
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                _users.Clear();
                var usersList = _db.GetAllUsers();
                foreach (var user in usersList)
                {
                    _users.Add(user);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgUsers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgUsers.SelectedItem is UserInfo selectedUser)
            {
                try
                {
                    var userDetails = _db.GetUserDetails(selectedUser.Id);
                    var detailsWindow = new UserDetailsWindow(userDetails);
                    detailsWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
        private void btnUnblockUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is UserInfo selectedUser)
            {
                if (selectedUser.Status != "Заблокирован")
                {
                    MessageBox.Show("Выбранный пользователь не заблокирован", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите разблокировать пользователя {selectedUser.Username}?",
                    "Подтверждение разблокировки",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = _db.UnblockUser(selectedUser.Id);
                        
                        if (success)
                        {
                            MessageBox.Show("Пользователь успешно разблокирован", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadUsers();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось разблокировать пользователя. Возможно, он уже разблокирован.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при разблокировке пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для разблокировки", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; }


    }
}
