using ComputerClub.BD; 
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using ClosedXML.Excel;
using System.Diagnostics;
using System.IO;

namespace ComputerClub.Admin
{
    /// <summary>
    /// Логика взаимодействия для Payments.xaml
    /// </summary>
    public partial class Payments : Window
    {

        private readonly DatabaseManager _dbManager = new DatabaseManager();


        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var payments = PaymentsHistoryGrid.ItemsSource as IEnumerable<PaymentModel>;
                if (payments == null || !payments.Any())
                {
                    MessageBox.Show("Нет данных для экспорта", "Информация",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Платежи");

                    worksheet.Cell(1, 1).Value = "ID Платежа";
                    worksheet.Cell(1, 2).Value = "Пользователь";
                    worksheet.Cell(1, 3).Value = "Сумма";
                    worksheet.Cell(1, 4).Value = "Тип";
                    worksheet.Cell(1, 5).Value = "Дата платежа";
                    worksheet.Cell(1, 6).Value = "Услуга/Товар";
                    worksheet.Cell(1, 7).Value = "Номер счета";
                    worksheet.Cell(1, 8).Value = "ID Пользователя";

                    var headerRange = worksheet.Range(1, 1, 1, 8);
                    headerRange.Style.Fill.BackgroundColor = XLColor.Red;
                    headerRange.Style.Font.FontColor = XLColor.White;
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                   
                    headerRange.SetAutoFilter();

                    
                    worksheet.SheetView.FreezeRows(1);

                    
                    int row = 2;
                    foreach (var payment in payments)
                    {
                        worksheet.Cell(row, 1).Value = payment.Id;
                        worksheet.Cell(row, 2).Value = payment.Username;
                        worksheet.Cell(row, 3).Value = payment.Amount;
                        worksheet.Cell(row, 4).Value = payment.TypePayment;
                        worksheet.Cell(row, 5).Value = payment.DatePayment;
                        worksheet.Cell(row, 6).Value = payment.ServiceName;
                        worksheet.Cell(row, 7).Value = payment.AccountNumber;
                        worksheet.Cell(row, 8).Value = payment.UserId;
                        row++;
                    }


                    worksheet.Cell(row, 2).Value = "Итого:";
                    worksheet.Cell(row, 3).FormulaA1 = $"=SUM(C2:C{row - 1})";

                    
                    var amountRange = worksheet.Range($"C2:C{row - 1}");
                    var cf = amountRange.AddConditionalFormat().WhenLessThan(0);
                    cf.Font.FontColor = XLColor.Red;

                   
                    worksheet.Columns().AdjustToContents();

                   
                    string fileName = $"Отчет_по_платежам_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx";
                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

                    workbook.SaveAs(path);
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });

                    MessageBox.Show($"Отчёт успешно сохранён: {path}", "Экспорт завершён",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public Payments()
        {
            InitializeComponent();
            this.Loaded += PaymentsWindow_Loaded;
        }

        private void PaymentsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPaymentHistory();
        }

        private void LoadPaymentHistory()
        {
            try
            {
                List<Payment> paymentHistory = _dbManager.GetAllPayments();

                List<PaymentModel> paymentModels = paymentHistory.Select(p => new PaymentModel
                {
                    Id = p.Id,
                    DatePayment = p.DatePayment,
                    Amount = p.Amount,
                    TypePayment = p.TypePayment,
                    ServiceName = p.ServiceName,
                    Username = p.Username,
                    AccountNumber = p.AccountNumber,
                    UserId = p.UserId 
                }).ToList();

                PaymentsHistoryGrid.ItemsSource = paymentModels;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при загрузке истории платежей: {ex.Message}",
                                "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PaymentsHistoryGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedPayment = PaymentsHistoryGrid.SelectedItem as PaymentModel;
            if (selectedPayment != null)
            {
                int userId = selectedPayment.UserId; 
                var receiptWindow = new ReceiptDetailsWindow(selectedPayment.Id, userId);
                receiptWindow.ShowDialog();
            }
        }
    }
}