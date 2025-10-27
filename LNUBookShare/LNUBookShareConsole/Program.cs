using LNUBookShareDAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Bogus;
using System.Linq;
using Npgsql.EntityFrameworkCore.PostgreSQL;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("--- Запуск Генератора Тестових Даних (30 записів/таблиця) ---");
Console.ResetColor();

// 1. Налаштовуємо "Господаря" (Host)
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        string connectionString = context.Configuration.GetConnectionString("DefaultConnection");
        // ----- НОВИЙ, ПРАВИЛЬНИЙ КОД -----
        services.AddDbContext<LNUBookShareDbContext>(options =>
    // Додаємо логування, щоб бачити SQL-запити в консолі
    options.UseNpgsql(connectionString).LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
);

    }).Build();

// 2. Викликаємо нашу головну функцію
await RunDataSeeder(host.Services);

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("\n--- Успіх! Базу даних заповнено 30-ма записами для кожної таблиці. ---");
Console.ResetColor();
Console.WriteLine("Натисніть Enter для виходу.");
Console.ReadLine();


// ========== ГОЛОВНА ЛОГІКА ЗАПУСКУ ==========

static async Task RunDataSeeder(IServiceProvider services)
{
    // Отримуємо DbContext
    using (var scope = services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<LNUBookShareDbContext>();
        await SeedDatabaseAsync(dbContext);
    }
}

// ========== МЕТОД ДЛЯ ЗАПОВНЕННЯ ДАНИМИ (SEEDER) ==========

