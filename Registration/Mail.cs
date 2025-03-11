using System.Net.Mail;
using System.Net;
using System.Linq;
using System;

namespace ComputerClub.Registration
{
    internal class Mail
    {
        private string smtpServer = "smtp.mail.ru";
        private int smtpPort = 587;
        private string smtpUsername = "game_magnitogorsk@mail.ru";
        private string smtpPassword = "JxjFWdchubdrUcVfphRh";

        /// <summary>
        /// Генерирует случайный пароль.
        /// </summary>
        public string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Отправляет сообщение на указанный email.
        /// </summary>
        public void SendMessage(string email, string generatedPassword)
        {
            using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;

                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(smtpUsername);
                    mailMessage.To.Add(email);
                    mailMessage.Subject = "Подтверждение регистрации";
                    mailMessage.Body = $"Ваш подтверждающий пароль: {generatedPassword}";

                    try
                    {
                        smtpClient.Send(mailMessage);
                        Console.WriteLine("Сообщение успешно отправлено.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка отправки сообщения: {ex.Message}");
                    }
                }
            }
        }
    }
}