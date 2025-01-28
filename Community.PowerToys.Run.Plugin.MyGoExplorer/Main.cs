using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Wox.Plugin;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;

namespace Community.PowerToys.Run.Plugin.MyGoExplorer
{

    public class Main : IPlugin
    {
        private PluginInitContext? _context;
        private List<MyGoLine>? _myGoLines;

        public string Name => "MyGo Explorer";
        public string Description => "搜尋 MyGo 台詞並複製圖片到剪貼簿";
        public static string PluginID => "4C1BF5784285489BA6BFC92AE0641AAE";

        public void Init(PluginInitContext context)
        {
            _context = context;

            try
            {
                var jsonPath = Path.Combine(_context.CurrentPluginMetadata.PluginDirectory, "MyGoLines.json");
                var jsonContent = File.ReadAllText(jsonPath);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _myGoLines = JsonSerializer.Deserialize<MyGoLinesData>(jsonContent, options)?.Result ?? new List<MyGoLine>();
            }
            catch (Exception ex)
            {
                _context.API.ShowMsg("錯誤", $"無法載入 MyGoLines 資料：{ex.Message}");
                _myGoLines = new List<MyGoLine>();
            }
        }

        public List<Result> Query(Query query)
        {
            var keyword = query.Search;

            if (string.IsNullOrEmpty(keyword))
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "請輸入關鍵字以搜尋 MyGo 台詞",
                        SubTitle = "例如：輸入 '示例台詞'",
                        IcoPath = "Images/plugin.png"
                    }
                };
            }

            var results = _myGoLines
                ?.FindAll(x => x.Text?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                .ConvertAll(x => new Result
                {
                    Title = x.Text,
                    SubTitle = $"Episode: {x.Episode}, Frame: {x.FrameStart}",
                    IcoPath = $"https://cdn.anon-tokyo.com/thumb/thumb/{x.Episode}__{x.FrameStart}.jpg",
                    Action = _ =>
                    {
                        HandleResultAction(x);
                        return true;
                    }
                });

            if (results?.Count == 0)
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "未找到相關結果",
                        SubTitle = "請嘗試輸入其他關鍵字",
                        IcoPath = "Images/error.png"
                    }
                };
            }

            return results ?? new List<Result>();
        }

        private async void HandleResultAction(MyGoLine selectedLine)
        {
            var imageUrl = $"https://anon-tokyo.com/image?frame={selectedLine.FrameStart}&episode={selectedLine.Episode}";
            var imagePath = await DownloadImage(imageUrl);

            if (!string.IsNullOrEmpty(imagePath))
            {
                using var image = Image.FromFile(imagePath);

                Clipboard.SetImage(image);
            }
            else
            {
                _context?.API.ShowMsg("操作失敗", "下載圖片失敗，請檢查網路連線。");
            }
        }

        private async Task<string?> DownloadImage(string url)
        {
            try
            {
                var tempPath = Path.Combine(Path.GetTempPath(), "MyGoExplorer");
                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }

                var fileName = Path.Combine(tempPath, $"{GetImageFileName(url)}.jpg");

                if (File.Exists(fileName))
                {
                    return fileName;
                }

                using var client = new HttpClient();
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    throw new Exception("圖片下載失敗");

                // 儲存圖片到 %TMP%\MyGoExplorer
                using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                await response.Content.CopyToAsync(fileStream);

                return fileName;
            }
            catch
            {
                return null;
            }
        }

        private string GetImageFileName(string url)
        {
            return url.GetHashCode().ToString();
        }

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
}
