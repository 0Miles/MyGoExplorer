using System.Text.Json.Serialization;

namespace Community.PowerToys.Run.Plugin.MyGoExplorer.Models
{
    public class TomorinApiResponse<T>
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("data")]
        public List<T> Data { get; set; }
    }

    public class TomorinApiResult
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("google_gemini_output")]
        public string? GoogleGeminiOutput { get; set; }

        [JsonPropertyName("scene_number")]
        public int? SceneNumber { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("episode")]
        public string Episode { get; set; }

        [JsonPropertyName("frame_start")]
        public int FrameStart { get; set; }

        [JsonPropertyName("frame_end")]
        public int FrameEnd { get; set; }

        [JsonPropertyName("characters")]
        public List<TomorinApiCharacter> Characters { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }
    }

    public class TomorinApiCharacter
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }

    public class TomorinApiErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}