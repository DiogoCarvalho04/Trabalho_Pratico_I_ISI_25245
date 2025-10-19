using NewsAggregatorApi.Models;

namespace NewsAggregatorApi.Services
{
    public class AggregationService : IAggregationService
    {
        private readonly NewsService _newsService;
        private readonly RedditService _redditService;
        private readonly ILogger<AggregationService> _logger;

        public AggregationService(NewsService newsService, RedditService redditService, ILogger<AggregationService> logger)
        {
            _newsService = newsService;
            _redditService = redditService;
            _logger = logger;
        }

        public async Task<IEnumerable<ArticleDto>> GetAggregatedArticlesAsync(
            string topic,
            int days,
            string? language,
            string? sortBy,
            int pageNumber,
            int pageSize)
        {
            _logger.LogInformation("[Aggregator] Iniciando agregação para topic={Topic}, language={Lang}, sortBy={Sort}",
                topic, language, sortBy);

            // Executa as duas chamadas em paralelo
            var newsTask = _newsService.GetArticlesAsync(topic, days, language, sortBy, pageNumber, pageSize);
            var redditTask = _redditService.GetRedditArticlesAsync(topic, days, sortBy, pageNumber, pageSize, language);

            await Task.WhenAll(newsTask, redditTask);

            var newsArticles = newsTask.Result ?? Enumerable.Empty<ArticleDto>();
            var redditArticles = redditTask.Result ?? Enumerable.Empty<ArticleDto>();

            _logger.LogInformation("[Aggregator] NewsAPI: {CountNews} artigos | Reddit: {CountReddit} artigos",
                newsArticles.Count(), redditArticles.Count());

            // Junta resultados e elimina duplicados
            var merged = newsArticles.Concat(redditArticles)
                .GroupBy(a => a.Url)
                .Select(g => g.First())
                .ToList();

            // Ordenação final (mais antigas → mais recentes)
            merged = merged
                .Where(a => a.PublishedAt != default)
                .OrderBy(a => a.PublishedAt)
                .ToList();

            _logger.LogInformation("[Aggregator] Total final de artigos: {Count}", merged.Count);

            // Paginação
            var skip = (pageNumber - 1) * pageSize;
            var paged = merged.Skip(skip).Take(pageSize);

            return paged;
        }
    }
}
