using Microsoft.EntityFrameworkCore;

namespace EReceiptAllInOne.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ReceiptEntity> Receipts => Set<ReceiptEntity>();
    public DbSet<ShortLinkEntity> ShortLinks => Set<ShortLinkEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReceiptEntity>().HasIndex(x => x.TxnId);
        modelBuilder.Entity<ShortLinkEntity>().HasIndex(x => x.ExpiresAt);
    }
}
