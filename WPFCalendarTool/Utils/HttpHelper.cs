using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WPFCalendarTool.Utils
{
    public static class HttpHelper
    {
        // 全局 HttpClient 最轻量安全的写法
        private static readonly HttpClient _client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)  // 默认超时
        };

        /// <summary>
        /// 添加默认 Header（比如 Token）
        /// </summary>
        public static void AddDefaultHeader(string key, string value)
        {
            if (_client.DefaultRequestHeaders.Contains(key))
                _client.DefaultRequestHeaders.Remove(key);

            _client.DefaultRequestHeaders.Add(key, value);
        }

        /// <summary>
        /// GET 请求，返回字符串
        /// </summary>
        public static async Task<string> GetAsync(string url)
        {
            var res = await _client.GetAsync(url);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// GET 请求 + 反序列化
        /// </summary>
        public static async Task<T?> GetJsonAsync<T>(string url)
        {
            var json = await GetAsync(url);
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// POST 表单（application/x-www-form-urlencoded）
        /// </summary>
        public static async Task<string> PostFormAsync(string url, Dictionary<string, string> form)
        {
            var content = new FormUrlEncodedContent(form);
            var res = await _client.PostAsync(url, content);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// POST JSON
        /// </summary>
        public static async Task<string> PostJsonAsync<T>(string url, T data)
        {
            string json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _client.PostAsync(url, content);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// POST JSON + 自动反序列化
        /// </summary>
        public static async Task<TResult?> PostJsonAsync<TBody, TResult>(string url, TBody body)
        {
            string json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _client.PostAsync(url, content);
            res.EnsureSuccessStatusCode();

            string resultJson = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TResult>(resultJson);
        }
    }
}
