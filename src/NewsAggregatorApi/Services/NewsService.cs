using System.Text.Json;
using NewsAggregatorApi.Services;

public class NewsService : INewsService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<NewsService> _logger;

    public NewsService(HttpClient http, IConfiguration config, ILogger<NewsService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("NewsAggregatorApp/1.0");
    }

    public async Task<IEnumerable<ArticleDto>> GetArticlesAsync(
    string topic,
    int days,
    string? language,
    string? sortBy,
    int pageNumber,
    int pageSize)
    {
        var apiKey = _config["NewsApi:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("NewsApi:ApiKey not configured.");

        if (days > 30)
            throw new Exception("Limite de pesquisa é 30 dias");

        // Calcula a data inicial (N dias atrás)
        var fromDate = DateTime.UtcNow.AddDays(-days).ToString("yyyy-MM-dd");

        // Usa valores padrão caso não sejam enviados
        sortBy ??= "publishedAt";

        // Monta a query base
        var url = $"https://newsapi.org/v2/everything?" +
                  $"q={Uri.EscapeDataString(topic)}" +
                  $"&from={fromDate}" +
                  $"&sortBy={sortBy}" +
                  $"&page={pageNumber}" +
                  $"&pageSize={pageSize}" +
                  $"&apiKey={apiKey}";

        // Adiciona o parâmetro de idioma se tiver sido enviado
        if (!string.IsNullOrWhiteSpace(language))
            url += $"&language={language}";

        _logger.LogInformation("Requesting NewsAPI URL: {Url}", url);

        var res = await _http.GetAsync(url);
        if (!res.IsSuccessStatusCode)
        {
            var errorText = await res.Content.ReadAsStringAsync();
            _logger.LogError("NewsAPI failed: {StatusCode} - {Error}", res.StatusCode, errorText);
            throw new HttpRequestException(
                $"NewsAPI returned {(int)res.StatusCode} {res.ReasonPhrase}",
                null,
                res.StatusCode
            );
        }

        var json = await res.Content.ReadAsStringAsync();
        var newsResp = JsonSerializer.Deserialize<NewsApiResponse>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (newsResp?.Articles == null)
            return Enumerable.Empty<ArticleDto>();

        // Mapeia para o DTO final
        return newsResp.Articles.Select(a => new ArticleDto
        {
            Source = a.Source?.Name ?? "unknown",
            Title = a.Title,
            Description = a.Description,
            Content = a.Content,
            Url = a.Url,
            PublishedAt = a.PublishedAt
        });
    }
}
