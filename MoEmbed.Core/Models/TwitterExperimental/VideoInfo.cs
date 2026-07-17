using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MoEmbed.Models.TwitterExperimental
{
    public class VideoInfo
    {
        [JsonPropertyName("aspect_ratio")]
        public List<int> AspectRatio { get; set; }

        [JsonPropertyName("duration_millis")]
        public int? DurationMillis { get; set; }

        [JsonPropertyName("variants")]
        public List<VideoVariant> Variants { get; set; }
    }
}
