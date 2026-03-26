namespace CrmVendas.Dtos;

public record PedidoRequest(
	int ClienteId,
	decimal Total,
	string Descricao
);

public record PedidoResponse(
	int Id,
	int ClienteId,
	string ClienteNome,
	decimal Total,
	string Status,
	string Descricao,
	DateTime Data
);