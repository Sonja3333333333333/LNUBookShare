using Npgsql;
using System;

class DatabaseOperations
{
private static string connectionString = 
    "Host=ep-small-mouse-a9sfbziw-pooler.gwc.azure.neon.tech;" +
    "Port=5432;" +
    "Username=neondb_owner;" +
    "Password=npg_Am9BxPvKDZN6;" +
    "Database=neondb;" +
    "SSL Mode=Require;" +
    "Trust Server Certificate=true;";

    public static void PrintUsers()
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        var cmd = new NpgsqlCommand("SELECT user_id, first_name, last_name, email FROM \"User\";", connection);

        using var reader = cmd.ExecuteReader();
        Console.WriteLine("Users in database:");
        Console.WriteLine("-------------------");

        while (reader.Read())
        {
            int userId = reader.GetInt32(0);
            string firstName = reader.GetString(1);
            string lastName = reader.GetString(2);
            string email = reader.GetString(3);

            Console.WriteLine($"{userId} | {firstName} {lastName} | {email}");
        }
    }
    public static void AddUser(string firstName, string lastName, string email, string passwordHash, int facultyId)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        var insertCmd = new NpgsqlCommand(
            "INSERT INTO \"User\" (first_name, last_name, email, password_hash, faculty_id) " +
            "VALUES (@firstName, @lastName, @email, @passwordHash, @facultyId);", connection);

        insertCmd.Parameters.AddWithValue("firstName", firstName);
        insertCmd.Parameters.AddWithValue("lastName", lastName);
        insertCmd.Parameters.AddWithValue("email", email);
        insertCmd.Parameters.AddWithValue("passwordHash", passwordHash);
        insertCmd.Parameters.AddWithValue("facultyId", facultyId);

        insertCmd.ExecuteNonQuery(); // просто вставка, нічого не повертає
        Console.WriteLine($"User {firstName} {lastName} added successfully.");
    }


   public static void GenerateNewUsers(int count)
{
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();

    // Очистка тестових користувачів, щоб не було конфліктів
    var cleanupCmd = new NpgsqlCommand(
        "DELETE FROM \"User\" WHERE email LIKE 'testuser%@lnu.edu.ua';", connection);
    cleanupCmd.ExecuteNonQuery();

    for (int i = 1; i <= count; i++)
    {
        // Генеруємо нові унікальні дані
        var firstName = $"TestFirst{i + 100}";   // +100 щоб не перетиналося з існуючими
        var lastName = $"TestLast{i + 100}";
        var email = $"testuser{i + 100}@lnu.edu.ua";
        var passwordHash = $"hash{i + 100}";
        var facultyId = (i % 5) + 1; // циклічно від 1 до 5
        var avatarId = (i % 10) + 1; // довільні id
        var isEmailConfirmed = i % 2 == 0; // чергуємо TRUE/FALSE

        var cmd = new NpgsqlCommand(@"
            INSERT INTO ""User"" 
                (first_name, last_name, email, password_hash, faculty_id, avatar_id, is_email_confirmed)
            VALUES
                (@first, @last, @email, @hash, @faculty, @avatar, @confirmed);", connection);

        cmd.Parameters.AddWithValue("first", firstName);
        cmd.Parameters.AddWithValue("last", lastName);
        cmd.Parameters.AddWithValue("email", email);
        cmd.Parameters.AddWithValue("hash", passwordHash);
        cmd.Parameters.AddWithValue("faculty", facultyId);
        cmd.Parameters.AddWithValue("avatar", avatarId);
        cmd.Parameters.AddWithValue("confirmed", isEmailConfirmed);

        cmd.ExecuteNonQuery();
    }

    Console.WriteLine($"{count} new test users generated!");
}

}
