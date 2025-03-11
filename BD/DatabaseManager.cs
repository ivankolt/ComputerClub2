using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Npgsql;
using BCrypt;
using ComputerClub.Users;

namespace ComputerClub.BD
{
    public class DatabaseManager
    {
        private readonly string _connectionString = "Host=localhost;Port=5432;Username=postgres;Password=12345;Database=ComputerClub";

       
        public bool RegisterUser(string username, string password, string email, string firstName, string lastName, string phoneNumber)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(phoneNumber))
            {
                MessageBox.Show("Все поля должны быть заполнены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM Users
                    WHERE username = @username OR email = @email";

                using (var command = new NpgsqlCommand(checkQuery, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@email", email);

                    int existingUserCount = Convert.ToInt32(command.ExecuteScalar());
                    if (existingUserCount > 0)
                    {
                        MessageBox.Show("Пользователь с таким логином или email уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
              string cardNumber = GenerateCardNumber();

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                var insertPersonQuery = @"
                    INSERT INTO Persons (first_name, last_name, phone_number)
                    VALUES (@first_name, @last_name, @phone_number)
                    RETURNING id";

                using (var command = new NpgsqlCommand(insertPersonQuery, connection))
                {
                    command.Parameters.AddWithValue("@first_name", firstName);
                    command.Parameters.AddWithValue("@last_name", lastName);
                    command.Parameters.AddWithValue("@phone_number", phoneNumber);

                        int personId = Convert.ToInt32(command.ExecuteScalar());

                    // Шаг 5: Добавление записи в Users
                    var insertUserQuery = @"
                        INSERT INTO Users (username, password_hash, role, email, card_number, balance, is_active, person_id)
                        VALUES (@username, @password_hash, 'user', @email, @card_number, 0.00, FALSE, @person_id)";

                    using (var userCommand = new NpgsqlCommand(insertUserQuery, connection))
                    {
                        userCommand.Parameters.AddWithValue("@username", username);
                        userCommand.Parameters.AddWithValue("@password_hash", hashedPassword);
                        userCommand.Parameters.AddWithValue("@email", email);
                        userCommand.Parameters.AddWithValue("@card_number", cardNumber);
                        userCommand.Parameters.AddWithValue("@person_id", personId);

                        try
                        {
                            userCommand.ExecuteNonQuery();
                            MessageBox.Show("Регистрация успешно завершена!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }
                }
            }
        }


        private string GenerateCardNumber()
        {
            Random random = new Random();
            string prefix = "22003344";
            string suffix = random.Next(10000000, 99999999).ToString();
            return prefix + suffix;
        }


        public int BookPC(int pcId, int userId, DateTime startTime, DateTime endTime)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (!IsPCAvailable(pcId, startTime, endTime))
                            throw new Exception("ПК уже забронирован");

                        decimal pricePerHour = GetPCPrice(pcId);
                        decimal hours = (decimal)(endTime - startTime).TotalHours;
                        decimal totalAmount = pricePerHour * hours;

                        // Вставка в bookings (БЕЗ qty_hour)
                        var bookingQuery = @"
                    INSERT INTO bookings (user_id, start_time, end_time, status, total_amount)
                    VALUES (@user_id, @start_time, @end_time, 'Ожидаемый', @total_amount)
                    RETURNING id;";

                        int bookingId;
                        using (var cmd = new NpgsqlCommand(bookingQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@user_id", userId);
                            cmd.Parameters.AddWithValue("@start_time", startTime);
                            cmd.Parameters.AddWithValue("@end_time", endTime);
                            cmd.Parameters.AddWithValue("@total_amount", totalAmount);
                            bookingId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Вставка qty_hour в pc_bookings
                        var pcBookingQuery = @"
                    INSERT INTO pc_bookings (booking_id, pc_id, qty_hour)
                    VALUES (@booking_id, @pc_id, @qty_hour)";

                        using (var pcCmd = new NpgsqlCommand(pcBookingQuery, connection, transaction))
                        {
                            pcCmd.Parameters.AddWithValue("@booking_id", bookingId);
                            pcCmd.Parameters.AddWithValue("@pc_id", pcId);
                            pcCmd.Parameters.AddWithValue("@qty_hour", hours);
                            pcCmd.ExecuteNonQuery();
                        }

                        UpdatePCActivity(pcId, true, connection, transaction);
                        transaction.Commit();
                        return bookingId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        public bool ConfirmBooking(int bookingId, int userId, decimal amount)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Проверка баланса
                        var balanceQuery = "SELECT balance FROM users WHERE id = @userId;";
                        decimal balance;
                        using (var cmd = new NpgsqlCommand(balanceQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            balance = Convert.ToDecimal(cmd.ExecuteScalar());
                        }

                        if (balance < amount)
                            return false;

                        // Списание средств
                        var updateBalanceQuery = "UPDATE users SET balance = balance - @amount WHERE id = @userId;";
                        using (var cmd = new NpgsqlCommand(updateBalanceQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@amount", amount);
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.ExecuteNonQuery();
                        }

                        // Обновление статуса бронирования
                        var updateBookingQuery = "UPDATE bookings SET status = 'Подтверждённый' WHERE id = @bookingId;";
                        using (var cmd = new NpgsqlCommand(updateBookingQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@bookingId", bookingId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public void CancelBooking(int bookingId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE bookings SET status = 'Отменённый' WHERE id = @bookingId;";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@bookingId", bookingId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private decimal GetPCPrice(int pcId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT price_per_hour FROM PC WHERE id = @pc_id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pc_id", pcId);
                    return Convert.ToDecimal(command.ExecuteScalar());
                }
            }
        }

        private bool IsPCAvailable(int pcId, DateTime startTime, DateTime endTime)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT COUNT(*) 
            FROM bookings b
            JOIN pc_bookings pb ON b.id = pb.booking_id
            WHERE pb.pc_id = @pc_id
            AND NOT (b.end_time <= @start_time OR b.start_time >= @end_time)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pc_id", pcId);
                    command.Parameters.AddWithValue("@start_time", startTime);
                    command.Parameters.AddWithValue("@end_time", endTime);

                    return Convert.ToInt32(command.ExecuteScalar()) == 0;
                }
            }
        }

        private void UpdatePCActivity(int pcId, bool activity, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            var query = "UPDATE pc SET activity = @activity WHERE id = @pc_id";
            using (var cmd = new NpgsqlCommand(query, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@activity", activity);
                cmd.Parameters.AddWithValue("@pc_id", pcId);
                cmd.ExecuteNonQuery();
            }
        }

        public string AuthenticateUser(string username, string password)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT id, role, password_hash 
            FROM Users 
            WHERE username = @username";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userId = reader.GetInt32(0);
                            string role = reader.GetString(1);
                            string storedHash = reader.GetString(2);

                            if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                            {
                                CurrentUser.Instance = new CurrentUser
                                {
                                    Id = userId,
                                    Role = role
                                };
                                return role; // Возвращаем роль
                            }
                        }
                    }
                }
                return null;
            }
        }
        private string GetRoleByUsername(string username)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
            SELECT role 
            FROM Users 
            WHERE username = @username";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(0);
                        }
                    }
                }
            }
            return null;
        }


        public List<PC> GetPCs()
        {
            var pcs = new List<PC>();
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT 
                PC.id, 
                zone, 
                price_per_hour, 
                video_card, 
                CPU, 
                monitor, 
                keyboard, 
                monitor_hertz, 
                activity 
            FROM PC 
            JOIN Equipment ON PC.equipment_id = Equipment.id";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pcs.Add(new PC(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetDecimal(2),
                            reader.GetString(3),
                            reader.GetString(4),
                            reader.GetString(5),
                            reader.GetString(6),
                            reader.GetInt32(7),
                            reader.GetBoolean(8)
                        ));
                    }
                }
            }
            return pcs;
        }

        public DataTable GetUsers()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT id, username, role, email, card_number, balance, is_active, person_id FROM public.users;";
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, connection))
                {
                    DataTable usersTable = new DataTable();
                    adapter.Fill(usersTable);
                    return usersTable;
                }
            }
        }

        public DataTable GetPayments()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT id, username, role, email, card_number, balance, is_active, person_id FROM public.users;";
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, connection))
                {
                    DataTable usersTable = new DataTable();
                    adapter.Fill(usersTable);
                    return usersTable;
                }
            }
        }
    }
}
