namespace CrmVendas.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Segmento { get; set; } = "";
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Navegação — EF Core lê isso como "Cliente tem muitos Pedidos"
    public ICollection<Pedido> Pedidos { get; set; } = [];
}