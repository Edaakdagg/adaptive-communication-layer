using InventoryService.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Shared.Contracts;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

// İki ayrı port: REST için HTTP/1.1, gRPC için HTTP/2
builder.WebHost.ConfigureKestrel(options =>
{
    // REST portu: sadece HTTP/1.1
    options.ListenLocalhost(5100, o =>
    {
        o.Protocols = HttpProtocols.Http1;
    });

    // gRPC portu: sadece HTTP/2
    options.ListenLocalhost(5101, o =>
    {
        o.Protocols = HttpProtocols.Http2;
    });
});

var app = builder.Build();

// gRPC endpoint'i (5101 üzerinden)
app.MapGrpcService<StockGrpcServiceImpl>();

// REST endpoint'i (5100 üzerinden)
var stockDatabase = new Dictionary<int, int> { { 101, 50 }, { 102, 3 }, { 103, 0 } };

app.MapPost("/api/v1/stock/check", (StockCheckRequest request) =>
{
    if (!stockDatabase.TryGetValue(request.ProductId, out var currentStock))
    {
        return Results.NotFound(new StockCheckResponse(request.ProductId, false, 0, "Ürün stokta bulunamadı (REST)."));
    }

    bool isAvailable = currentStock >= request.Quantity;
    return Results.Ok(new StockCheckResponse(
        request.ProductId,
        isAvailable,
        currentStock,
        isAvailable ? "Stok yeterli (REST)." : "Yetersiz stok (REST)!"
    ));
});

app.Run();