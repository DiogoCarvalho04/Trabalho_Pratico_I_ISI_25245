using Microsoft.AspNetCore.Mvc;
using NewsAggregatorApi.Services;

namespace NewsAggregatorApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly AggregationService _aggregationService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(AggregationService aggregationService, ILogger<NewsController> logger)
        {
            _aggregationService = aggregationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string topic,
            [FromQuery] int days,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? language = null,
            [FromQuery] string? sortBy = null)
        {
            if (string.IsNullOrWhiteSpace(topic))
                return BadRequest("O parâmetro 'topic' é obrigatório.");

            try
            {
                _logger.LogInformation("Pedido recebido: topic={Topic}, days={Days}, language={Lang}, sortBy={Sort}, page={Page}, size={Size}",
                    topic, days, language, sortBy, pageNumber, pageSize);

                var articles = await _aggregationService.GetAggregatedArticlesAsync(
                    topic, days, language, sortBy, pageNumber, pageSize);

                var response = new
                {
                    topic,
                    daysUsed = days,
                    totalResults = articles.Count(),
                    articles
                };

                _logger.LogInformation("Resposta: {Count} artigos agregados.", response.totalResults);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pedido em NewsController.");
                return StatusCode(500, new { error = "Erro interno ao processar o pedido.", details = ex.Message });
            }
        }
    }
}
