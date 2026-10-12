using Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Simüle edilmiş basit ürün stok veritabanı (Bellek içi sözlük)
var stockDatabase = new Dictionary<int, int>
{
    { 101, 50 },  // 101 id'li üründen 50 adet var
    { 102, 3 },   // 102 id'li üründen 3 adet var
    { 103, 0 }    // 103 id'li üründen hiç yok
};

// REST Endpoint: HTTP POST /api/v1/stock/check
app.MapPost("/api/v1/stock/check", (StockCheckRequest request) =>
{
    if (!stockDatabase.TryGetValue(request.ProductId, out var currentStock))
    {
        return Results.NotFound(new StockCheckResponse(
            request.ProductId,
            false,
            0,
            "Ürün stokta bulunamadı."
        ));
    }

    bool isAvailable = currentStock >= request.Quantity;
    string message = isAvailable ? "Stok yeterli." : "Yetersiz stok!";

    return Results.Ok(new StockCheckResponse(
        request.ProductId,
        isAvailable,
        currentStock,
        message
    ));
});

// Port çakışmasını önlemek için InventoryService'i 5100 portunda çalıştıralım
app.Run("http://localhost:5100");