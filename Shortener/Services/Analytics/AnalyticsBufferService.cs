using System.Threading.Channels;
using Shortener.DTOs;

namespace Shortener.Services.Analytics;

public class AnalyticsBufferService
{
    private readonly Channel<RedirectAnalytics> _channel;
    private readonly Serilog.ILogger _logger;
    
    public AnalyticsBufferService()
    {
        _logger = Serilog.Log.ForContext<AnalyticsBufferService>();
        var options = new BoundedChannelOptions(10)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = false,
            SingleReader = true
        };
        _channel = Channel.CreateBounded<RedirectAnalytics>(options);
    }

    public bool IsEmpty => _channel.Reader.CanCount && _channel.Reader.Count <= 0; 
    
    public async Task WriteAsync(RedirectAnalytics analytics, CancellationToken ct = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(100);
            
            await _channel.Writer.WriteAsync(analytics, cts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("Analytics buffer is full, dropping event for {ShortCode}", analytics.ShortCode);
        }
    }
    
    public IAsyncEnumerable<RedirectAnalytics> ReadAllAsync(CancellationToken ct = default)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}