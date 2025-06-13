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
using System.Security.Policy;
using ComputerClub.Manager;
using NpgsqlTypes;
using System.Windows.Controls;

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

                var checkQuery = "SELECT check_user_exists(@username, @email)";

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

                var insertPersonQuery = "SELECT insert_person(@first_name, @last_name, @phone_number)";

                using (var command = new NpgsqlCommand(insertPersonQuery, connection))
                {
                    command.Parameters.AddWithValue("@first_name", firstName);
                    command.Parameters.AddWithValue("@last_name", lastName);
                    command.Parameters.AddWithValue("@phone_number", phoneNumber);

                    int personId = Convert.ToInt32(command.ExecuteScalar());


                    var registerUserQuery = "CALL register_user(@username, @password_hash, @email, @card_number, @person_id, NULL, NULL)";

                    using (var userCommand = new NpgsqlCommand(registerUserQuery, connection))
                    {
                        userCommand.Parameters.AddWithValue("@username", username);
                        userCommand.Parameters.AddWithValue("@password_hash", hashedPassword);
                        userCommand.Parameters.AddWithValue("@email", email);
                        userCommand.Parameters.AddWithValue("@card_number", cardNumber);
                        userCommand.Parameters.AddWithValue("@person_id", personId);


                        var successParam = new NpgsqlParameter("p_success", NpgsqlDbType.Boolean)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var messageParam = new NpgsqlParameter("p_message", NpgsqlDbType.Varchar)
                        {
                            Direction = ParameterDirection.Output,
                            Size = 200
                        };
                        userCommand.Parameters.Add(successParam);
                        userCommand.Parameters.Add(messageParam);

                        try
                        {
                            userCommand.ExecuteNonQuery();

                            bool success = (bool)successParam.Value;
                            string message = (string)messageParam.Value;

                            MessageBox.Show(message, success ? "Успешно" : "Ошибка",
                                          MessageBoxButton.OK,
                                          success ? MessageBoxImage.Information : MessageBoxImage.Error);
                            return success;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при вызове процедуры: {ex.Message}",
                                           "Ошибка",
                                           MessageBoxButton.OK,
                                           MessageBoxImage.Error);
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
                        using (var cmd = new NpgsqlCommand("SELECT book_pc(@pc_id, @user_id, @start_time, @end_time)", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@pc_id", pcId);
                            cmd.Parameters.AddWithValue("@user_id", userId);
                            cmd.Parameters.AddWithValue("@start_time", startTime);
                            cmd.Parameters.AddWithValue("@end_time", endTime);

                            var result = cmd.ExecuteScalar();
                            transaction.Commit();
                            return Convert.ToInt32(result);
                        }
                    }
                    catch (PostgresException ex) when (ex.SqlState == "P0001")
                    {
                        transaction.Rollback();
                 
                        throw new Exception(ex.MessageText); 
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Ошибка при бронировании ПК", ex);
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

                        if (balance < amount)
                        {
                            using (var cmd = new NpgsqlCommand(
                                "UPDATE bookings SET status = 'Отменённый' WHERE id = @bookingId",
                                connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@bookingId", bookingId);
                                cmd.ExecuteNonQuery();
                            }
                            transaction.Commit();
                            return false;
                        }

                        using (var cmd = new NpgsqlCommand(
                            "UPDATE users SET balance = balance - @amount WHERE id = @userId",
                            connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@amount", amount);
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new NpgsqlCommand(
                            "UPDATE bookings SET status = 'Ожидаемый' WHERE id = @bookingId",
                            connection, transaction))
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

        public async Task<bool> UpdateProduct(int productId, string productName, decimal price, string imagePath)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string imageFileName = System.IO.Path.GetFileName(imagePath);

                    string query = @"
                UPDATE products 
                SET product_name = @productName, 
                    price = @price, 
                    picture = @imageFileName
                WHERE id = @productId";

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@productId", productId);
                        cmd.Parameters.AddWithValue("@productName", productName);
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@imageFileName", imageFileName);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex.Message}");
                throw;
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
                FROM blocked_users bu
                JOIN users u ON bu.user_id = u.id 
                WHERE u.username = @username;";
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

                                SetPostgresRole(role);

                                return role; 
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void SetPostgresRole(string role)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = $"SET ROLE {role};";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        private void SetPostgresRole(NpgsqlConnection connection, string role)
        {
            var query = $"SET ROLE {role};";
            using (var command = new NpgsqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
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

        public void IsActive(int id_user)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"update users
                        set is_active = true
                        where id = @id_users";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_users", id_user);
                    command.ExecuteNonQuery();
                }    

            }
        }

        public (bool, string) BlockedOrNot(int id_user)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"SELECT id, reason FROM blocked_users 
                    WHERE user_id = @users_id";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@users_id", id_user);

               
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) 
                        {
                           
                            string reason = reader["reason"]?.ToString() ?? "";
                            return (true, reason);
                        }
                        else
                        {
                            return (false, ""); 
                        }
                    }
                }
            }
        }
        public void IsNotActive(int id_user)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"update users
                        set is_active = false
                        where id = @id_users";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_users", id_user);
                    command.ExecuteNonQuery();
                }

            }
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
            JOIN Equipment ON PC.equipment_id = Equipment.id order by PC.id";
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


        public async Task ExecuteStoredProcedure(string query, params object[] parameters)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", parameters[0]);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Product>> GetProducts()
        {
            var products = new List<Product>();
            NpgsqlConnection connection = null;
            NpgsqlCommand command = null;
            NpgsqlDataReader reader = null;

            try
            {
                connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT
                        id,
                        product_name,
                        price,
                        quantity_store,
                        product_type,
                        picture,
                        deleted
                    FROM products
                    ORDER BY id";

                command = new NpgsqlCommand(query, connection);
                reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    products.Add(new Product
                    {
                        Id = reader.GetInt32(0),
                        ProductName = reader.GetString(1),
                        Price = reader.GetDecimal(2),
                        QuantityStore = reader.GetInt32(3),
                        Category = reader.GetString(4),
                        Picture = reader.GetString(5),
                        Deleted = reader.GetBoolean(6)
                    });
                }
            }
            finally
            {
                reader?.Dispose();
                command?.Dispose();
                connection?.Dispose();
            }

            return products;
        }

        public async Task<List<Product>> GetProducts1()
        {
            var products = new List<Product>();
            NpgsqlConnection connection = null;
            NpgsqlCommand command = null;
            NpgsqlDataReader reader = null;

            try
            {
                connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT
                        id,
                        product_name,
                        price,
                        quantity_store,
                        product_type,
                        picture
                    FROM products
                    WHERE deleted = false
                    ORDER BY id";

                command = new NpgsqlCommand(query, connection);
                reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    products.Add(new Product
                    {
                        Id = reader.GetInt32(0),
                        ProductName = reader.GetString(1),
                        Price = reader.GetDecimal(2),
                        QuantityStore = reader.GetInt32(3),
                        Category = reader.GetString(4),
                        Picture = reader.GetString(5)
                    });
                }
            }
            finally
            {
                reader?.Dispose();
                command?.Dispose();
                connection?.Dispose();
            }

            return products;
        }

        public bool AddReceivedProduct(int productId, int receivedCount)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            INSERT INTO received_products (product_id, received_count)
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
                var query = "SELECT * FROM active_users";
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

        public async Task<List<Product>> GetAvailableProducts()
        {
            var products = await GetProducts1();
            return products.Where(p => p.QuantityStore > 0).ToList();
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
            ON received_products.product_id = products.id";
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
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }

                        foreach (var item in cartItems)
                        {
                            var checkProductCmd = new NpgsqlCommand(
                                "SELECT quantity_store FROM products WHERE id = @productId",
                                connection, transaction);
                            checkProductCmd.Parameters.AddWithValue("@productId", item.ProductId);
                            var currentQuantity = Convert.ToInt32(checkProductCmd.ExecuteScalar());
                            if (currentQuantity < item.Quantity)
                            {
                                transaction.Rollback();
                                MessageBox.Show($"Товара {item.ProductName} недостаточно на складе!");
                                return false;
                            }
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
                    
                            var updateProductCmd = new NpgsqlCommand(
                                "UPDATE products SET quantity_store = quantity_store - @quantity WHERE id = @productId",
                                connection, transaction);
                            updateProductCmd.Parameters.AddWithValue("@quantity", item.Quantity);
                            updateProductCmd.Parameters.AddWithValue("@productId", item.ProductId);
                            updateProductCmd.ExecuteNonQuery();

                       
                            AddProductToOrder(orderId, item.ProductId, item.Quantity, transaction);
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

        public class ShiftInfo
        {
            public int Id { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public TimeSpan Duration => EndTime.HasValue ?
                EndTime.Value - StartTime :
                DateTime.Now - StartTime;
        }

        public ShiftInfo GetCurrentShift(int employeeId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand(@"
            SELECT id, start_time, end_time 
            FROM shifts 
            WHERE employee_id = @empId 
            AND (end_time IS NULL OR end_time > NOW() - INTERVAL '12 hours')
            ORDER BY start_time DESC
            LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@empId", employeeId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new ShiftInfo
                        {
                            Id = reader.GetInt32(0),
                            StartTime = reader.GetDateTime(1),
                            EndTime = reader.IsDBNull(2) ? null : (DateTime?)reader.GetDateTime(2)
                        };
                    }
                }
            }
            return null;
        }

        public void StartShift(int employeeId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand(@"
            INSERT INTO shifts(start_time, employee_id)
            VALUES (NOW(), @empId)", conn);
                cmd.Parameters.AddWithValue("@empId", employeeId);
                cmd.ExecuteNonQuery();
            }
        }

        public void EndShift(int shiftId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand(@"
            UPDATE shifts SET end_time = NOW()
            WHERE id = @shiftId", conn);
                cmd.Parameters.AddWithValue("@shiftId", shiftId);
                cmd.ExecuteNonQuery();
            }
        }


        public void CreateReceipt(int paymentId, string receiptData)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            INSERT INTO receipts (payment_id, payment)
            VALUES (@paymentId, @receiptData::jsonb)";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@paymentId", paymentId);
                    cmd.Parameters.AddWithValue("@receiptData", receiptData);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public EmployeeInfo GetEmployeeInfo(int employeeId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand(@"
            SELECT p.first_name || ' ' || p.last_name, 
               pos.position_name, 
               u.email, 
               p.phone_number
            FROM employees e
            JOIN persons p ON e.person_id = p.id
            JOIN posts pos ON e.position_id = pos.id
            JOIN users u ON u.person_id = p.id
            WHERE e.id =  @empId", conn);
                cmd.Parameters.AddWithValue("@empId", employeeId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new EmployeeInfo
                        {
                            FullName = reader.GetString(0),
                            Position = reader.GetString(1),
                            Email = reader.GetString(2),
                            Phone = reader.IsDBNull(3) ? "Не указан" : reader.GetString(3)
                        };
                    }
                }
            }
            return null;
        }

        public int CurrentEmployee(int userId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT e.id AS employee_id
            FROM public.users u
            JOIN public.employees e ON u.person_id = e.person_id
            WHERE u.id = @user_id";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user_id", userId);
                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        throw new Exception("Employee not found for this user");
                    }
                }
            }
        }
        public Payment GetPaymentById(int paymentId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT p.id, p.date_payment, p.amount, p.type_payment, p.service_name, 
                   p.account_number, p.user_id, u.username
            FROM payments p
            LEFT JOIN users u ON p.user_id = u.id
            WHERE p.id = @paymentId";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@paymentId", paymentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Payment
                            {
                                Id = reader.GetInt32(0),
                                DatePayment = reader.GetDateTime(1),
                                Amount = reader.GetDecimal(2),
                                TypePayment = reader.GetString(3),
                                ServiceName = reader.GetString(4),
                                AccountNumber = reader.GetString(5),
                                UserId = reader.GetInt32(6),
                                Username = reader.IsDBNull(7) ? "Гость" : reader.GetString(7)
                            };
                        }
                        return null;
                    }
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

        public List<Payment> GetAllPayments()
{
    var payments = new List<Payment>();

    using (var connection = new NpgsqlConnection(_connectionString))
    {
        connection.Open();

        var query = @"
            SELECT p.id, p.date_payment, p.amount, p.type_payment, p.service_name, 
                   u.username, u.card_number, p.user_id 
            FROM payments p 
            LEFT JOIN users u ON p.user_id = u.id 
            ORDER BY p.date_payment DESC";

        using (var cmd = new NpgsqlCommand(query, connection))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                payments.Add(new Payment
                {
                    Id = reader.GetInt32(0),
                    DatePayment = reader.GetDateTime(1),
                    Amount = reader.GetDecimal(2),
                    TypePayment = reader.GetString(3),
                    ServiceName = reader.GetString(4),
                    Username = reader.IsDBNull(5) ? "Гость" : reader.GetString(5),
                    AccountNumber = reader.GetString(6),
                    UserId = reader.GetInt32(7)
                });
            }
        }
    }

    return payments;
}

        public IEnumerable<Order> GetOrdersByStatus(string status)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand(
                    "SELECT * FROM orders WHERE status = @status::order_status_enum order by order_date desc", conn);
                cmd.Parameters.AddWithValue("@status", status);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new Order
                        {
                            Id = reader.GetInt32(0),
                            OrderDate = reader.GetDateTime(1),
                            TotalAmount = reader.GetDecimal(2),
                            UserId = reader.GetInt32(3),
                            Status = reader.GetString(4)
                        };
                    }
                }
            }
        }

        public void UpdateOrderStatus(int orderId, string newStatus)
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand(
                        "UPDATE orders SET status = @status::order_status_enum WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("@status", newStatus);
                    cmd.Parameters.AddWithValue("@id", orderId);
                    cmd.ExecuteNonQuery();
                }
            }

            public void CreatePayment(Payment payment)
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand(
                        @"INSERT INTO payments 
                (amount, type_payment, service_name, user_id, date_payment)
                VALUES (@amount, @type::payment_type_enum, @service::service_name_enum, @userId, @date)", conn);

                    cmd.Parameters.AddWithValue("@amount", payment.Amount);
                    cmd.Parameters.AddWithValue("@type", payment.TypePayment);
                    cmd.Parameters.AddWithValue("@service", payment.ServiceName);
                    cmd.Parameters.AddWithValue("@userId", payment.UserId);
                    cmd.Parameters.AddWithValue("@date", payment.DatePayment);

                    cmd.ExecuteNonQuery();
                }
            }
        public int CreatePaymentAndGetId(Payment payment)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {        
                connection.Open();
                SetPostgresRole(connection, CurrentUser.Instance.Role);
                var query = @"
            INSERT INTO payments 
                (amount, type_payment, service_name, user_id, date_payment)
            VALUES 
                (@amount, @type_payment::payment_type_enum, @service_name::service_name_enum, @userId, @datePayment)
            RETURNING id";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@amount", payment.Amount);
                    cmd.Parameters.AddWithValue("@type_payment", payment.TypePayment);
                    cmd.Parameters.AddWithValue("@service_name", payment.ServiceName);
                    cmd.Parameters.AddWithValue("@userId", payment.UserId);
                    cmd.Parameters.AddWithValue("@datePayment", payment.DatePayment);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void CancelOrderWithTransaction(int orderId, int userId, decimal amount)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                SetPostgresRole(connection, CurrentUser.Instance.Role);

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new NpgsqlCommand("cancel_order_with_transaction", connection, transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("order_id", orderId);
                            cmd.Parameters.AddWithValue("user_id", userId);
                            cmd.Parameters.AddWithValue("amount", amount);

                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Неожиданная ошибка: {CurrentUser.Instance.Role} {ex.Message}");
                    }
                }
            }
        }
        public IEnumerable<Booking> GetBookingsByStatus(string status)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand(
                    "SELECT * FROM bookings WHERE status = @status::booking_status_enum", conn);
                cmd.Parameters.AddWithValue("@status", status);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new Booking
                        {
                            Id = reader.GetInt32(0),
                            StartTime = reader.GetDateTime(1),
                            EndTime = reader.GetDateTime(2),
                            Status = reader.GetString(3),
                            TotalAmount = reader.GetDecimal(4),
                            UserId = reader.GetInt32(5)
                        };
                    }
                }
            }
        }

        public void InsertUserAction(string actionType, int userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("CALL public.insert_user_action(@actionType::action_type_enum, @userId)", connection))
                {
                    command.Parameters.AddWithValue("actionType", actionType);
                    command.Parameters.AddWithValue("userId", userId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public DataTable GetAllActionUsers()
        {
            DataTable dt = new DataTable();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(
                    "SELECT id, action_type, action_time, user_id FROM public.user_actions;", conn))
                {
                    using (var adapter = new NpgsqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }


        public void UpdateBookingStatus(int bookingId, string newStatus)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand(
                    "UPDATE bookings SET status = @status::booking_status_enum WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@status", newStatus);
                cmd.Parameters.AddWithValue("@id", bookingId);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdatePCStatusForExpiredBookings()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                   
                        var updateBookingsQuery = @"
                    UPDATE bookings
                    SET status = 'Завершённый'
                    WHERE end_time < NOW()
                    AND status NOT IN ('Завершённый', 'Отменённый');";
                        using (var cmdBookings = new NpgsqlCommand(updateBookingsQuery, connection, transaction))
                        {
                            cmdBookings.ExecuteNonQuery();
                        }

                 
                        var updatePCsQuery = @"
                    UPDATE pc
                    SET activity = false
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM pc_bookings
                        JOIN bookings ON pc_bookings.booking_id = bookings.id
                        WHERE pc_bookings.pc_id = pc.id
                        AND bookings.end_time >= NOW()
                        AND bookings.status = 'Подтверждённый'
                    );";
                        using (var cmdPCs = new NpgsqlCommand(updatePCsQuery, connection, transaction))
                        {
                            cmdPCs.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public DataTable GetOrderDetails(int orderId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var query = "SELECT * FROM order_details_view WHERE order_id = @orderId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    var adapter = new NpgsqlDataAdapter(cmd);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public UserFullInfo GetUserFullInfo(int userId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var query = "SELECT * FROM user_full_info WHERE user_id = @userId";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserFullInfo
                            {
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                PhoneNumber = reader.GetString(3),
                                Email = reader.GetString(4),
                                Username = reader.GetString(5),
                                CardNumber = reader.GetString(6)
                            };
                        }
                    }
                }
            }
            return null;
        }
        public decimal GetPaymentsTotal(string serviceType, DateTime startDate, DateTime endDate)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand(
                    @"SELECT COALESCE(SUM(amount), 0) 
            FROM payments 
            WHERE service_name = @service::service_name_enum
            AND date_payment >= @startDate
            AND date_payment < @endDate", conn);

                cmd.Parameters.AddWithValue("@service", serviceType);
                cmd.Parameters.AddWithValue("@startDate", startDate);
                cmd.Parameters.AddWithValue("@endDate", endDate);

                return (decimal)cmd.ExecuteScalar();
            }
        }

        public DataTable GetBookingDetails(int bookingId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var query = @"
            SELECT 
                pc.id AS ПК,
                pc.zone AS Зона,
                pb.qty_hour AS Часы,
                pc.price_per_hour AS Цена_час,
                (pb.qty_hour * pc.price_per_hour) AS Сумма
            FROM pc_bookings pb
            JOIN pc ON pb.pc_id = pc.id
            WHERE pb.booking_id = @bookingId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@bookingId", bookingId);
                    var adapter = new NpgsqlDataAdapter(cmd);
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public Receipt GetReceiptByPaymentId(int paymentId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var command = new NpgsqlCommand("SELECT id, created_at, payment, payment_id FROM receipts WHERE payment_id = @paymentId", conn))
                {
                    command.Parameters.AddWithValue("@paymentId", paymentId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Receipt
                            {
                                Id = reader.GetInt32(0),
                                CreatedAt = reader.GetDateTime(1),
                                PaymentJson = reader.GetString(2),
                                PaymentId = reader.GetInt32(3)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public List<ReceiptItem> GetReceiptItems(int receiptId)
        {
            var items = new List<ReceiptItem>();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var command = new NpgsqlCommand("SELECT payment FROM receipts WHERE id = @receiptId", conn))
                {
                    command.Parameters.AddWithValue("@receiptId", receiptId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var paymentJson = reader.GetString(0);

                            try
                            {
                                var jsonDoc = Newtonsoft.Json.Linq.JObject.Parse(paymentJson);
                                var itemsArray = jsonDoc["items"] as Newtonsoft.Json.Linq.JArray;

                                if (itemsArray != null)
                                {
                                    foreach (var item in itemsArray)
                                    {
                                        items.Add(new ReceiptItem
                                        {
                                            Name = item["name"]?.ToString() ?? "Неизвестный товар",
                                            Quantity = item["quantity"] != null ? Convert.ToInt32(item["quantity"]) : 1,
                                            Price = (item["price"] != null ? Convert.ToDecimal(item["price"]) : 0).ToString("C"),
                                            Total = (item["total"] != null ? Convert.ToDecimal(item["total"]) : 0).ToString("C")
                                        });
                                    }
                                }
                                else
                                {
                                    var paymentType = jsonDoc["type"]?.ToString() ?? "Услуга";
                                    var amount = jsonDoc["amount"] != null ? Convert.ToDecimal(jsonDoc["amount"]) : 0;
                                    var serviceName = jsonDoc["serviceName"]?.ToString() ?? "Неизвестная услуга";

                                    items.Add(new ReceiptItem
                                    {
                                        Name = serviceName,
                                        Quantity = 1,
                                        Price = amount.ToString("C"),
                                        Total = amount.ToString("C")
                                    });
                                }
                            }
                            catch
                            {
                                items.Add(new ReceiptItem
                                {
                                    Name = "Платеж",
                                    Quantity = 1,
                                    Price = "0.00 ₽",
                                    Total = "0.00 ₽"
                                });
                            }
                        }
                    }
                }
            }

            return items;
        }

        public string GetEmployeeNameByPaymentId(int paymentId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                using (var command = new NpgsqlCommand(
                    @"SELECT e.name 
              FROM employees e 
              JOIN payments p ON e.id = p.employee_id 
              WHERE p.id = @paymentId", conn))
                {
                    command.Parameters.AddWithValue("@paymentId", paymentId);
                    var result = command.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        public List<Equipment> GetAllEquipment()
        {
            var equipmentList = new List<Equipment>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"SELECT * FROM equipment_info";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        equipmentList.Add(new Manager.Equipment
                        {
                            Id = reader.GetInt32(0),
                            VideoCard = reader.GetString(1),
                            CPU = reader.GetString(2),
                            Monitor = reader.GetString(3),
                            Keyboard = reader.GetString(4),
                            MonitorHertz = reader.GetInt32(5)
                        });
                    }
                }
            }
            return equipmentList;
        }

        public List<string> GetPCZones()
        {
            var zones = new List<string>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT unnest(enum_range(NULL::pc_zone_enum))::text as zone";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        zones.Add(reader.GetString(0));
                    }
                }
            }
            return zones;
        }

        public bool AddNewPC(decimal pricePerHour, string zone, int equipmentId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var query = @"
                    INSERT INTO pc (price_per_hour, zone, equipment_id, activity)
                    VALUES (@pricePerHour, @zone::pc_zone_enum, @equipmentId, false)";

                        using (var cmd = new NpgsqlCommand(query, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@pricePerHour", pricePerHour);
                            cmd.Parameters.AddWithValue("@zone", zone);
                            cmd.Parameters.AddWithValue("@equipmentId", equipmentId);
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

        public List<Manager.PCInfo> GetAllPCs()
        {
            var pcList = new List<Manager.PCInfo>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT p.id, p.zone, p.price_per_hour
            FROM pc p
            JOIN equipment e ON p.equipment_id = e.id
            ORDER BY p.id";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pcList.Add(new Manager.PCInfo
                        {
                            Id = reader.GetInt32(0),
                            Zone = reader.GetString(1),
                            PricePerHour = reader.GetDecimal(2)
                        });
                    }
                }
            }
            return pcList;
        }

        public bool DeletePC(int pcId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var checkQuery = @"
                    SELECT COUNT(*) 
                    FROM pc_bookings pb
                    JOIN bookings b ON pb.booking_id = b.id
                    WHERE pb.pc_id = @pcId
                    AND b.status IN ('Ожидаемый', 'Подтверждённый')";

                        using (var cmd = new NpgsqlCommand(checkQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@pcId", pcId);
                            int activeBookings = Convert.ToInt32(cmd.ExecuteScalar());

                            if (activeBookings > 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
                        }

                        var deleteQuery = "DELETE FROM pc WHERE id = @pcId";
                        using (var cmd = new NpgsqlCommand(deleteQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@pcId", pcId);
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
      

        public bool IsCardNumberUnique(string cardNumber)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT COUNT(*) FROM users WHERE card_number = @cardNumber";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@cardNumber", cardNumber);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count == 0; 
                }
            }
        }

        public List<Manager.EmployeeInfo> GetAllEmployees()
        {
            var employeesList = new List<Manager.EmployeeInfo>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT e.id, p.first_name, p.last_name, p.phone_number, 
                   po.position_name, e.hire_date
            FROM employees e
            JOIN persons p ON e.person_id = p.id
            JOIN posts po ON e.position_id = po.id
            ORDER BY e.id";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        employeesList.Add(new Manager.EmployeeInfo
                        {
                            Id = reader.GetInt32(0),
                            FullName = $"{reader.GetString(1)} {reader.GetString(2)}",
                            PhoneNumber = reader.GetString(3),
                            Position = reader.GetString(4),
                            HireDate = reader.GetDateTime(5)
                        });
                    }
                }
            }
            return employeesList;
        }

        public Manager.EmployeeFullInfo GetEmployeeDetails(int employeeId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT p.first_name, p.last_name, p.phone_number, 
                   e.gender, po.position_name, po.salary, e.hire_date,
                   e.passport_series, e.passport_number
            FROM employees e
            JOIN persons p ON e.person_id = p.id
            JOIN posts po ON e.position_id = po.id
            WHERE e.id = @employeeId";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@employeeId", employeeId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Manager.EmployeeFullInfo
                            {
                                Id = employeeId,
                                FirstName = reader.GetString(0),
                                LastName = reader.GetString(1),
                                PhoneNumber = reader.GetString(2),
                                Gender = reader.GetString(3),
                                Position = reader.GetString(4),
                                Salary = reader.GetDecimal(5),
                                HireDate = reader.GetDateTime(6),
                                PassportSeries = reader.GetString(7),
                                PassportNumber = reader.GetString(8)
                            };
                        }
                        else
                        {
                            throw new Exception("Сотрудник не найден");
                        }
                    }
                }
            }
        }

        public List<Manager.Position> GetAllPositions()
        {
            var positions = new List<Manager.Position>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT id, position_name, salary FROM posts ORDER BY id";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        positions.Add(new Manager.Position
                        {
                            Id = reader.GetInt32(0),
                            PositionName = reader.GetString(1),
                            Salary = reader.GetDecimal(2)
                        });
                    }
                }
            }
            return positions;
        }

        public List<Manager.UserInfo> GetAllUsers()
        {
            var usersList = new List<Manager.UserInfo>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT u.id, u.username, p.first_name, p.last_name, u.email, u.balance, CASE WHEN bu.user_id IS NOT NULL THEN 'Заблокирован' ELSE 'Активен' END as status
            FROM users u
            JOIN persons p ON u.person_id = p.id
            LEFT JOIN blocked_users bu ON u.id = bu.user_id
            WHERE u.role = 'users'
            ORDER BY u.id";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usersList.Add(new Manager.UserInfo
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            FullName = $"{reader.GetString(2)} {reader.GetString(3)}",
                            Email = reader.GetString(4),
                            Balance = reader.GetDecimal(5),
                            Status = reader.GetString(6)
                        });
                    }
                }
            }
            return usersList;
        }

      

        public Manager.UserFullInfo GetUserDetails(int userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var userInfo = new Manager.UserFullInfo();
                var query = @"            SELECT p.first_name, p.last_name, p.phone_number, 
                   u.username, u.email, u.card_number, u.balance,
                   CASE WHEN bu.id IS NULL THEN false ELSE true END as is_blocked
            FROM users u
            JOIN persons p ON u.person_id = p.id
            LEFT JOIN blocked_users bu ON u.id = bu.user_id
            WHERE u.id = @userId";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userInfo.Id = userId;
                            userInfo.FirstName = reader.GetString(0);
                            userInfo.LastName = reader.GetString(1);
                            userInfo.PhoneNumber = reader.GetString(2);
                            userInfo.Username = reader.GetString(3);
                            userInfo.Email = reader.GetString(4);
                            userInfo.CardNumber = reader.GetString(5);
                            userInfo.Balance = reader.GetDecimal(6);
                            userInfo.IsBlocked = reader.GetBoolean(7);
                        }
                        else
                        {
                            throw new Exception("Пользователь не найден");
                        }
                    }
                }

          
                if (userInfo.IsBlocked)
                {
                    var blockQuery = @"
                SELECT reason, block_date
                FROM blocked_users
                WHERE user_id = @userId";

                    using (var cmd = new NpgsqlCommand(blockQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userInfo.BlockInfo = new Manager.BlockInfo
                                {
                                    Reason = reader.GetString(0),
                                    BlockDate = reader.GetDateTime(1)
                                };
                            }
                        }
                    }
                }

                return userInfo;
            }
        }

        public bool UnblockUser(int userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                      
                        var deleteBlockQuery = "DELETE FROM blocked_users WHERE user_id = @userId";
                        using (var cmd = new NpgsqlCommand(deleteBlockQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            int rowsAffected = cmd.ExecuteNonQuery();

                           
                            if (rowsAffected == 0)
                            {
                                transaction.Rollback();
                                return false;
                            }
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

        public bool AddNewEmployee(Manager.NewEmployeeData employeeData)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                
                        int personId;
                        var personQuery = @"
                    INSERT INTO persons (first_name, last_name, phone_number)
                    VALUES (@firstName, @lastName, @phoneNumber)
                    RETURNING id";

                        using (var cmd = new NpgsqlCommand(personQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@firstName", employeeData.FirstName);
                            cmd.Parameters.AddWithValue("@lastName", employeeData.LastName);
                            cmd.Parameters.AddWithValue("@phoneNumber", employeeData.PhoneNumber);
                            personId = (int)cmd.ExecuteScalar();
                        }

                 
                        int employeeId;
                        var employeeQuery = @"
                    INSERT INTO employees (passport_series, passport_number, gender, person_id, position_id, hire_date)
                    VALUES (@passportSeries, @passportNumber, @gender, @personId, @positionId, @hireDate)
                    RETURNING id";

                        using (var cmd = new NpgsqlCommand(employeeQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@passportSeries", employeeData.PassportSeries);
                            cmd.Parameters.AddWithValue("@passportNumber", employeeData.PassportNumber);
                            cmd.Parameters.AddWithValue("@gender", employeeData.Gender);
                            cmd.Parameters.AddWithValue("@personId", personId);
                            cmd.Parameters.AddWithValue("@positionId", employeeData.PositionId);
                            cmd.Parameters.AddWithValue("@hireDate", employeeData.HireDate);
                            employeeId = (int)cmd.ExecuteScalar();
                        }

                   
                        var userQuery = @"
                    INSERT INTO users (username, password_hash, role, email, card_number, balance, is_active, person_id)
                    VALUES (@username, @passwordHash, 'admin', @email, @cardNumber, 0, true, @personId)";

                        using (var cmd = new NpgsqlCommand(userQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@username", employeeData.Username);
                            cmd.Parameters.AddWithValue("@passwordHash", BCrypt.Net.BCrypt.HashPassword(employeeData.Password));
                            cmd.Parameters.AddWithValue("@email", employeeData.Email);
                            cmd.Parameters.AddWithValue("@cardNumber", employeeData.CardNumber);
                            cmd.Parameters.AddWithValue("@personId", personId);
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

        public bool FireEmployee(int employeeId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                   
                        int personId;
                        var getPersonIdQuery = "SELECT person_id FROM employees WHERE id = @employeeId";
                        using (var cmd = new NpgsqlCommand(getPersonIdQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@employeeId", employeeId);
                            var result = cmd.ExecuteScalar();
                            if (result == null)
                            {
                                transaction.Rollback();
                                return false;
                            }
                            personId = (int)result;
                        }

                     
                        var deleteUserQuery = "DELETE FROM users WHERE person_id = @personId";
                        using (var cmd = new NpgsqlCommand(deleteUserQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@personId", personId);
                            cmd.ExecuteNonQuery();
                        }

             
                        var deleteEmployeeQuery = "DELETE FROM employees WHERE id = @employeeId";
                        using (var cmd = new NpgsqlCommand(deleteEmployeeQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@employeeId", employeeId);
                            cmd.ExecuteNonQuery();
                        }

   
                        var deletePersonQuery = "DELETE FROM persons WHERE id = @personId";
                        using (var cmd = new NpgsqlCommand(deletePersonQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@personId", personId);
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
        public void CancelBookingWithTransaction(int bookingId, int userId, decimal amount)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var paymentCmd = new NpgsqlCommand(
                            @"INSERT INTO payments 
                    (amount, type_payment, service_name, user_id, date_payment)
                    VALUES (@amount, @type::payment_type_enum, @service::service_name_enum, 
                            @userId, @date)",
                            conn, transaction);

                        paymentCmd.Parameters.AddWithValue("@amount", -amount);
                        paymentCmd.Parameters.AddWithValue("@type", "возврат");
                        paymentCmd.Parameters.AddWithValue("@service", "Бронирование ПК");
                        paymentCmd.Parameters.AddWithValue("@userId", userId);
                        paymentCmd.Parameters.AddWithValue("@date", DateTime.Now);
                        paymentCmd.ExecuteNonQuery();

                        var balanceCmd = new NpgsqlCommand(
                            "UPDATE users SET balance = balance + @amount WHERE id = @userId",
                            conn, transaction);
                        balanceCmd.Parameters.AddWithValue("@amount", amount);
                        balanceCmd.Parameters.AddWithValue("@userId", userId);
                        balanceCmd.ExecuteNonQuery();

                        var statusCmd = new NpgsqlCommand(
                            "UPDATE bookings SET status = 'Отменённый'::booking_status_enum WHERE id = @bookingId",
                            conn, transaction);
                        statusCmd.Parameters.AddWithValue("@bookingId", bookingId);
                        statusCmd.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
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


public class EmployeeInfo
{
    public string FullName { get; set; }
    public string Position { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}

public class PaymentDetail 
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string TypePayment { get; set; }
    public DateTime DatePayment { get; set; }
    public string ServiceName { get; set; } 
    public string AccountNumber { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
}
public class Receipt
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PaymentJson { get; set; }
    public int PaymentId { get; set; }
}