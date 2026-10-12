namespace Shared.Contracts;

// İstemcinin göndereceği istek modeli
public record StockCheckRequest(int ProductId, int Quantity);

// Hedef servisin döneceği yanıt modeli
public record StockCheckResponse(int ProductId, bool IsAvailable, int RemainingStock, string Message);