using LNUBookShareDAL.Models;

using Microsoft.EntityFrameworkCore;

using System;

namespace LNUBookShareTests
{
    public abstract class TestBase : IDisposable
    {
        protected LNUBookShareDbContext DbContext { get; }

        protected TestBase()
        {
            var options = new DbContextOptionsBuilder<LNUBookShareDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            this.DbContext = new LNUBookShareDbContext(options);
        }

        public void Dispose()
        {
            this.DbContext?.Dispose();
        }
    }
}