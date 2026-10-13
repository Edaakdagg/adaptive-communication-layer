using Grpc.Core;
using Shared.Contracts.Grpc;

namespace InventoryService.Services;

// Shared.Contracts'taki proto derlemesinden gelen temel sınıftan miras alıyoruz
public class StockGrpcServiceImpl : StockGrpcService.StockGrpcServiceBase
{
    private static readonly Dictionary<int, int> StockDatabase = new()
    {
        { 101, 50 },
        { 102, 3 },
        { 103, 0 }
    };

    public override Task<StockGrpcResponse> CheckStock(StockGrpcRequest request, ServerCallContext context)
    {
        if (!StockDatabase.TryGetValue(request.ProductId, out var currentStock))
        {
            return Task.FromResult(new StockGrpcResponse
            {
                ProductId = request.ProductId,
                IsAvailable = false,
                RemainingStock = 0,
                Message = "Ürün stokta bulunamadı (gRPC yanıtı)."
            });
        }

        bool isAvailable = currentStock >= request.Quantity;
        string message = isAvailable ? "Stok yeterli (gRPC yanıtı)." : "Yetersiz stok (gRPC yanıtı)!";

        return Task.FromResult(new StockGrpcResponse
        {
            ProductId = request.ProductId,
            IsAvailable = isAvailable,
            RemainingStock = currentStock,
            Message = message
        });
    }
}