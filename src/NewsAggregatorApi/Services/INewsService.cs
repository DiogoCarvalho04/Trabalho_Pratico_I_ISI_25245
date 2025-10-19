using NewsAggregatorApi.Models;

namespace NewsAggregatorApi.Services
{
    public interface INewsService
    {
        Task<IEnumerable<ArticleDto>> GetArticlesAsync(
            string topic,
            int days,
            string? language,
            string? sortBy,
            int pageNumber,
            int pageSize);
    }
}
