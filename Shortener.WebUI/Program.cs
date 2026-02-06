namespace Shortener.WebUI;

public class Program
{
    record ShortUrlResponseDTO(string OriginalUrl, string ShortCode, DateTimeOffset? ExpiresAt);
    
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpClient("shortener", client =>
        {
            string apiServerAddress = builder.Configuration["API_SERVER_ADDRESS"] ?? throw new InvalidOperationException("API_SERVER_ADDRESS is missing");
            client.BaseAddress = new Uri(apiServerAddress);
        });
        
        var app = builder.Build();

        app.UseStaticFiles();
        
        app.MapGet("/", () => Results.File("index.html", "text/html"));
        app.MapGet("/links-history", () => Results.File("linksHistory.html", "text/html"));
        app.MapGet("/error", () => Results.File("error.html", "text/html"));

        app.Use(async (context, next) =>
        {
            await next();
            if (context.Response.StatusCode == 404)
            {
                context.Response.Redirect("/error");
            }
        });

        app.MapGet("/{shortCode:regex(^[a-zA-Z0-9]{{8}}$)}", async (IHttpClientFactory httpClientFactory, string shortCode) =>
        {
            var client = httpClientFactory.CreateClient("shortener");

            try
            {
                var response = await client.GetAsync($"/api/links/{shortCode}");
                if (!response.IsSuccessStatusCode)
                {
                    return Results.Redirect("/error");
                }
                ShortUrlResponseDTO data = (await response.Content.ReadFromJsonAsync<ShortUrlResponseDTO>())!;
                if (data.ExpiresAt.HasValue && data.ExpiresAt.Value < DateTime.Now)
                {
                    return Results.Redirect("/error");
                }
                return Results.Redirect(data.OriginalUrl);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });

        app.Run();
    }
}