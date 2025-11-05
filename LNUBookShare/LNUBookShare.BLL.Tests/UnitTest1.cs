using Xunit;
using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Enums;

namespace LNUBookShare.BLL.Tests.Features.Books
{
    public class GetBooksQueryHandlerTests
    {
        private LNUBookShareDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new LNUBookShareDbContext(options);
        }

        [Fact]
        public async Task GetBooks_ReturnsAllBooks()
        {
            using var context = GetContext();

            var category = new Category { Name = "Tech" };
            context.Categories.Add(category);

            var faculty = new Faculty { Name = "CS" };
            context.Faculties.Add(faculty);

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@lnu.edu.ua",
                PasswordHash = "hash",
                FacultyId = 1,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            context.SaveChanges();

            context.Books.Add(new Book
            {
                Title = "Book1",
                Author = "Author1",
                CategoryId = 1,
                OwnerId = 1,
                Status = "available",
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            // ✅ Використовуємо лише DbContext
            var handler = new GetBooksQueryHandler(context);
            var query = new GetBooksQuery { CurrentUserId = 1 };

            var result = await handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetBooks_FiltersByStatus()
        {
            using var context = GetContext();

            var category = new Category { Name = "Tech" };
            context.Categories.Add(category);

            var faculty = new Faculty { Name = "CS" };
            context.Faculties.Add(faculty);

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@lnu.edu.ua",
                PasswordHash = "hash",
                FacultyId = 1,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            context.SaveChanges();

            context.Books.AddRange(
                new Book
                {
                    Title = "Available Book",
                    Author = "Author",
                    CategoryId = 1,
                    OwnerId = 1,
                    Status = "available",
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "Issued Book",
                    Author = "Author",
                    CategoryId = 1,
                    OwnerId = 1,
                    Status = "issued",
                    CreatedAt = DateTime.UtcNow
                }
            );
            context.SaveChanges();

            var handler = new GetBooksQueryHandler(context);
            var query = new GetBooksQuery
            {
                CurrentUserId = 1,
                FilterBy = BookFilterStatus.Available
            };

            var result = await handler.Handle(query, CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task GetBooks_SearchesByTitle()
        {
            using var context = GetContext();

            var category = new Category { Name = "Tech" };
            context.Categories.Add(category);

            var faculty = new Faculty { Name = "CS" };
            context.Faculties.Add(faculty);

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@lnu.edu.ua",
                PasswordHash = "hash",
                FacultyId = 1,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            context.SaveChanges();

            context.Books.Add(new Book
            {
                Title = "Clean Code",
                Author = "Martin",
                CategoryId = 1,
                OwnerId = 1,
                Status = "available",
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var handler = new GetBooksQueryHandler(context);
            var query = new GetBooksQuery
            {
                CurrentUserId = 1,
                SearchTerm = "Clean",
                SearchBy = BookSearchCriteria.Title
            };

            var result = await handler.Handle(query, CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.Items[0].Title.Should().Contain("Clean");
        }
    }
}
