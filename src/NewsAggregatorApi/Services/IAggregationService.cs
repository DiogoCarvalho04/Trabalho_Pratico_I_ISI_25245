using NewsAggregatorApi.Models;

namespace NewsAggregatorApi.Services
{
    public interface IAggregationService
    {
        Task<IEnumerable<ArticleDto>> GetAggregatedArticlesAsync(
            string topic,
            int days,
            string? language,
            string? sortBy,
            int pageNumber,
            int pageSize);
    }
}
