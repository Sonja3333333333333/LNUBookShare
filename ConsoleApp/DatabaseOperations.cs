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

    public static void GenerateFaculties(int count = 30)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        for (int i = 1; i <= count; i++)
        {
            var name = $"Faculty {i}";
            var cmd = new NpgsqlCommand("INSERT INTO Faculty (name) VALUES (@name) ON CONFLICT DO NOTHING;", connection);
            cmd.Parameters.AddWithValue("name", name);
            cmd.ExecuteNonQuery();
        }

        Console.WriteLine($"{count} Faculties generated!");
    }

    public static void GenerateCategories(int count = 30)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        for (int i = 1; i <= count; i++)
        {
            var name = $"Category {i}";
            var cmd = new NpgsqlCommand("INSERT INTO Category (name) VALUES (@name) ON CONFLICT DO NOTHING;", connection);
            cmd.Parameters.AddWithValue("name", name);
            cmd.ExecuteNonQuery();
        }

        Console.WriteLine($"{count} Categories generated!");
    }

    public static void GenerateImages(int count = 30)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        var types = new[] { "book_cover", "avatar" };

        for (int i = 1; i <= count; i++)
        {
            var path = $"/images/img{i}.jpg";

            // Чередуємо між book_cover та avatar
            var type = types[i % types.Length];

            // Явне приведення тексту до ENUM у SQL
            var cmd = new NpgsqlCommand(@"
                INSERT INTO Image (image_path, image_type) 
                VALUES (@path, @type::image_type_enum);", connection);

            cmd.Parameters.AddWithValue("path", path);
            cmd.Parameters.AddWithValue("type", type);

            cmd.ExecuteNonQuery();
        }

        Console.WriteLine($"{count} Images generated!");
    }


    public static void GenerateBooks(int count = 30)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        for (int i = 1; i <= count; i++)
        {
            var title = $"Book Title {i}";
            var author = $"Author {i}";
            var isbn = $"ISBN-{1000 + i}";
            var year = 2000 + (i % 23);
            var publisher = $"Publisher {i}";
            var language = "English";
            var categoryId = (i % 30) + 1;
            var ownerId = (i % 30) + 1;
            var status = (i % 2 == 0) ? "available" : "issued";
            var coverId = (i % 30) + 1;

            var cmd = new NpgsqlCommand(@"
                INSERT INTO Book 
                    (title, author, isbn, year, publisher, language, category_id, owner_id, status, cover_id) 
                VALUES 
                    (@title, @author, @isbn, @year, @publisher, @language,
                    @category, @owner, @status::book_status_enum, @cover);", connection);

            cmd.Parameters.AddWithValue("title", title);
            cmd.Parameters.AddWithValue("author", author);
            cmd.Parameters.AddWithValue("isbn", isbn);
            cmd.Parameters.AddWithValue("year", year);
            cmd.Parameters.AddWithValue("publisher", publisher);
            cmd.Parameters.AddWithValue("language", language);
            cmd.Parameters.AddWithValue("category", categoryId);
            cmd.Parameters.AddWithValue("owner", ownerId);
            cmd.Parameters.AddWithValue("status", status);  // кастинг у SQL через ::book_status_enum
            cmd.Parameters.AddWithValue("cover", coverId);

            cmd.ExecuteNonQuery();
        }

        Console.WriteLine($"{count} Books generated!");
    }

     public static void GenerateFavorites(int count = 30)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        for (int i = 1; i <= count; i++)
        {
            var userId = (i % 30) + 1;
            var bookId = (i % 30) + 1;

            var cmd = new NpgsqlCommand(@"
                INSERT INTO Favorite (user_id, book_id) 
                VALUES (@user, @book) 
                ON CONFLICT DO NOTHING;", connection);

            cmd.Parameters.AddWithValue("user", userId);
            cmd.Parameters.AddWithValue("book", bookId);

            cmd.ExecuteNonQuery();
        }

        Console.WriteLine($"{count} Favorites generated!");
    }

    public static void GenerateEmailConfirmations(int count = 30)
    {
        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        for (int i = 1; i <= count; i++)
        {
            var userId = (i % 30) + 1;
            var token = Guid.NewGuid().ToString("N");
            var createdAt = DateTime.Now;
            var expiresAt = createdAt.AddDays(1);

            var cmd = new NpgsqlCommand(@"
                INSERT INTO EmailConfirmation (user_id, confirmation_token, created_at, expires_at) 
                VALUES (@user, @token, @created, @expires)
                ON CONFLICT (user_id) DO NOTHING;", connection);

            cmd.Parameters.AddWithValue("user", userId);
            cmd.Parameters.AddWithValue("token", token);
            cmd.Parameters.AddWithValue("created", createdAt);
            cmd.Parameters.AddWithValue("expires", expiresAt);

            cmd.ExecuteNonQuery();
        }

        Console.WriteLine($"{count} EmailConfirmations generated!");
    }
}
