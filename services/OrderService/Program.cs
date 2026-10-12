using Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);

// .NET'in optimize edilmiş HTTP istemci fabrikası (Connection pooling yönetir)
builder.Services.AddHttpClient("InventoryClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5100");
});

var app = builder.Build();

// Müşterinin sipariş verdiği uç nokta: POST /api/v1/order/create
app.MapPost("/api/v1/order/create", async (StockCheckRequest orderRequest, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient("InventoryClient");

    // InventoryService'e REST (JSON over HTTP/1.1) çağrısı yapıyoruz
    var response = await client.PostAsJsonAsync("/api/v1/stock/check", orderRequest);

    if (!response.IsSuccessStatusCode)
    {
        return Results.BadRequest(new { Status = "Hata", Detail = "Stok servisi yanıt vermedi veya ürün yok." });
    }

    var stockResult = await response.Content.ReadFromJsonAsync<StockCheckResponse>();

    if (stockResult is not null && stockResult.IsAvailable)
    {
        return Results.Ok(new 
        { 
            Status = "Sipariş Başarılı", 
            ProductId = orderRequest.ProductId,
            Quantity = orderRequest.Quantity,
            InventoryDetail = stockResult 
        });
    }

    return Results.BadRequest(new 
    { 
        Status = "Sipariş Reddedildi", 
        Reason = "Yetersiz Stok", 
        InventoryDetail = stockResult 
    });
});

// OrderService'i 5200 portunda çalıştıralım
app.Run("http://localhost:5200");