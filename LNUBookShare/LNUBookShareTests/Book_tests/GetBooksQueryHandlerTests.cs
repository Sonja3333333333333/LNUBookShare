using Xunit;
using Microsoft.EntityFrameworkCore;
using LNUBookShareDAL.Models;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Enums;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LNUBookShareTests.Book_tests
{
    public class GetBooksQueryHandlerTests
    { 
        private readonly LNUBookShareDbContext _dbContext;
        private DbContextOptions<LNUBookShareDbContext> _options;
        public GetBooksQueryHandlerTests()
        {
            this._options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            this._dbContext = new LNUBookShareDbContext(this._options);

            
            var user = new User
            {
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PasswordHash = "password hash",
                FacultyId = 10
            };
            var category = new Category
            {
                CategoryId = 1,
                Name = "Programming"
            };

            this._dbContext.Users.Add(user);
            this._dbContext.Categories.Add(category);
            this._dbContext.SaveChanges();

           
            this._dbContext.Books.AddRange(new List<Book>
            {
                new Book { BookId = 1, Title = "C# Basics", Author = "John Doe", Status = "available", OwnerId = 1, CategoryId = 1 },
                new Book { BookId = 2, Title = "Advanced C#", Author = "Jane Doe", Status = "issued", OwnerId = 1, CategoryId = 1 },
                new Book { BookId = 3, Title = "Entity Framework", Author = "John Doe", Status = "available", OwnerId = 1, CategoryId = 1 }
            });
            this._dbContext.SaveChanges();
        }

        [Fact]
        public async Task Handle_ShouldReturnAllBooks_WhenNoFiltersApplied()
        {
            
            var handler = new GetBooksQueryHandler(this._dbContext);
            var query = new GetBooksQuery
            {
                CurrentUserId = 1,
                PageNumber = 1,
                PageSize = 10
            };

           
            var result = await handler.Handle(query, CancellationToken.None);

           
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count);
        }

        [Fact]
        public async Task Handle_ShouldFilterByStatus()
        {
            
            var handler = new GetBooksQueryHandler(this._dbContext);
            var query = new GetBooksQuery
            {
                CurrentUserId = 1,
                FilterBy = BookFilterStatus.Available,
                PageNumber = 1,
                PageSize = 10
            };
                      
            var result = await handler.Handle(query, CancellationToken.None);
                        
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, book => Assert.Equal("available", book.Status));
        }

        [Fact]
        public async Task Handle_ShouldFilterByAuthor()
        {
            
            var handler = new GetBooksQueryHandler(this._dbContext);
            var query = new GetBooksQuery
            {
                CurrentUserId = 1,
                SearchBy = BookSearchCriteria.Author,
                SearchTerm = "John Doe",
                PageNumber = 1,
                PageSize = 10
            };
            
            var result = await handler.Handle(query, CancellationToken.None);
            
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, book => Assert.Equal("John Doe", book.Author));
        }

        [Fact]
        public async Task Handle_ShouldApplyPagination()
        {
            
            var handler = new GetBooksQueryHandler(this._dbContext);
            var query = new GetBooksQuery
            {
                CurrentUserId = 1,
                PageNumber = 2,
                PageSize = 2
            };
            
            var result = await handler.Handle(query, CancellationToken.None);
                        
            Assert.Equal(3, result.TotalCount);
            Assert.Single(result.Items); 
        }

        [Fact]
        public async Task Handle_ShouldReturnRecommendedBooks_FromSameFaculty()
        {
            // Arrange
            var sameFacultyUser = new User
            {
                UserId = 2,
                FacultyId = 10,
                FirstName = "Same",
                LastName = "Faculty",
                Email = "same@faculty.com",
                PasswordHash = "hash2"
            };

            var diffFacultyUser = new User
            {
                UserId = 3,
                FacultyId = 20,
                FirstName = "Diff",
                LastName = "Faculty",
                Email = "diff@faculty.com",
                PasswordHash = "hash3"
            };


            this._dbContext.Users.AddRange(sameFacultyUser, diffFacultyUser);

            this._dbContext.Books.AddRange(
                new Book { BookId = 10, Title = "Book A", OwnerId = 2, CategoryId = 1, Status = "available", Author = "Unknown" }, 
                new Book { BookId = 11, Title = "Book B", OwnerId = 3, CategoryId = 1, Status = "available", Author = "Unknown" }, 
                new Book { BookId = 12, Title = "Book C", OwnerId = 1, CategoryId = 1, Status = "available", Author = "Unknown" }  
            );

            await this._dbContext.SaveChangesAsync();

            var handler = new GetBooksQueryHandler(this._dbContext);

            var query = new GetBooksQuery
            {
                CurrentUserId = 1,
                RecommendForUser = true,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(10, result.Items.First().BookId);
        }

        [Fact]
        public async Task Handle_ShouldNotApplyRecommendation_WhenFlagIsFalse()
        {
            var handler = new GetBooksQueryHandler(this._dbContext);

            var query = new GetBooksQuery
            {
                CurrentUserId = 1,
                RecommendForUser = false,
                PageNumber = 1,
                PageSize = 10
            };

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.Equal(3, result.Items.Count); 
        }
    }
}
