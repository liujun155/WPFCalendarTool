using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WPFCalendarTool.Models
{
    /// <summary>
    /// 节假日API响应（字典格式）
    /// </summary>
    public class HolidayApiResponse : Dictionary<string, HolidayData>
    {
    }

    /// <summary>
    /// 节假日数据
    /// </summary>
    public class HolidayData
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("isOffDay")]
        public bool IsOffDay { get; set; }
    }
}