using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
namespace WebApplication1.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public DbSet<AccountList> AccountLists { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<AccountTransaction> AccountTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Index the AccountNumber
            modelBuilder.Entity<AccountList>()
                .HasIndex(a => a.AccountNumber)
                .IsUnique();


            modelBuilder.Entity<AccountTransaction>()
                .HasIndex(t => new { t.AccountListId, t.TransactionDate });
        }
    }
}
