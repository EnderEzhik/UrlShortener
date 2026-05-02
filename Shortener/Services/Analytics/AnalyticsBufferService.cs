using System.Threading.Channels;
using Shortener.DTOs;
namespace Shortener.Services.Analytics;

public class AnalyticsBufferService
{
    private readonly Channel<ClickAnalytics> _channel;
    private readonly Serilog.ILogger _logger;
    
    public AnalyticsBufferService()
    {
        _logger = Serilog.Log.ForContext<AnalyticsBufferService>();
        // Ограничиваем размер буфера (например, 10 событий)
        var options = new BoundedChannelOptions(10)
        {
            FullMode = BoundedChannelFullMode.Wait, // Ждем, если переполнен
            SingleWriter = false,
            SingleReader = true
        };
        _channel = Channel.CreateBounded<ClickAnalytics>(options);
    }

    public bool IsEmpty => _channel.Reader.CanCount && _channel.Reader.Count <= 0; 
    
    public async ValueTask<bool> WriteAsync(ClickAnalytics analytics, CancellationToken ct = default)
    {
        try
        {
            // Попытка записи с таймаутом 100ms
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(100);
            
            await _channel.Writer.WriteAsync(analytics, cts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("Analytics buffer is full, dropping event for {ShortCode}", analytics.ShortCode);
            return false; // Буфер переполнен - дропаем событие
        }
    }
    
    public IAsyncEnumerable<ClickAnalytics> ReadAllAsync(CancellationToken ct = default)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}