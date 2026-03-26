using Microsoft.EntityFrameworkCore;
using CrmVendas.Models;

namespace CrmVendas.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Precisão decimal — sem isso MySQL salva errado valores monetários
        mb.Entity<Pedido>()
          .Property(p => p.Total)
          .HasColumnType("decimal(10,2)");

        mb.Entity<Pedido>()
          .HasOne(p => p.Cliente)
          .WithMany(c => c.Pedidos)
          .HasForeignKey(p => p.ClienteId);
    }
}