static async Task SeedDatabaseAsync(LNUBookShareDbContext dbContext)
{
    const int recordCount = 30; // Кількість записів для кожної таблиці

    // Спочатку все очистимо
    Console.WriteLine($"Очищення старих даних... (TRUNCATE RESTART IDENTITY)");
    await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE favorite, book, \"User\", category, faculty, image, emailconfirmation RESTART IDENTITY CASCADE");

    // --- 1. Генеруємо незалежні дані (Факультети, Категорії, Зображення) ---

    Console.WriteLine($"Генерація {recordCount} Факультетів...");
    var facultyFaker = new Faker<Faculty>("uk") // "uk" для українських даних
        .RuleFor(f => f.Name, fkr => fkr.Commerce.Department() + " факультет");
    var faculties = facultyFaker.Generate(recordCount);
    await dbContext.Faculties.AddRangeAsync(faculties);

    Console.WriteLine($"Генерація {recordCount} Категорій...");
    var categoryFaker = new Faker<Category>("uk")
        .RuleFor(c => c.Name, fkr => fkr.Commerce.Categories(1)[0]);
    var categories = categoryFaker.Generate(recordCount);
    await dbContext.Categories.AddRangeAsync(categories);

    Console.WriteLine($"Генерація {recordCount} Зображень (аватари та обкладинки)...");
    var imageFaker = new Faker<Image>()
        .RuleFor(i => i.ImagePath, fkr => fkr.Image.PicsumUrl())
        .RuleFor(i => i.ImageType, fkr => fkr.PickRandom(new[] { "book_cover", "avatar" })); // Наші ENUM типи
    var images = imageFaker.Generate(recordCount);
    await dbContext.Images.AddRangeAsync(images);

    // ЗБЕРІГАЄМО, щоб отримати ID для наступних кроків
    await dbContext.SaveChangesAsync();
    Console.WriteLine("...Факультети, Категорії та Зображення збережено в БД.");

    // --- 2. Генеруємо Користувачів (залежать від Факультетів та Зображень) ---

    // Отримуємо ID, які щойно створили
    var facultyIds = faculties.Select(f => f.FacultyId).ToList();
    var avatarIds = images.Where(i => i.ImageType == "avatar").Select(i => i.ImageId).ToList();
    // (Якщо раптом аватари не згенерувалися, беремо будь-які зображення)
    if (avatarIds.Count == 0) avatarIds = images.Select(i => i.ImageId).ToList();

    Console.WriteLine($"Генерація {recordCount} Користувачів...");
    var userFaker = new Faker<User>("uk")
        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
        .RuleFor(u => u.LastName, f => f.Name.LastName())
        .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName, "lnu.edu.ua")) // Корпоративна пошта
        .RuleFor(u => u.PasswordHash, f => f.Internet.Password(12)) // Просто рядок, не реальний хеш
        .RuleFor(u => u.FacultyId, f => f.PickRandom(facultyIds)) // Випадковий ID факультету
        .RuleFor(u => u.AvatarId, f => f.PickRandom(avatarIds)) // Випадковий ID аватара
        .RuleFor(u => u.IsEmailConfirmed, f => f.Random.Bool(0.8f)); // 80% користувачів підтверджені
    var users = userFaker.Generate(recordCount);
    await dbContext.Users.AddRangeAsync(users);

    // ЗБЕРІГАЄМО, щоб отримати ID Користувачів
    await dbContext.SaveChangesAsync();
    Console.WriteLine("...Користувачів збережено в БД.");

    // --- 3. Генеруємо Книги (залежать від Користувачів, Категорій, Зображень) ---

    var userIds = users.Select(u => u.UserId).ToList();
    var categoryIds = categories.Select(c => c.CategoryId).ToList();
    var coverIds = images.Where(i => i.ImageType == "book_cover").Select(i => i.ImageId).ToList();
    if (coverIds.Count == 0) coverIds = images.Select(i => i.ImageId).ToList();

    Console.WriteLine($"Генерація {recordCount} Книг...");
    var bookFaker = new Faker<Book>("uk")
        .RuleFor(b => b.Title, f => f.Commerce.ProductName()) // Назви продуктів схожі на назви книг
        .RuleFor(b => b.Author, f => f.Name.FullName())
        .RuleFor(b => b.Isbn, f => f.Commerce.Ean13())
        .RuleFor(b => b.Year, f => f.Date.Past(20).Year) // Рік за останні 20 років
        .RuleFor(b => b.Publisher, f => f.Company.CompanyName())
        .RuleFor(b => b.Language, f => f.PickRandom(new[] { "Українська", "Англійська", "Польська" }))
        .RuleFor(b => b.CategoryId, f => f.PickRandom(categoryIds)) // Випадкова категорія
        .RuleFor(b => b.OwnerId, f => f.PickRandom(userIds)) // Випадковий власник
        .RuleFor(b => b.CoverId, f => f.PickRandom(coverIds)) // Випадкова обкладинка
        .RuleFor(b => b.Status, f => f.PickRandom(new[] { "available", "issued" })); 
    var books = bookFaker.Generate(recordCount);
    await dbContext.Books.AddRangeAsync(books);

    // ЗБЕРІГАЄМО, щоб отримати ID Книг
    await dbContext.SaveChangesAsync();
    Console.WriteLine("...Книги збережено в БД.");

    // --- 4. Генеруємо Залежні таблиці (Уподобане, Підтвердження Email) ---

    var bookIds = books.Select(b => b.BookId).ToList();

    Console.WriteLine($"Генерація {recordCount} записів 'Уподобане' (з перевіркою на унікальність)...");
    var favorites = new List<Favorite>();
    var uniqueFavoritePairs = new HashSet<(int, int)>(); // Для уникнення дублікатів (User-Book)
    var random = new Random();

    for (int i = 0; i < recordCount; i++)
    {
        int randomUserId;
        int randomBookId;

        // Шукаємо унікальну пару User-Book, щоб не порушити обмеження UNIQUE
        do
        {
            randomUserId = userIds[random.Next(userIds.Count)];
            randomBookId = bookIds[random.Next(bookIds.Count)];
        }
        while (uniqueFavoritePairs.Contains((randomUserId, randomBookId)));

        uniqueFavoritePairs.Add((randomUserId, randomBookId));
        favorites.Add(new Favorite { UserId = randomUserId, BookId = randomBookId });
    }
    await dbContext.Favorites.AddRangeAsync(favorites);

    Console.WriteLine($"Генерація {recordCount} записів 'Підтвердження Email' (унікальні користувачі)...");
    // Беремо 30 унікальних ID користувачів
    var uniqueUserIdsForConfirmation = userIds.OrderBy(x => random.Next()).Take(recordCount).ToList();
    var confirmations = new List<Emailconfirmation>();
    var tokenFaker = new Faker();

    foreach (var userId in uniqueUserIdsForConfirmation)
    {
        confirmations.Add(new Emailconfirmation
        {
            UserId = userId,
            ConfirmationToken = tokenFaker.Random.Guid().ToString(), // Випадковий токен
            ExpiresAt = DateTime.UtcNow.AddHours(24) // Діє 24 години
        });
    }
    await dbContext.Emailconfirmations.AddRangeAsync(confirmations);

    // --- ФІНАЛЬНЕ ЗБЕРЕЖЕННЯ ---
    await dbContext.SaveChangesAsync();
    Console.WriteLine("...Уподобане та Підтвердження Email збережено в БД.");
}

