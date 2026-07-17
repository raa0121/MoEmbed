using System.Text.Json.Serialization;

namespace MoEmbed.Models.TwitterExperimental
{
    public class VideoVariant
    {
        [JsonPropertyName("bitrate")]
        public int? Bitrate { get; set; }

        [JsonPropertyName("content_type")]
        public string ContentType { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
