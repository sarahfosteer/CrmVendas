namespace CrmVendas.Models;

public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }  // FK — EF detecta pelo nome
    public decimal Total { get; set; }
    public string Status { get; set; } = "Pendente";
    public DateTime Data { get; set; } = DateTime.UtcNow;
    public string Descricao { get; set; } = "";

    // Navegação de volta pro cliente
    public Cliente Cliente { get; set; } = null!;
}