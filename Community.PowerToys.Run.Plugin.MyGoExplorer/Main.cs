using System.Text.Json;
using Wox.Plugin;
using Community.PowerToys.Run.Plugin.MyGoExplorer.Models;
using System.Collections.Concurrent;

namespace Community.PowerToys.Run.Plugin.MyGoExplorer
{

    public class Main : IPlugin
    {
        private PluginInitContext? _context;
        private List<MyGoLine>? _myGoLines;

        private readonly ConcurrentDictionary<string, List<Result>> _cache = new();

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
            string keyword = query.Search;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "請輸入關鍵字以搜尋 MyGO!!!!! 或 Ave Mujica 台詞",
                        SubTitle = "例如：輸入 '示例台詞'"
                    }
                };
            }

            return QueryResults(keyword.Trim()).GetAwaiter().GetResult();
        }

        private async Task<List<Result>> QueryResults(string keyword)
        {
            List<Result> mujicaResults = await QueryTomorinApi(keyword);

            List<Result> mygoResults = _myGoLines
                ?.Where(x => x.Text?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                .Select(x => new Result
                {
                    Title = x.Text,
                    SubTitle = $"MyGO!!!!! Episode: {x.Episode}, Frames: {x.FrameStart}-{x.FrameEnd}",
                    Action = _ =>
                    {
                        HandleMyGoResultAction(x);
                        return true;
                    }
                })
                .ToList() ?? new List<Result>();

            List<Result> results = [.. mygoResults, .. mujicaResults];

            if (results.Count == 0)
            {
                return new List<Result>
                {
                    new Result
                    {
                        Title = "未找到相關結果",
                        SubTitle = "請嘗試輸入其他關鍵字"
                    }
                };
            }
            return results;
        }

        private async Task<List<Result>> QueryTomorinApi(string keyword)
        {
            try
            {
                var query = $"keyword={Uri.EscapeDataString(keyword)}";

                using var client = new HttpClient();
                var response = await client.GetAsync($"https://mygo-api.tomorin.cc/public-api/ave-search?{query}");
                var ocrResults = new List<TomorinApiResult>();
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TomorinApiResponse<TomorinApiResult>>(json);
                    if (result != null)
                        ocrResults = result.Data;
                }
                return ocrResults
                    ?.Select(x => new Result
                    {
                        Title = x.Text ?? $"(出現角色:[{string.Join(", ", x.Characters.Select(x => x.Name))}])",
                        SubTitle = $"Ave Mujica Episode: {x.Episode}, Frames: {x.FrameStart}-{x.FrameEnd}",
                        Action = _ =>
                        {
                            HandleTomorinApiResultAction(x);
                            return true;
                        }
                    })
                    .ToList() ?? new List<Result>();
            }
            catch
            {
                return new List<Result>();
            }
        }

        private async void HandleMyGoResultAction(MyGoLine selectedLine)
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

        private async void HandleTomorinApiResultAction(TomorinApiResult selectedResult)
        {
            var imageUrl = $"https://mygo-api.tomorin.cc/public-api/ave-frames?episode={selectedResult.Episode}&frame_start={selectedResult.FrameStart}&frame_end={selectedResult.FrameEnd}";
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
    }
}
