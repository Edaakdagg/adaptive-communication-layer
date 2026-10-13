using Grpc.Net.Client;
using Shared.Contracts;
using Shared.Contracts.Grpc;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("InventoryClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5100"); // REST portu
});

var app = builder.Build();

// REST ile sipariş
app.MapPost("/api/v1/order/create-rest", async (StockCheckRequest orderRequest, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient("InventoryClient");
    var response = await client.PostAsJsonAsync("/api/v1/stock/check", orderRequest);
    var stockResult = await response.Content.ReadFromJsonAsync<StockCheckResponse>();
    return Results.Ok(new { Protocol = "REST (HTTP/1.1 + JSON)", Result = stockResult });
});

// gRPC ile sipariş
app.MapPost("/api/v1/order/create-grpc", async (StockCheckRequest orderRequest) =>
{
    // gRPC için ayrı port 5101
    using var channel = GrpcChannel.ForAddress("http://localhost:5101");
    var grpcClient = new StockGrpcService.StockGrpcServiceClient(channel);

    var grpcResponse = await grpcClient.CheckStockAsync(new StockGrpcRequest
    {
        ProductId = orderRequest.ProductId,
        Quantity = orderRequest.Quantity
    });

    return Results.Ok(new
    {
        Protocol = "gRPC (HTTP/2 + Protobuf)",
        Result = new
        {
            grpcResponse.ProductId,
            grpcResponse.IsAvailable,
            grpcResponse.RemainingStock,
            grpcResponse.Message
        }
    });
});

app.Run("http://localhost:5200");