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
using ComputerClub.Admin;

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
                        decimal balance;
                        using (var cmd = new NpgsqlCommand(
                            "SELECT balance FROM users WHERE id = @userId",
                            connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            balance = Convert.ToDecimal(cmd.ExecuteScalar());
                        }

                        if (balance < amount) return false;

                        using (var cmd = new NpgsqlCommand(
                            "UPDATE users SET balance = balance - @amount WHERE id = @userId",
                            connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@amount", amount);
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new NpgsqlCommand(
                            "UPDATE bookings SET status = 'Подтверждённый' WHERE id = @bookingId",
                            connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@bookingId", bookingId);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new NpgsqlCommand(@"
                    INSERT INTO payments 
                        (amount, type_payment, service_name, user_id, account_number)
                    VALUES 
                        (@amount, 'бронирование'::payment_type_enum, 
                         'Бронирование ПК'::service_name_enum, @userId, 
                         (SELECT card_number FROM users WHERE id = @userId))",
                            connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@amount", amount);
                            cmd.Parameters.AddWithValue("@userId", userId);
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
        public class UserInfo
        {
            public string CardNumber { get; set; }
            public decimal Balance { get; set; }
        }

        public UserInfo GetUserInfo(int userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT card_number, balance 
            FROM users 
            WHERE id = @userId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserInfo
                            {
                                CardNumber = reader.GetString(0),
                                Balance = reader.GetDecimal(1)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public string AuthenticateUser(string username, string password)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

              
                var checkBlockedQuery = @"
            SELECT COUNT(*) 
            FROM blocked_users 
            WHERE user_id = (SELECT id FROM users WHERE username = @username)";

                using (var checkBlockedCmd = new NpgsqlCommand(checkBlockedQuery, connection))
                {
                    checkBlockedCmd.Parameters.AddWithValue("@username", username);
                    int blockedCount = Convert.ToInt32(checkBlockedCmd.ExecuteScalar());

                    if (blockedCount > 0)
                    {
                        MessageBox.Show("Ваш аккаунт заблокирован. Обратитесь к администратору.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }

         
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

        public List<Product> GetProducts()
        {
            var products = new List<Product>();
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT 
                id, 
                product_name, 
                price, 
                quantity_store, 
                product_type, 
                picture 
            FROM products order by id";
                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0), // Убедитесь, что Id корректно считывается
                            ProductName = reader.GetString(1),
                            Price = reader.GetDecimal(2),
                            QuantityStore = reader.GetInt32(3),
                            Category = reader.GetString(4),
                            Picture = reader.GetString(5)
                        });
                    }
                }
            }
            return products;
        }

        public bool AddReceivedProduct(int productId, int receivedCount)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            INSERT INTO received_products (products_id, received_count)
            VALUES (@product_id, @received_count)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@product_id", productId);
                    command.Parameters.AddWithValue("@received_count", receivedCount);
                    try
                    {
                        command.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
        }

        public DataTable GetUsers()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT id, username, email, card_number, balance, is_active, person_id FROM public.users where role = 'user';";
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, connection))
                {
                    DataTable usersTable = new DataTable();
                    adapter.Fill(usersTable);
                    return usersTable;
                }
            }
        }

        public void AddProduct(Product product)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            INSERT INTO public.products(
                product_name, price, quantity_store, product_type, picture)
            VALUES 
                (@name, @price, @quantity, @type::product_type_enum, @picture)";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", product.ProductName);
                    cmd.Parameters.AddWithValue("@price", product.Price);
                    cmd.Parameters.AddWithValue("@quantity", product.QuantityStore);
                    cmd.Parameters.AddWithValue("@type", product.Category); 
                    cmd.Parameters.AddWithValue("@picture", product.Picture);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // DatabaseManager.cs
        public List<Product> GetAvailableProducts()
        {
            return GetProducts().Where(p => p.QuantityStore > 0).ToList();
        }

        public Product GetProductById(int productId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM products WHERE id = @id";
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", productId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Product
                            {
                                Id = reader.GetInt32(0),
                                ProductName = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                QuantityStore = reader.GetInt32(3),
                                Category = reader.GetString(4),
                                Picture = reader.GetString(5)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public int CreateOrder(int userId, decimal totalAmount)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            INSERT INTO orders (user_id, total_amount)
            VALUES (@userId, @totalAmount)
            RETURNING id";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@totalAmount", totalAmount);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void AddProductToOrder (int orderId, int productId, int quantity, NpgsqlTransaction transaction = null)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            INSERT INTO orders_products (order_id, product_id, quantity)
            VALUES (@orderId, @productId, @quantity)";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.Parameters.AddWithValue("@productId", productId);
                    cmd.Parameters.AddWithValue("@quantity", quantity);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int CreateOrder(int userId, decimal totalAmount, NpgsqlTransaction transaction)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "INSERT INTO orders (user_id, total_amount) VALUES (@userId, @totalAmount) RETURNING id";
                using (var cmd = new NpgsqlCommand(query, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@totalAmount", totalAmount);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public DataTable GetOrderHistory()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT 
                received_products.id, 
                received_products.date_receipt, 
                received_products.received_count, 
                products.product_name 
            FROM received_products 
            INNER JOIN products 
            ON received_products.products_id = products.id";
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, connection))
                {
                    DataTable historyTable = new DataTable();
                    adapter.Fill(historyTable);
                    return historyTable;
                }
            }
        }

        public bool ConfirmPurchase(int userId, decimal totalAmount, List<CartItem> cartItems)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
     
                    connection.Open();
              
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var balanceQuery = "SELECT balance FROM users WHERE id = @userId";
                        using (var cmd = new NpgsqlCommand(balanceQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            var currentBalance = Convert.ToDecimal(cmd.ExecuteScalar());
                            if (currentBalance < totalAmount)
                                return false;
                        }

                        var updateBalanceQuery = "UPDATE users SET balance = balance - @amount WHERE id = @userId";
                        using (var cmd = new NpgsqlCommand(updateBalanceQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@amount", totalAmount);
                            cmd.ExecuteNonQuery();
                        }

                        var orderId = CreateOrder(userId, totalAmount, transaction);

                        foreach (var item in cartItems)
                        {
                            AddProductToOrder(orderId, item.ProductId, item.Quantity, transaction);
                        }

                        var paymentQuery = @"
                    INSERT INTO payments (amount, type_payment, service_name, user_id)
                    VALUES (@amount, 'покупка'::payment_type_enum, 'Товар'::service_name_enum, @userId)";
                        using (var paymentCmd = new NpgsqlCommand(paymentQuery, connection, transaction))
                        {
                            paymentCmd.Parameters.AddWithValue("@amount", totalAmount);
                            paymentCmd.Parameters.AddWithValue("@userId", userId);
                            paymentCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка: {ex.Message}");
                        return false;
                    }
                }
            }
        }
        public UserFullInfo GetFullUserInfo(int userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT p.first_name, p.last_name, p.phone_number, 
                   u.email, u.username, u.card_number
            FROM users u
            JOIN persons p ON u.person_id = p.id
            WHERE u.id = @userId";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserFullInfo
                            {
                                FirstName = reader.GetString(0),
                                LastName = reader.GetString(1),
                                PhoneNumber = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                Email = reader.GetString(3),
                                Username = reader.GetString(4),
                                CardNumber = reader.GetString(5)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public List<Payment> GetUserPayments(int userId)
        {
            var payments = new List<Payment>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT date_payment, amount, type_payment, service_name
            FROM payments
            WHERE user_id = @userId
            ORDER BY date_payment DESC";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            payments.Add(new Payment
                            {
                                DatePayment = reader.GetDateTime(0),
                                Amount = reader.GetDecimal(1),
                                TypePayment = reader.GetString(2),
                                ServiceName = reader.GetString(3)
                            });
                        }
                    }
                }
            }
            return payments;
        }

        public bool UpdatePassword(int userId, string newPassword)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    var query = "UPDATE users SET password_hash = @hash WHERE id = @userId";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@hash", hashedPassword);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public DataTable GetUserByCardNumber(string cardNumber)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT id, username, card_number, balance FROM users WHERE card_number = @cardNumber";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@cardNumber", cardNumber);
                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public bool BlockUser(int userId, string reason, string cardNumber)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (NpgsqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string insertQuery = @"INSERT INTO blocked_users (reason, block_date, user_id)
                                     VALUES (@reason, CURRENT_DATE, @userId)";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(insertQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@reason", reason);
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.ExecuteNonQuery();
                        }

                        string updateQuery = "UPDATE users SET is_active = false WHERE card_number = @cardNumber";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(updateQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@cardNumber", cardNumber);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public bool UpdateUserBalance(int userId, decimal amount, string cardNumber)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string query = "UPDATE users SET balance = balance + @amount WHERE id = @userId AND card_number = @cardNumber";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@cardNumber", cardNumber);
                    return cmd.ExecuteNonQuery() > 0;
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
public class UserFullInfo
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string CardNumber { get; set; }
}

public class Payment
{
    public DateTime DatePayment { get; set; }
    public decimal Amount { get; set; }
    public string TypePayment { get; set; }
    public string ServiceName { get; set; }
}