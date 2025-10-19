using System.Text.Json;
using System.Text.Json.Serialization;
using NewsAggregatorApi.Models;

namespace NewsAggregatorApi.Services
{
    public class RedditService
    {
        private readonly HttpClient _http;
        private readonly ILogger<RedditService> _logger;

        public RedditService(IHttpClientFactory httpClientFactory, ILogger<RedditService> logger)
        {
            _http = httpClientFactory.CreateClient();
            _logger = logger;

            _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; NewsAggregator/1.0; +https://localhost)");
        }

        public async Task<IEnumerable<ArticleDto>> GetRedditArticlesAsync(
            string topic, int days, string? sortBy, int pageNumber, int pageSize, string? language = "en")
        {
            _logger.LogInformation("[RedditService] Iniciado para topic={Topic}, days={Days}, sortBy={Sort}, lang={Lang}",
                topic, days, sortBy, language);

            var (sortParam, tParam) = BuildRedditSort(sortBy, days);
            var limit = Math.Clamp(pageSize, 1, 100);

            // Escolher subreddits conforme linguagem
            var subreddits = GetSubredditsByLanguage(language ?? "en");

            // Filtro de subreddits ex: subreddit:CryptoCurrency OR subreddit:Bitcoin
            var subredditFilter = string.Join(" OR ", subreddits.Select(s => $"subreddit:{s}"));

            // Query final: combina tópico + filtro de subreddits
            var query = $"{topic} ({subredditFilter})";

            var url = $"https://www.reddit.com/search.json?q={Uri.EscapeDataString(query)}" +
                      $"&sort={sortParam}" +
                      (tParam is null ? "" : $"&t={tParam}") +
                      $"&limit={limit}";

            try
            {
                _logger.LogInformation("Reddit URL: {Url}", url);
                var res = await _http.GetAsync(url);

                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Reddit devolveu erro: {Status}", res.StatusCode);
                    return Enumerable.Empty<ArticleDto>();
                }

                var json = await res.Content.ReadAsStringAsync();

                var parsed = JsonSerializer.Deserialize<RedditSearchResponse>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString
                    });

                if (parsed?.Data?.Children == null)
                {
                    _logger.LogWarning("Nenhum campo 'children' encontrado no JSON do Reddit.");
                    return Enumerable.Empty<ArticleDto>();
                }

                var fromUtc = DateTime.UtcNow.AddDays(-days);

                var posts = parsed.Data.Children
                    .Select(c => c.Data)
                    .Where(p => p != null)
                    .Where(p =>
                    {
                        double timestamp = p!.Created_utc ?? p.Created ?? 0;
                        return timestamp > 0 && DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(timestamp)).UtcDateTime >= fromUtc;
                    })
                    .OrderByDescending(p => p!.Ups ?? 0)
                    .Select(p => new ArticleDto
                    {
                        Source = $"Reddit - r/{p!.Subreddit}",
                        Title = p.Title ?? "(Sem título)",
                        Description = p.Selftext ?? "",
                        Url = !string.IsNullOrWhiteSpace(p.Permalink)
                            ? $"https://www.reddit.com{p.Permalink}"
                            : p.Url,
                        PublishedAt = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(p.Created_utc ?? p.Created ?? 0)).UtcDateTime
                    })
                    .ToList();

                _logger.LogInformation("[RedditService] {Count} artigos obtidos do Reddit ({Lang})", posts.Count, language);
                return posts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar dados do Reddit.");
                return Enumerable.Empty<ArticleDto>();
            }
        }

        private static (string sort, string? t) BuildRedditSort(string? sortBy, int days)
        {
            if (string.Equals(sortBy, "popularity", StringComparison.OrdinalIgnoreCase))
            {
                string t = days <= 7 ? "week" : days <= 30 ? "month" : "year";
                return ("top", t);
            }
            return ("new", null);
        }

        //Subreddits por idioma
        private static IEnumerable<string> GetSubredditsByLanguage(string language)
        {
            if (language.Equals("pt", StringComparison.OrdinalIgnoreCase))
            {
                return new[]
                {
                    "brasil",
                    "portugal",
                    "noticias",
                    "criptomoedas",
                    "tecnologia",
                    "investimentos",
                    "EconomiaBrasil",
                    "futebol",
                    "politicaBR"
                };
            }

            // Default: inglês / internacional
            return new[]
            {
                "CryptoCurrency",
                "Bitcoin",
                "CryptoMarkets",
                "technology",
                "worldnews",
                "news",
                "politics",
                "investing",
                "economics"
            };
        }
    }
}
