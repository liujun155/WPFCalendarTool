using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCalendarTool.Models
{
    public class CalendarDayModel
    {
        public int Day { get; set; }
        public string LunarText { get; set; } = string.Empty;
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool IsWeekend { get; set; }
        public bool IsSelected { get; set; }
        public DateTime Date { get; set; }

        // 假日信息
        public HolidayType HolidayType { get; set; } = HolidayType.None;
        public string HolidayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 假日类型枚举
    /// </summary>
    public enum HolidayType
    {
        None,           // 普通工作日
        Holiday,        // 法定节假日（休息）
        RestDay,        // 周末休息日
        WorkDay         // 调休补班日
    }
}
