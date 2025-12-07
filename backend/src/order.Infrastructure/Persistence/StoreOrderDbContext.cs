using Microsoft.EntityFrameworkCore;
using OrderEntity = Order.Core.Domain.Orders.Order;
using OrderItem = Order.Core.Domain.Orders.OrderItem;
using Order.Core.Domain.Orders.Enums;
using Order.Core.Domain.Orders.ValueObjects;

namespace Order.Infrastructure.Persistence;

public class StoreOrderDbContext : DbContext
{
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public StoreOrderDbContext(DbContextOptions<StoreOrderDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureOrder(modelBuilder);
        ConfigureOrderItem(modelBuilder);
    }

    private void ConfigureOrder(ModelBuilder modelBuilder)
    {
        var order = modelBuilder.Entity<OrderEntity>();
        order.ToTable("orders");

        order.HasKey(o => o.Id);

        order.HasIndex(o => o.OrderNumber)
            .IsUnique();

        order.Property(o => o.OrderNumber)
            .HasConversion(
                on => on.Value,
                value => OrderNumber.FromString(value)
            )
            .HasColumnName("OrderNumber")
            .IsRequired();

        order.Property(o => o.Total)
            .HasConversion(
                m => m.Amount,
                value => Money.FromDecimal(value)
            )
            .HasColumnName("Total")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        order.Property(o => o.Status)
            .HasConversion<string>()
            .HasColumnName("Status")
            .IsRequired();

        order.Property(o => o.CreatedAt);
        order.Property(o => o.UpdatedAt);
        order.Property(o => o.ClosedAt);
        order.Property(o => o.DeletedAt);

        // relação 1:N
        order.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureOrderItem(ModelBuilder modelBuilder)
    {
        var item = modelBuilder.Entity<OrderItem>();

        item.ToTable("order_items");

        item.HasKey(i => i.Id);
        item.Ignore(i => i.Subtotal);

        item.Property(i => i.OrderId)
            .IsRequired();

        item.Property(i => i.UnitPrice)
            .HasConversion(
                m => m.Amount,
                value => Money.FromDecimal(value)
            )
            .HasColumnName("UnitPrice")
            .HasColumnType("decimal(18,2)");

        item.Property(i => i.Description)
            .HasMaxLength(200)
            .IsRequired();

        item.Property(i => i.Quantity)
            .IsRequired();
        
    }

}
