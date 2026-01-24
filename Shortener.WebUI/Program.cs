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

        app.MapGet("/error", async (HttpContext context) =>
        {
            await context.Response.WriteAsync("An error occured");
        });

        app.MapGet("/redirect/{shortCode}", async (IHttpClientFactory httpClientFactory, string shortCode) =>
        {
            var client = httpClientFactory.CreateClient("shortener");

            try
            {
                var response = await client.GetAsync($"/api/links/{shortCode}");
                response.EnsureSuccessStatusCode();
                ShortUrlResponseDTO data = (await response.Content.ReadFromJsonAsync<ShortUrlResponseDTO>())!;
                return Results.Redirect(data.OriginalUrl);
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    return Results.Redirect("/error");
                }

                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });
        
        app.MapGet(("/api/links"), async (IHttpClientFactory httpClientFactory) =>
        {
            var client = httpClientFactory.CreateClient("shortener");

            try
            {
                var response = await client.GetAsync("/api/links");
                response.EnsureSuccessStatusCode();

                var urls = (await response.Content.ReadFromJsonAsync<List<ShortUrlResponseDTO>>())!;
                return urls;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        });

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.Run();
    }
}