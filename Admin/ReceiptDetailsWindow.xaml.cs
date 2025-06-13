using ComputerClub.Admin;
using ComputerClub.BD;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;

namespace ComputerClub.Admin
{
    public partial class ReceiptDetailsWindow : Window
    {
        private int _paymentId;

        private int _userId;

        public ReceiptDetailsWindow(int paymentId, int userId) 
        {
            InitializeComponent();
            _userId = userId; 
            LoadReceiptDetails(paymentId);
        }

        private void LoadReceiptDetails(int paymentId)
        {
            try
            {
                var dbManager = new DatabaseManager();
                var receipt = dbManager.GetReceiptByPaymentId(paymentId);

                if (receipt != null)
                {
                    dynamic paymentData = Newtonsoft.Json.JsonConvert.DeserializeObject(receipt.PaymentJson);

                    var userInfo = dbManager.GetFullUserInfo(_userId);
                    string customerName = userInfo != null
                        ? $"{userInfo.FirstName} {userInfo.LastName}"
                        : "Неизвестный клиент";

                    var viewModel = new ReceiptViewModel
                    {
                        ReceiptId = receipt.Id,
                        CreatedAt = receipt.CreatedAt,
                        CashierName = paymentData.payment?.employeeName?.ToString() ?? "Не указан",
                        CustomerName = customerName,
                        TotalAmount = paymentData.payment?.amount ?? 0,
                        PaymentMethod = "Наличные",
                        Items = new List<ReceiptItem>()
                    };

                    if (paymentData.items != null)
                    {
                        foreach (var item in paymentData.items)
                        {
                            viewModel.Items.Add(new ReceiptItem
                            {
                                Name = item.name,
                                Quantity = item.quantity,
                                Price = item.price.ToString("C"),
                                Total = item.total.ToString("C")
                            });
                        }
                    }
                    else if (paymentData.payment?.serviceName != null)
                    {
                        viewModel.Items.Add(new ReceiptItem
                        {
                            Name = paymentData.payment.serviceName,
                            Quantity = 1,
                            Price = paymentData.payment.amount.ToString("C"),
                            Total = paymentData.payment.amount.ToString("C")
                        });
                    }

                    this.DataContext = viewModel;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке чека: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetCustomerName(int paymentId)
        {
            return "Клиент"; 
        }

        private void SaveAsImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg|PDF Document|*.pdf",
                    Title = "Сохранить чек",
                    FileName = $"Receipt_{((ReceiptViewModel)DataContext).ReceiptId}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string extension = Path.GetExtension(saveFileDialog.FileName).ToLower();
                    
                    if (extension == ".pdf")
                    {
                        SaveAsPdf(saveFileDialog.FileName);
                    }
                    else
                    {
                        SaveAsImage(saveFileDialog.FileName);
                    }
                    
                    MessageBox.Show("Чек успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении чека: {ex.Message}", "Ошибка", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAsImage(string filePath)
        {
            var buttonsGrid = (Grid)((Grid)Content).Children[2];
            var originalVisibility = buttonsGrid.Visibility;
            buttonsGrid.Visibility = Visibility.Collapsed;

            try
            {
                var contentGrid = (Grid)Content;
                
                RenderTargetBitmap renderTarget = new RenderTargetBitmap(
                    (int)contentGrid.ActualWidth,
                    (int)contentGrid.ActualHeight,
                    96, 96, PixelFormats.Pbgra32);
                
                renderTarget.Render(contentGrid);
                
                BitmapEncoder encoder;
                if (Path.GetExtension(filePath).ToLower() == ".jpg")
                {
                    encoder = new JpegBitmapEncoder();
                }
                else
                {
                    encoder = new PngBitmapEncoder();
                }
                
                encoder.Frames.Add(BitmapFrame.Create(renderTarget));
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
            finally
            {
                buttonsGrid.Visibility = originalVisibility;
            }
        }

        private void SaveAsPdf(string filePath)
        {

            string tempImagePath = Path.Combine(Path.GetTempPath(), "receipt_temp.png");
            SaveAsImage(tempImagePath);
            
            try
            {
                PdfSharp.Pdf.PdfDocument document = new PdfSharp.Pdf.PdfDocument();
                document.Info.Title = $"Receipt {((ReceiptViewModel)DataContext).ReceiptId}";
                
                PdfSharp.Pdf.PdfPage page = document.AddPage();
                
                PdfSharp.Drawing.XGraphics gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
                
                PdfSharp.Drawing.XImage image = PdfSharp.Drawing.XImage.FromFile(tempImagePath);
                
                double pageWidth = page.Width;
                double pageHeight = page.Height;
                double imageWidth = image.PixelWidth;
                double imageHeight = image.PixelHeight;
                
                double scale = Math.Min(pageWidth / imageWidth, pageHeight / imageHeight);
                
                double scaledWidth = imageWidth * scale;
                double scaledHeight = imageHeight * scale;
                
                double x = (pageWidth - scaledWidth) / 2;
                double y = (pageHeight - scaledHeight) / 2;
                
                gfx.DrawImage(image, x, y, scaledWidth+100, scaledHeight+100);
                
                document.Save(filePath);
            }
            finally
            {
                if (File.Exists(tempImagePath))
                {
                    File.Delete(tempImagePath);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

public class ReceiptViewModel
{
    public int ReceiptId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CashierName { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; }
    public List<ReceiptItem> Items { get; set; }
}

public class PaymentData
{
    public string Type { get; set; }
    public decimal Amount { get; set; }
    public string ServiceName { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public List<ReceiptItem> Items { get; set; }
}
