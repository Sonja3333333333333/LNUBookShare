using Bogus;

using LNUBookShareDAL.Models;

using Microsoft.EntityFrameworkCore;

namespace LNUBookShareConsole
{
    public class DataSeeder
    {
        private readonly LNUBookShareDbContext _dbContext;
        private readonly Faker _faker;

        public DataSeeder(LNUBookShareDbContext dbContext)
        {
            this._dbContext = dbContext;
            this._faker = new Faker("uk");
        }

        public async Task SeedDatabaseAsync(int recordCount)
        {
            Console.WriteLine($"Очищення старих даних... (TRUNCATE RESTART IDENTITY)");
            await this._dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE favorite, book, \"User\", category, faculty, image, emailconfirmation RESTART IDENTITY CASCADE");

            Console.WriteLine($"Генерація {recordCount} Факультетів...");
            var faculties = await this.SeedFacultiesAsync(recordCount);

            Console.WriteLine($"Генерація {recordCount} Категорій...");
            var categories = await this.SeedCategoriesAsync(recordCount);

            Console.WriteLine($"Генерація {recordCount} Зображень...");
            var images = await this.SeedImagesAsync(recordCount);

            await this._dbContext.SaveChangesAsync();
            Console.WriteLine("...Факультети, Категорії та Зображення збережено в БД.");

            Console.WriteLine($"Генерація {recordCount} Користувачів...");
            var users = await this.SeedUsersAsync(recordCount, faculties, images);
            await this._dbContext.SaveChangesAsync();
            Console.WriteLine("...Користувачів збережено в БД.");

            Console.WriteLine($"Генерація {recordCount} Книг...");
            var books = await this.SeedBooksAsync(recordCount, users, categories, images);
            await this._dbContext.SaveChangesAsync();
            Console.WriteLine("...Книги збережено в БД.");

            Console.WriteLine($"Генерація {recordCount} записів 'Уподобане'...");
            await this.SeedFavoritesAsync(recordCount, users, books);

            Console.WriteLine($"Генерація {recordCount} записів 'Підтвердження Email'...");
            await this.SeedEmailConfirmationsAsync(recordCount, users);

            await this._dbContext.SaveChangesAsync();
            Console.WriteLine("...Уподобане та Підтвердження Email збережено в БД.");
        }

        private async Task<List<Faculty>> SeedFacultiesAsync(int count)
        {
            var facultyFaker = new Faker<Faculty>("uk")
                .RuleFor(f => f.Name, fkr => fkr.Commerce.Department() + " факультет");
            var faculties = facultyFaker.Generate(count);
            await this._dbContext.Faculties.AddRangeAsync(faculties);
            return faculties;
        }

        private async Task<List<Category>> SeedCategoriesAsync(int count)
        {
            var categoryFaker = new Faker<Category>("uk")
                .RuleFor(c => c.Name, fkr => fkr.Commerce.Categories(1)[0]);
            var categories = categoryFaker.Generate(count);
            await this._dbContext.Categories.AddRangeAsync(categories);
            return categories;
        }

        private async Task<List<Image>> SeedImagesAsync(int count)
        {
            var imageFaker = new Faker<Image>()
                .RuleFor(i => i.ImagePath, fkr => fkr.Image.PicsumUrl())
                .RuleFor(i => i.ImageType, fkr => fkr.PickRandom(new[] { "book_cover", "avatar" }));
            var images = imageFaker.Generate(count);
            await this._dbContext.Images.AddRangeAsync(images);
            return images;
        }

        private async Task<List<User>> SeedUsersAsync(int count, List<Faculty> faculties, List<Image> images)
        {
            var facultyIds = faculties.Select(f => f.FacultyId).ToList();
            var avatarIds = images.Where(i => i.ImageType == "avatar").Select(i => i.ImageId).ToList();
            if (avatarIds.Count == 0)
            {
                avatarIds = images.Select(i => i.ImageId).ToList();
            }

            var userFaker = new Faker<User>("uk")
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName, "lnu.edu.ua"))
                .RuleFor(u => u.PasswordHash, f => f.Internet.Password(12))
                .RuleFor(u => u.FacultyId, f => f.PickRandom(facultyIds))
                .RuleFor(u => u.AvatarId, f => f.PickRandom(avatarIds))
                .RuleFor(u => u.IsEmailConfirmed, f => f.Random.Bool(0.8f));
            var users = userFaker.Generate(count);
            await this._dbContext.Users.AddRangeAsync(users);
            return users;
        }

        private async Task<List<Book>> SeedBooksAsync(int count, List<User> users, List<Category> categories, List<Image> images)
        {
            var userIds = users.Select(u => u.UserId).ToList();
            var categoryIds = categories.Select(c => c.CategoryId).ToList();
            var coverIds = images.Where(i => i.ImageType == "book_cover").Select(i => i.ImageId).ToList();
            if (coverIds.Count == 0)
            {
                coverIds = images.Select(i => i.ImageId).ToList();
            }

            var bookFaker = new Faker<Book>("uk")
                .RuleFor(b => b.Title, f => f.Commerce.ProductName())
                .RuleFor(b => b.Author, f => f.Name.FullName())
                .RuleFor(b => b.Isbn, f => f.Commerce.Ean13())
                .RuleFor(b => b.Year, f => f.Date.Past(20).Year)
                .RuleFor(b => b.Publisher, f => f.Company.CompanyName())
                .RuleFor(b => b.Language, f => f.PickRandom(new[] { "Українська", "Англійська", "Польська" }))
                .RuleFor(b => b.CategoryId, f => f.PickRandom(categoryIds))
                .RuleFor(b => b.OwnerId, f => f.PickRandom(userIds))
                .RuleFor(b => b.CoverId, f => f.PickRandom(coverIds))
                .RuleFor(b => b.Status, f => f.PickRandom(new[] { "available", "issued" }));
            var books = bookFaker.Generate(count);
            await this._dbContext.Books.AddRangeAsync(books);
            return books;
        }

        private async Task SeedFavoritesAsync(int count, List<User> users, List<Book> books)
        {
            var userIds = users.Select(u => u.UserId).ToList();
            var bookIds = books.Select(b => b.BookId).ToList();

            var favorites = new List<Favorite>();
            var uniqueFavoritePairs = new HashSet<(int, int)>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                int randomUserId;
                int randomBookId;
                do
                {
                    randomUserId = userIds[random.Next(userIds.Count)];
                    randomBookId = bookIds[random.Next(bookIds.Count)];
                }
                while (uniqueFavoritePairs.Contains((randomUserId, randomBookId)));

                uniqueFavoritePairs.Add((randomUserId, randomBookId));
                favorites.Add(new Favorite { UserId = randomUserId, BookId = randomBookId });
            }

            await this._dbContext.Favorites.AddRangeAsync(favorites);
        }

        private async Task SeedEmailConfirmationsAsync(int count, List<User> users)
        {
            var userIds = users.Select(u => u.UserId).ToList();
            var random = new Random();

            var uniqueUserIds = userIds.OrderBy(x => random.Next()).Take(count).ToList();
            var confirmations = new List<Emailconfirmation>();

            foreach (var userId in uniqueUserIds)
            {
                confirmations.Add(new Emailconfirmation
                {
                    UserId = userId,
                    ConfirmationToken = this._faker.Random.Guid().ToString(),
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                });
            }

            await this._dbContext.Emailconfirmations.AddRangeAsync(confirmations);
        }
    }
}