using Microsoft.EntityFrameworkCore;
using OrderEntity = Order.Core.Domain.Orders.Order;
using OrderItem = Order.Core.Domain.Orders.OrderItem;
using Order.Core.Domain.Orders.Enums;
using Order.Core.Domain.Orders.ValueObjects;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.Persistence;

public class StoreOrderDbContext : DbContext
{
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();


    public StoreOrderDbContext(DbContextOptions<StoreOrderDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureOrder(modelBuilder);
        ConfigureOrderItem(modelBuilder);
        ConfigureOutbox(modelBuilder);
    }

    private void ConfigureOrder(ModelBuilder modelBuilder)
    {
        var order = modelBuilder.Entity<OrderEntity>();
        order.ToTable("orders");

        order.HasKey(o => o.Id);

        order.HasIndex(o => o.OrderNumber)
            .IsUnique();
        order.Property(o => o.ClientName);

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
        
        order.Ignore(o => o.DomainEvents);
    }

    private void ConfigureOrderItem(ModelBuilder modelBuilder)
    {
        var item = modelBuilder.Entity<OrderItem>();

        item.ToTable("order_items");

        item.HasKey(i => i.Id);
        item.Ignore(i => i.Subtotal);

        item.HasIndex(i => i.ProductId).IsUnique();
        item.Property(i => i.ProductId).IsRequired().HasMaxLength(60);
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
    private void ConfigureOutbox(ModelBuilder modelBuilder)
    {
        var outbox = modelBuilder.Entity<OutboxMessage>(cfg =>
        {
            cfg.ToTable("outbox_messages");
            cfg.HasKey(o => o.Id);
            cfg.Property(o => o.OccurredOn).IsRequired();
            cfg.Property(o => o.Type).HasMaxLength(300);
            cfg.Property(o => o.Payload).IsRequired();
            cfg.Property(o => o.Processed).HasDefaultValue(false);
            cfg.Property(o => o.ProcessedOn);
        });
    }

}
