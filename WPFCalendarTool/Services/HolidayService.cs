using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WPFCalendarTool.Models;

namespace WPFCalendarTool.Services
{
    /// <summary>
    /// 节假日服务
    /// </summary>
    public class HolidayService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Dictionary<int, Dictionary<string, HolidayData>> _holidayCache = new();
        private static readonly object _lock = new object();

        /// <summary>
        /// 从API获取指定年份的节假日数据
        /// </summary>
        public static async Task<bool> FetchHolidaysForYearAsync(int year)
        {
            try
            {
                var url = $"https://api.jiejiariapi.com/v1/holidays/{year}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"获取{year}年节假日数据失败: HTTP {response.StatusCode}");
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<HolidayApiResponse>(json);

                if (apiResponse == null || apiResponse.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"获取{year}年节假日数据为空");
                    return false;
                }

                lock (_lock)
                {
                    _holidayCache[year] = apiResponse;
                }

                System.Diagnostics.Debug.WriteLine($"成功获取{year}年节假日数据，共{apiResponse.Count}条");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"获取{year}年节假日数据异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取指定日期的节假日信息
        /// </summary>
        public static (HolidayType Type, string Name) GetHolidayInfo(DateTime date)
        {
            var year = date.Year;
            var dateStr = date.ToString("yyyy-MM-dd");

            lock (_lock)
            {
                if (_holidayCache.TryGetValue(year, out var yearData))
                {
                    if (yearData.TryGetValue(dateStr, out var holidayData))
                    {
                        // 判断节假日类型
                        var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

                        if (holidayData.IsOffDay)
                        {
                            // 如果是休息日
                            if (isWeekend && string.IsNullOrEmpty(holidayData.Name))
                            {
                                // 普通周末
                                return (HolidayType.RestDay, string.Empty);
                            }
                            else
                            {
                                // 法定节假日
                                return (HolidayType.Holiday, holidayData.Name);
                            }
                        }
                        else
                        {
                            // isOffDay = false 表示调休工作日
                            return (HolidayType.WorkDay, holidayData.Name);
                        }
                    }
                }
            }

            // 如果缓存中没有数据，按普通周末判断
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return (HolidayType.RestDay, string.Empty);
            }

            return (HolidayType.None, string.Empty);
        }

        /// <summary>
        /// 判断是否为休息日（包括法定节假日和普通周末）
        /// </summary>
        public static bool IsRestDay(DateTime date)
        {
            var (type, _) = GetHolidayInfo(date);

            // 如果是调休工作日，则不是休息日
            if (type == HolidayType.WorkDay)
                return false;

            // 如果是法定节假日或普通休息日，则是休息日
            return type == HolidayType.Holiday || type == HolidayType.RestDay;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public static void ClearCache()
        {
            lock (_lock)
            {
                _holidayCache.Clear();
            }
        }

        /// <summary>
        /// 预加载多个年份的数据
        /// </summary>
        public static async Task PreloadHolidaysAsync(params int[] years)
        {
            var tasks = new List<Task<bool>>();
            foreach (var year in years)
            {
                tasks.Add(FetchHolidaysForYearAsync(year));
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 检查指定年份的数据是否已缓存
        /// </summary>
        public static bool IsYearCached(int year)
        {
            lock (_lock)
            {
                return _holidayCache.ContainsKey(year);
            }
        }
    }
}