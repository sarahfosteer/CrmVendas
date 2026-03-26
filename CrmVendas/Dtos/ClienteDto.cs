namespace CrmVendas.Dtos;

// O que o usuário MANDA pra criar/editar um cliente
public record ClienteRequest(
    string Nome,
    string Email,
    string Telefone,
    string Segmento
);

// O que a API DEVOLVE pro usuário
public record ClienteResponse(
    int Id,
    string Nome,
    string Email,
    string Telefone,
    string Segmento,
    DateTime CriadoEm,
    int TotalPedidos
);