using Npgsql;
class DatabaseTests
{
private static string connectionString = 
    "Host=ep-small-mouse-a9sfbziw-pooler.gwc.azure.neon.tech;" +
    "Port=5432;" +
    "Username=neondb_owner;" +
    "Password=npg_Am9BxPvKDZN6;" +
    "Database=neondb;" +
    "SSL Mode=Require;" +
    "Trust Server Certificate=true;";


    public static void ConnectionTest()
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        if (connection.State == System.Data.ConnectionState.Open)
            Console.WriteLine("ConnectionTest Success");
        else
            Console.WriteLine("ConnectionTest Unsuccessful");
    }

    public static void UsersExistTest()
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM \"User\";", connection);
        var count = (long)cmd.ExecuteScalar();
        if (count > 0) Console.WriteLine("UsersExistTest Success");
        else Console.WriteLine("UsersExistTest Unsuccessful");
    }

    public static void InsertAndReadUserTest()
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        // 0. Видаляємо попередніх тестових користувачів
        var cleanupCmd = new NpgsqlCommand(
            "DELETE FROM \"User\" WHERE email = 'test.user@lnu.edu.ua';", connection);
        cleanupCmd.ExecuteNonQuery();

        // 1. Вставка тестового користувача
        var insertCmd = new NpgsqlCommand(
            "INSERT INTO \"User\" (first_name, last_name, email, password_hash, faculty_id) " +
            "VALUES ('Test', 'User', 'test.user@lnu.edu.ua', 'hash', 1) RETURNING user_id;", connection);
        var userId = (int)insertCmd.ExecuteScalar();

        // 2. Зчитування
        var selectCmd = new NpgsqlCommand(
            $"SELECT first_name, last_name FROM \"User\" WHERE user_id = {userId};", connection);
        using var reader = selectCmd.ExecuteReader();
        if (reader.Read())
            Console.WriteLine("InsertAndReadUserTest  Success");
        else
            Console.WriteLine("InsertAndReadUserTest  Unsuccessful");

        reader.Close();

        // 3. Видалення тестового користувача після перевірки
        var deleteCmd = new NpgsqlCommand(
            $"DELETE FROM \"User\" WHERE user_id = {userId};", connection);
        deleteCmd.ExecuteNonQuery();
    }




}
