using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WPFCalendarTool.Utils;

namespace WPFCalendarTool.Services
{
    public class WeatherService
    {
        private readonly string _apiKey;

        /// <summary>
        /// 构造函数，传入心知天气 API Key
        /// </summary>
        /// <param name="apiKey">心知天气 API Key</param>
        public WeatherService(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        /// <summary>
        /// 获取当前天气
        /// </summary>
        /// <param name="location">城市名或地名（如 "shenzhen"）</param>
        /// <returns>WeatherNow 对象</returns>
        public async Task<WeatherNow> GetNowWeatherAsync(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentNullException(nameof(location));

            string url = $"https://api.seniverse.com/v3/weather/now.json" +
                         $"?key={_apiKey}&location={location}&language=zh-Hans&unit=c";

            // 调用静态 HttpHelper
            string json = await HttpHelper.GetAsync(url);

            var result = JsonSerializer.Deserialize<WeatherResult>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Results?[0];
        }
    }

    public class WeatherResult
    {
        public List<WeatherNow> Results { get; set; }
    }

    public class WeatherNow
    {
        public Location Location { get; set; }
        public Now Now { get; set; }
        public string Last_Updated { get; set; }
    }

    public class Location
    {
        public string Name { get; set; }
        public string Country { get; set; }
    }

    public class Now
    {
        public string Text { get; set; }
        public string Code { get; set; }
        public string Temperature { get; set; }
    }
}
