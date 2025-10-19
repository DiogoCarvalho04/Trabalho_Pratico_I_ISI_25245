using System.Text.Json.Serialization;

namespace NewsAggregatorApi.Models
{
    public class RedditSearchResponse
    {
        [JsonPropertyName("data")]
        public RedditData? Data { get; set; }
    }

    public class RedditData
    {
        [JsonPropertyName("children")]
        public List<RedditChild>? Children { get; set; }
    }

    public class RedditChild
    {
        [JsonPropertyName("data")]
        public RedditPostData? Data { get; set; }
    }

    public class RedditPostData
    {
        [JsonPropertyName("subreddit")]
        public string? Subreddit { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("selftext")]
        public string? Selftext { get; set; }

        [JsonPropertyName("ups")]
        public int? Ups { get; set; }

        [JsonPropertyName("permalink")]
        public string? Permalink { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        // Alguns posts têm "created" e outros "created_utc"
        [JsonPropertyName("created_utc")]
        public double? Created_utc { get; set; }

        [JsonPropertyName("created")]
        public double? Created { get; set; }
    }
}
