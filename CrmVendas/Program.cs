using Microsoft.EntityFrameworkCore;
using CrmVendas.Data;
using CrmVendas.Models;
using CrmVendas.Dtos;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(conn, ServerVersion.AutoDetect(conn)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("FrontendPolicy");

app.UseSwagger();
app.UseSwaggerUI();

// ───────── CLIENTES ─────────

// GET /clientes — SELECT c.*, COUNT(p.Id) FROM Clientes c LEFT JOIN Pedidos p
app.MapGet("/clientes", async (AppDbContext db) =>
{
    var clientes = await db.Clientes
    .OrderBy(c => c.Nome) // <-- 1. PRIMEIRO ORDENA!
    .Select(c => new ClienteResponse( // <-- 2. DEPOIS TRANSFORMA!
        c.Id,
        c.Nome,
        c.Email,
        c.Telefone,
        c.Segmento,
        c.CriadoEm,
        c.Pedidos.Count()
    ))
    .ToListAsync(); // <-- 3. TRAZ PRA MEMÓRIA
    return Results.Ok(clientes);
});

// GET /clientes/{id} — SELECT * FROM Clientes WHERE Id = @id
app.MapGet("/clientes/{id}", async (int id, AppDbContext db) =>
{
    var c = await db.Clientes
        .Where(c => c.Id == id)
        .Select(c => new ClienteResponse(
            c.Id, c.Nome, c.Email, c.Telefone, c.Segmento,
            c.CriadoEm,
            c.Pedidos.Count()))
        .FirstOrDefaultAsync();
    return c is null ? Results.NotFound() : Results.Ok(c);
});

// POST /clientes — INSERT INTO Clientes ...
app.MapPost("/clientes", async (ClienteRequest dto, AppDbContext db) =>
{
    var cliente = new Cliente
    {
        Nome = dto.Nome,
        Email = dto.Email,
        Telefone = dto.Telefone,
        Segmento = dto.Segmento
    };
    db.Clientes.Add(cliente);
    await db.SaveChangesAsync();
    return Results.Created($"/clientes/{cliente.Id}",
        new ClienteResponse(cliente.Id, cliente.Nome, cliente.Email,
            cliente.Telefone, cliente.Segmento, cliente.CriadoEm, 0));
});

// PUT /clientes/{id} — UPDATE Clientes SET ... WHERE Id = @id
app.MapPut("/clientes/{id}", async (int id, ClienteRequest dto, AppDbContext db) =>
{
    var cliente = await db.Clientes.FindAsync(id);
    if (cliente is null) return Results.NotFound();

    cliente.Nome = dto.Nome;
    cliente.Email = dto.Email;
    cliente.Telefone = dto.Telefone;
    cliente.Segmento = dto.Segmento;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE /clientes/{id} — DELETE FROM Clientes WHERE Id = @id
app.MapDelete("/clientes/{id}", async (int id, AppDbContext db) =>
{
    var cliente = await db.Clientes.FindAsync(id);
    if (cliente is null) return Results.NotFound();

    db.Clientes.Remove(cliente);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ───────── PEDIDOS ─────────

// GET /pedidos — todos os pedidos com nome do cliente (JOIN)
app.MapGet("/pedidos", async (AppDbContext db) =>
{
    var pedidos = await db.Pedidos
        .Include(p => p.Cliente)               // JOIN com Clientes
        .OrderByDescending(p => p.Data)
        .Select(p => new PedidoResponse(
            p.Id, p.ClienteId, p.Cliente.Nome,
            p.Total, p.Status, p.Descricao, p.Data))
        .ToListAsync();
    return Results.Ok(pedidos);
});

// GET /clientes/{id}/pedidos — pedidos de um cliente específico
app.MapGet("/clientes/{id}/pedidos", async (int id, AppDbContext db) =>
{
    var existe = await db.Clientes.AnyAsync(c => c.Id == id);
    if (!existe) return Results.NotFound("Cliente não encontrado");

    var pedidos = await db.Pedidos
        .Where(p => p.ClienteId == id)
        .OrderByDescending(p => p.Data)
        .Select(p => new PedidoResponse(
            p.Id, p.ClienteId, "",
            p.Total, p.Status, p.Descricao, p.Data))
        .ToListAsync();
    return Results.Ok(pedidos);
});

// POST /pedidos — INSERT INTO Pedidos ...
app.MapPost("/pedidos", async (PedidoRequest dto, AppDbContext db) =>
{
    // Valida se o cliente existe antes de criar o pedido
    var cliente = await db.Clientes.FindAsync(dto.ClienteId);
    if (cliente is null)
        return Results.BadRequest("Cliente não encontrado");

    var pedido = new Pedido
    {
        ClienteId = dto.ClienteId,
        Total = dto.Total,
        Descricao = dto.Descricao,
        Status = "Pendente",
        Data = DateTime.UtcNow
    };
    db.Pedidos.Add(pedido);
    await db.SaveChangesAsync();

    return Results.Created($"/pedidos/{pedido.Id}",
        new PedidoResponse(pedido.Id, pedido.ClienteId, cliente.Nome,
            pedido.Total, pedido.Status, pedido.Descricao, pedido.Data));
});

// PUT /pedidos/{id}/status — atualizar só o status
app.MapPut("/pedidos/{id}/status", async (int id, string status, AppDbContext db) =>
{
    var pedido = await db.Pedidos.FindAsync(id);
    if (pedido is null) return Results.NotFound();

    pedido.Status = status;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();