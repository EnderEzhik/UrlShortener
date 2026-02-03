namespace Shortener.WebUI;

public class Program
{
    record ShortUrlResponseDTO(string OriginalUrl, string ShortCode, DateTimeOffset? ExpiresAt);
    
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHttpClient("shortener", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000");
        });
        
        var app = builder.Build();

        app.UseStaticFiles();
        
        app.MapGet("/", () => Results.File("index.html", "text/html"));
        app.MapGet("/error", () => Results.File("error.html", "text/html"));

        app.Use(async (context, next) =>
        {
            await next();
            if (context.Response.StatusCode == 404)
            {
                context.Response.Redirect("/error");
            }
        });

        app.MapGet("/redirect/{shortCode}", async (IHttpClientFactory httpClientFactory, string shortCode) =>
        {
            var client = httpClientFactory.CreateClient("shortener");

            try
            {
                var response = await client.GetAsync($"/api/links/{shortCode}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return Results.NotFound();
                }
                ShortUrlResponseDTO data = (await response.Content.ReadFromJsonAsync<ShortUrlResponseDTO>())!;
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