using Microsoft.EntityFrameworkCore;
using Shortener.Data;
using Shortener.DTOs;
using Shortener.Entities;

namespace Shortener.Services.Analytics;

public class AnalyticsProcessorService : BackgroundService
{
    private readonly Serilog.ILogger _logger;
    private readonly IServiceProvider _services;
    private readonly AnalyticsBufferService _buffer;
    
    public AnalyticsProcessorService(IServiceProvider services, AnalyticsBufferService buffer)
    {
        _logger = Serilog.Log.ForContext<AnalyticsProcessorService>();
        _services = services;
        _buffer = buffer;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<RedirectAnalytics>(20);
        
        await foreach (var analytics in _buffer.ReadAllAsync(stoppingToken))
        {
            batch.Add(analytics);
            
            if (batch.Count >= 10 || (_buffer.IsEmpty && batch.Count > 0))
            {
                await SaveBatchAsync(batch, stoppingToken);
                batch.Clear();
            }
        }
        
        if (batch.Count > 0)
            await SaveBatchAsync(batch, stoppingToken);
    }
    
    private async Task SaveBatchAsync(List<RedirectAnalytics> batch, CancellationToken ct)
    {
        try
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var redirects = batch.Select(c => new Redirect()
            {
                ShortCode = c.ShortCode,
                RedirectedAt = c.RedirectedAt
            }).ToList();
            
            await db.Redirects.AddRangeAsync(redirects, ct);
            
            var counts = redirects
                .GroupBy(r => r.ShortCode)
                .ToDictionary(g => g.Key, g => g.Count());

            var linkIds = counts.Keys.ToList();
            var links = db.Urls
                .Where(u => linkIds.Contains(u.ShortCode))
                .ToList();

            foreach (var link in links)
            {
                link.TotalRedirects += counts[link.ShortCode];
            }
            
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save analytics batch");
        }
    }
}