using System;
using System.Globalization;

namespace WPFCalendarTool.Utils
{
    /// <summary>
    /// 农历转换帮助类
    /// </summary>
    public static class LunarCalendarHelper
    {
        private static readonly ChineseLunisolarCalendar _chineseCalendar = new ChineseLunisolarCalendar();

        private static readonly string[] _lunarDayNames =
        {
            "初一", "初二", "初三", "初四", "初五", "初六", "初七", "初八", "初九", "初十",
            "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九", "二十",
            "廿一", "廿二", "廿三", "廿四", "廿五", "廿六", "廿七", "廿八", "廿九", "三十"
        };

        private static readonly string[] _lunarMonthNames =
        {
            "正月", "二月", "三月", "四月", "五月", "六月",
            "七月", "八月", "九月", "十月", "冬月", "腊月"
        };

        // 节气数据（近似算法，精确到分钟级别需要更复杂的天文算法）
        private static readonly string[] _solarTerms =
        {
            "小寒", "大寒", "立春", "雨水", "惊蛰", "春分",
            "清明", "谷雨", "立夏", "小满", "芒种", "夏至",
            "小暑", "大暑", "立秋", "处暑", "白露", "秋分",
            "寒露", "霜降", "立冬", "小雪", "大雪", "冬至"
        };

        // 传统节日
        private static readonly (int Month, int Day, string Name)[] _traditionalFestivals =
        {
            (1, 1, "春节"),
            (1, 15, "元宵"),
            (2, 2, "龙抬头"),
            (5, 5, "端午"),
            (7, 7, "七夕"),
            (7, 15, "中元"),
            (8, 15, "中秋"),
            (9, 9, "重阳"),
            (12, 8, "腊八"),
            (12, 23, "小年")
        };

        // 公历节日
        private static readonly (int Month, int Day, string Name)[] _gregorianFestivals =
        {
            (1, 1, "元旦"),
            (2, 14, "情人节"),
            (3, 8, "妇女节"),
            (3, 12, "植树节"),
            (4, 1, "愚人节"),
            (5, 1, "劳动节"),
            (5, 4, "青年节"),
            (6, 1, "儿童节"),
            (7, 1, "建党节"),
            (8, 1, "建军节"),
            (9, 10, "教师节"),
            (10, 1, "国庆节"),
            (12, 25, "圣诞节")
        };

        /// <summary>
        /// 获取农历日期文本（优先显示节日、节气，否则显示农历日期）
        /// </summary>
        public static string GetLunarText(DateTime date)
        {
            // 1. 检查公历节日
            var gregorianFestival = GetGregorianFestival(date);
            if (!string.IsNullOrEmpty(gregorianFestival))
                return gregorianFestival;

            // 2. 检查节气
            var solarTerm = GetSolarTerm(date);
            if (!string.IsNullOrEmpty(solarTerm))
                return solarTerm;

            try
            {
                // 3. 获取农历日期
                int lunarYear = _chineseCalendar.GetYear(date);
                int lunarMonth = _chineseCalendar.GetMonth(date);
                int lunarDay = _chineseCalendar.GetDayOfMonth(date);

                // 处理闰月
                int leapMonth = _chineseCalendar.GetLeapMonth(lunarYear);
                if (leapMonth > 0 && lunarMonth >= leapMonth)
                {
                    lunarMonth--;
                }

                // 4. 检查农历节日
                var lunarFestival = GetLunarFestival(lunarMonth, lunarDay);
                if (!string.IsNullOrEmpty(lunarFestival))
                    return lunarFestival;

                // 5. 农历初一显示月份，否则显示日期
                if (lunarDay == 1)
                {
                    return _lunarMonthNames[lunarMonth - 1];
                }

                return _lunarDayNames[lunarDay - 1];
            }
            catch
            {
                // 超出农历范围时返回空
                return "";
            }
        }

        /// <summary>
        /// 获取公历节日
        /// </summary>
        private static string GetGregorianFestival(DateTime date)
        {
            foreach (var festival in _gregorianFestivals)
            {
                if (date.Month == festival.Month && date.Day == festival.Day)
                    return festival.Name;
            }

            // 特殊节日：清明节（4月4-6日之间）
            if (date.Month == 4 && date.Day >= 4 && date.Day <= 6)
            {
                // 简化处理，实际清明节日期需要更精确计算
                var qingming = GetQingmingDate(date.Year);
                if (date.Day == qingming.Day)
                    return "清明节";
            }

            // 母亲节：5月第二个星期日
            if (date.Month == 5 && date.DayOfWeek == DayOfWeek.Sunday)
            {
                int weekOfMonth = (date.Day - 1) / 7 + 1;
                if (weekOfMonth == 2)
                    return "母亲节";
            }

            // 父亲节：6月第三个星期日
            if (date.Month == 6 && date.DayOfWeek == DayOfWeek.Sunday)
            {
                int weekOfMonth = (date.Day - 1) / 7 + 1;
                if (weekOfMonth == 3)
                    return "父亲节";
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取农历节日
        /// </summary>
        private static string GetLunarFestival(int month, int day)
        {
            foreach (var festival in _traditionalFestivals)
            {
                if (month == festival.Month && day == festival.Day)
                    return festival.Name;
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取节气（简化算法）
        /// </summary>
        private static string GetSolarTerm(DateTime date)
        {
            // 使用简化的节气计算（基于月份和日期范围）
            var termDates = new (int Month, int Day1, int Day2, string Term1, string Term2)[]
            {
                (1, 5, 20, "小寒", "大寒"),
                (2, 4, 19, "立春", "雨水"),
                (3, 6, 21, "惊蛰", "春分"),
                (4, 5, 20, "清明", "谷雨"),
                (5, 6, 21, "立夏", "小满"),
                (6, 6, 22, "芒种", "夏至"),
                (7, 7, 23, "小暑", "大暑"),
                (8, 8, 23, "立秋", "处暑"),
                (9, 8, 23, "白露", "秋分"),
                (10, 8, 23, "寒露", "霜降"),
                (11, 7, 22, "立冬", "小雪"),
                (12, 7, 22, "大雪", "冬至")
            };

            foreach (var term in termDates)
            {
                if (date.Month == term.Month)
                {
                    if (Math.Abs(date.Day - term.Day1) <= 1)
                        return term.Term1;
                    if (Math.Abs(date.Day - term.Day2) <= 1)
                        return term.Term2;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取清明节日期（简化算法）
        /// </summary>
        private static DateTime GetQingmingDate(int year)
        {
            // 清明节简化计算公式
            int day = (int)((year % 4) * 0.2422 + 4.81);
            if (year == 2008) day = 4; // 特例修正
            return new DateTime(year, 4, day);
        }

        /// <summary>
        /// 获取完整农历日期字符串（用于详细显示）
        /// </summary>
        public static string GetFullLunarDate(DateTime date)
        {
            try
            {
                int lunarYear = _chineseCalendar.GetYear(date);
                int lunarMonth = _chineseCalendar.GetMonth(date);
                int lunarDay = _chineseCalendar.GetDayOfMonth(date);

                int leapMonth = _chineseCalendar.GetLeapMonth(lunarYear);
                string leapText = "";
                if (leapMonth > 0 && lunarMonth == leapMonth)
                {
                    leapText = "闰";
                    lunarMonth--;
                }

                return $"农历{lunarYear}年{leapText}{_lunarMonthNames[lunarMonth - 1]}{_lunarDayNames[lunarDay - 1]}";
            }
            catch
            {
                return "";
            }
        }
    }
}