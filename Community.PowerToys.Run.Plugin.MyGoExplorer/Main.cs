using System.Text.Json;
using Wox.Plugin;
using Community.PowerToys.Run.Plugin.MyGoExplorer.Models;
using System.Collections.Concurrent;

namespace Community.PowerToys.Run.Plugin.MyGoExplorer
{

    public class Main : IPlugin
    {
        private PluginInitContext? _context;

        private readonly ConcurrentDictionary<string, List<Result>> _cache = new();

        public string Name => "MyGo Explorer";
        public string Description => "搜尋 MyGO!!!!! 或 Ave Mujica 的台詞並複製圖片到剪貼簿";
        public static string PluginID => "4C1BF5784285489BA6BFC92AE0641AAE";

        public void Init(PluginInitContext context)
        {
            _context = context;

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
                        SubTitle = "例如：輸入 '是又怎樣'"
                    }
                };
            }

            return QueryResults(keyword.Trim()).GetAwaiter().GetResult();
        }

        private async Task<List<Result>> QueryResults(string keyword)
        {
            List<Result> results = await QueryTomorinApi(keyword);

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
                var query = $"keyword={Uri.EscapeDataString(keyword)}&sources=mygo%2Cave";

                using var client = new HttpClient();
                var response = await client.GetAsync($"https://api-3.tomorin.cc/api/search?{query}");
                var ocrResults = new List<TomorinApiResult>();
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<TomorinApiResponse<TomorinApiResult>>(json);
                    if (result != null)
                        ocrResults = result.Data;
                }

                

                return ocrResults
                    ?.Select(x =>
                    {
                        var soruce = x.Source == "mygo" ? "MyGO!!!!!" : "Ave Mujica";
                        return new Result
                        {
                            Title = x.Text ?? $"(出現角色:[{string.Join(", ", x.Characters.Select(x => x.Name))}])",
                            SubTitle = $"{soruce} Episode: {x.Episode}, Frames: {x.FrameStart}-{x.FrameEnd}",
                            Action = _ =>
                            {
                                HandleTomorinApiResultAction(x);
                                return true;
                            }
                        };
                    })
                    .ToList() ?? new List<Result>();
            }
            catch
            {
                return new List<Result>();
            }
        }

        private async void HandleTomorinApiResultAction(TomorinApiResult selectedResult)
        {
            var frameSoruce = selectedResult.Source == "mygo" ? "frame" : "ave-frames";
            var imageUrl = $"https://api-3.tomorin.cc/api/{frameSoruce}?episode={selectedResult.Episode}&frame_start={selectedResult.FrameStart}&frame_end={selectedResult.FrameEnd}";
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
