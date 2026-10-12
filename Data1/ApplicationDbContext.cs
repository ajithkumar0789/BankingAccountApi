using BankingAccountApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingAccountApi.Data1;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<BankAccount> Accounts => Set<BankAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BankAccount>()
            .HasIndex(a => a.AccountNumber)
            .IsUnique();

        modelBuilder.Entity<BankAccount>()
            .Property(a => a.Balance)
            .HasPrecision(18, 2);
    }
}