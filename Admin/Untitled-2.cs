public async Task<bool> UpdateProduct(int productId, string productName, decimal price, string imagePath)
{
    try
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            
            string query = @"
                UPDATE products 
                SET product_name = @productName, 
                    price = @price, 
                    image_path = @imagePath
                WHERE id = @productId";
                
            using (var cmd = new NpgsqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@productId", productId);
                cmd.Parameters.AddWithValue("@productName", productName);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@imagePath", imagePath);
                
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