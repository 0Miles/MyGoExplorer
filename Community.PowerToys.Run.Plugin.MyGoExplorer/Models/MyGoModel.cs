using System.Text.Json.Serialization;

namespace Community.PowerToys.Run.Plugin.MyGoExplorer.Models
{
    public class MyGoLinesData
    {
        [JsonPropertyName("result")]
        public List<MyGoLine>? Result { get; set; }
    }

    public class MyGoLine
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("episode")]
        public string? Episode { get; set; }

        [JsonPropertyName("frame_start")]
        public int? FrameStart { get; set; }
        [JsonPropertyName("frame_end")]
        public int? FrameEnd { get; set; }
        [JsonPropertyName("segment_id")]
        public int? SegmentId { get; set; }
    }
}
