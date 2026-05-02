using Shortener.Data;
using Shortener.DTOs;
using Shortener.Entities;

namespace Shortener.Services.Analytics;

public class AnalyticsProcessorService : BackgroundService
{
    private readonly Serilog.ILogger _logger;
    private readonly IServiceProvider _services;
    private readonly AnalyticsBufferService _buffer;
    private readonly ApplicationDbContext _db;
    
    
    public AnalyticsProcessorService(IServiceProvider services, AnalyticsBufferService buffer, ApplicationDbContext applicationContext)
    {
        _logger = Serilog.Log.ForContext<AnalyticsProcessorService>();
        _services = services;
        _buffer = buffer;
        _db = applicationContext;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Batch-обработка для уменьшения нагрузки на БД
        var batch = new List<ClickAnalytics>(20);
        
        await foreach (var analytics in _buffer.ReadAllAsync(stoppingToken))
        {
            batch.Add(analytics);
            
            // Сохраняем батч каждые 10 событий или когда буфер опустеет
            if (batch.Count >= 10 || (batch.Count > 0 && _buffer.IsEmpty))
            {
                await SaveBatchAsync(batch, stoppingToken);
                batch.Clear();
            }
        }
        
        // Сохраняем остатки при остановке
        if (batch.Any())
            await SaveBatchAsync(batch, stoppingToken);
    }
    
    private async Task SaveBatchAsync(List<ClickAnalytics> batch, CancellationToken ct)
    {
        try
        {
            // using var scope = _services.CreateScope();
            // var analyticsDb = scope.ServiceProvider.GetRequiredService<IAnalyticsRepository>();//TODO: заменить на сервис аналитики или прямой ef core запрос временно
            
            // Обогащаем данные (гео, устройство и т.д.)
            foreach (var item in batch)
            {
                EnrichWithGeoDataAsync(item);
                EnrichWithPlatformType(item);
                // ... другие обогащения
            }
            
            // Массовая вставка в ClickHouse/PostgreSQL
            await _db.Clicks.AddRangeAsync(batch, ct);
            await _db.SaveChangesAsync(ct);
            
            _logger.Debug("Saved {Count} analytics events", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save analytics batch");
            // Можно положить в dead-letter queue
        }
    }
    
    private void EnrichWithGeoDataAsync(ClickAnalytics analytics)
    {
        // Здесь можно закэшировать GeoIP данные
        // Или использовать in-memory lookup
        analytics.CountryCode = "TEST_UAK";
    }
    
    private void EnrichWithPlatformType(ClickAnalytics analytics)
    {
        analytics.Platform = nameof(PlatformType.Android);
    }
}