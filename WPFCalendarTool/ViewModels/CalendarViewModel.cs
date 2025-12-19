using Caliburn.Micro;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WPFCalendarTool.Models;
using WPFCalendarTool.Services;
using WPFCalendarTool.Utils;

namespace WPFCalendarTool.ViewModels
{
    public class CalendarViewModel : Caliburn.Micro.Screen
    {
        private Window? _window;
        private readonly IWindowManager _windowManager;
        private System.Timers.Timer _timer;
        private bool _isTimerActive;

        #region Bindings

        private bool _isWindowOpen;
        public bool IsWindowOpen
        {
            get => _isWindowOpen;
            set
            {
                if (_isWindowOpen != value)
                {
                    _isWindowOpen = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        private string timeNow;
        public string TimeNow
        {
            get => timeNow;
            set
            {
                timeNow = value;
                NotifyOfPropertyChange();
            }
        }

        private string dateNow;
        public string DateNow
        {
            get => dateNow;
            set
            {
                dateNow = value;
                NotifyOfPropertyChange();
            }
        }

        private string _weatherInfo;
        public string WeatherInfo
        {
            get => _weatherInfo;
            set
            {
                _weatherInfo = value;
                NotifyOfPropertyChange();
            }
        }

        private string weatherIcon;
        public string WeatherIcon
        {
            get => weatherIcon;
            set
            {
                if (weatherIcon != value)
                {
                    weatherIcon = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        private DateTime _currentMonth;
        public DateTime CurrentMonth
        {
            get => _currentMonth;
            set
            {
                _currentMonth = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => CurrentMonthText);
                GenerateCalendarDays();
            }
        }

        public string CurrentMonthText => CurrentMonth.ToString("yyyy年MM月");

        private ObservableCollection<CalendarDayModel> _calendarDays;
        /// <summary>
        /// 日历内容
        /// </summary>
        public ObservableCollection<CalendarDayModel> CalendarDays
        {
            get => _calendarDays;
            set
            {
                _calendarDays = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        public CalendarViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
            _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            CalendarDays = new ObservableCollection<CalendarDayModel>();

            TimeNow = DateTime.Now.ToString("HH:mm:ss");
            UpdateDateNow();

            // 初始化定时器
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;

            // 预加载节假日数据（不阻塞UI）
            _ = InitializeHolidaysAsync();

            GenerateCalendarDays();
        }

        /// <summary>
        /// 初始化节假日数据
        /// </summary>
        private async Task InitializeHolidaysAsync()
        {
            var currentYear = DateTime.Now.Year;
            // 预加载当前年份和下一年的节假日数据
            await HolidayService.PreloadHolidaysAsync(currentYear, currentYear + 1);

            // 重新生成日历以应用节假日数据
            System.Windows.Application.Current?.Dispatcher.Invoke(GenerateCalendarDays);
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_isTimerActive) return;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                TimeNow = DateTime.Now.ToString("HH:mm:ss");
                UpdateDateNow();
            });
        }

        private void UpdateDateNow()
        {
            var now = DateTime.Now;
            var weekDay = now.DayOfWeek switch
            {
                DayOfWeek.Monday => "星期一",
                DayOfWeek.Tuesday => "星期二",
                DayOfWeek.Wednesday => "星期三",
                DayOfWeek.Thursday => "星期四",
                DayOfWeek.Friday => "星期五",
                DayOfWeek.Saturday => "星期六",
                DayOfWeek.Sunday => "星期日",
                _ => ""
            };
            DateNow = $"{now:yyyy年MM月dd日} {weekDay}";
        }

        /// <summary>
        /// 生成日历日期
        /// </summary>
        private void GenerateCalendarDays()
        {
            CalendarDays.Clear();

            var firstDayOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // 获取第一天是星期几（1=星期一, 7=星期日）
            int firstDayOfWeek = ((int)firstDayOfMonth.DayOfWeek == 0) ? 7 : (int)firstDayOfMonth.DayOfWeek;

            // 填充上个月的日期
            var previousMonth = firstDayOfMonth.AddMonths(-1);
            var daysInPreviousMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
            for (int i = firstDayOfWeek - 1; i > 0; i--)
            {
                var day = daysInPreviousMonth - i + 1;
                var date = new DateTime(previousMonth.Year, previousMonth.Month, day);
                var (holidayType, holidayName) = HolidayService.GetHolidayInfo(date);

                CalendarDays.Add(new CalendarDayModel
                {
                    Day = day,
                    LunarText = GetLunarDay(date),
                    IsCurrentMonth = false,
                    IsToday = false,
                    IsWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday,
                    Date = date,
                    HolidayType = holidayType,
                    HolidayName = holidayName
                });
            }

            // 填充当前月的日期
            for (int day = 1; day <= lastDayOfMonth.Day; day++)
            {
                var date = new DateTime(CurrentMonth.Year, CurrentMonth.Month, day);
                var today = DateTime.Today;
                var (holidayType, holidayName) = HolidayService.GetHolidayInfo(date);

                CalendarDays.Add(new CalendarDayModel
                {
                    Day = day,
                    LunarText = GetLunarDay(date),
                    IsCurrentMonth = true,
                    IsToday = date == today,
                    IsWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday,
                    Date = date,
                    HolidayType = holidayType,
                    HolidayName = holidayName
                });
            }

            // 填充下个月的日期（填满42格 = 6周）
            var nextMonth = firstDayOfMonth.AddMonths(1);
            int remainingDays = 42 - CalendarDays.Count;
            for (int day = 1; day <= remainingDays; day++)
            {
                var date = new DateTime(nextMonth.Year, nextMonth.Month, day);
                var (holidayType, holidayName) = HolidayService.GetHolidayInfo(date);

                CalendarDays.Add(new CalendarDayModel
                {
                    Day = day,
                    LunarText = GetLunarDay(date),
                    IsCurrentMonth = false,
                    IsToday = false,
                    IsWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday,
                    Date = date,
                    HolidayType = holidayType,
                    HolidayName = holidayName
                });
            }
        }

        private string GetLunarDay(DateTime date)
        {
            return LunarCalendarHelper.GetLunarText(date);
        }

        public void OnCalendarDaysMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                PreviousMonth();
            else
                NextMonth();
        }

        // 上一个月
        public async void PreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);

            // 如果该年份数据未缓存，则加载
            if (!HolidayService.IsYearCached(CurrentMonth.Year))
            {
                await HolidayService.FetchHolidaysForYearAsync(CurrentMonth.Year);
                GenerateCalendarDays(); // 重新生成以应用新数据
            }
        }

        // 下一个月
        public async void NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);

            // 如果该年份数据未缓存，则加载
            if (!HolidayService.IsYearCached(CurrentMonth.Year))
            {
                await HolidayService.FetchHolidaysForYearAsync(CurrentMonth.Year);
                GenerateCalendarDays(); // 重新生成以应用新数据
            }
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            if (_window == null && view is Window win)
            {
                _window = win;
            }
        }

        protected override void OnViewLoaded(object view)
        {
            IsWindowOpen = true;
            base.OnViewLoaded(view);
            if (view is Window win)
            {
                win.Activate();
            }
        }

        public void ToggleTheme(object sender, RoutedEventArgs e)
        {
            ThemeManager.Instance.ToggleTheme();

            if (sender is System.Windows.Controls.Button button)
            {
                button.Content = ThemeManager.Instance.CurrentTheme switch
                {
                    ThemeType.Light => "🌙",
                    ThemeType.Dark => "🖥️",
                    ThemeType.System => "☀️",
                    _ => "🌙"
                };
                button.ToolTip = ThemeManager.Instance.CurrentTheme switch
                {
                    ThemeType.Light => "当前：亮色主题",
                    ThemeType.Dark => "当前：暗色主题",
                    ThemeType.System => "当前：跟随系统",
                    _ => "切换主题"
                };
            }
        }

        public void DragWindow(Window window, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                window.DragMove();
            }
        }

        public void OnWindowDeactivated(Window window)
        {
            // 留空，不自动隐藏
        }

        protected override Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            if (_window != null)
            {
                PositionWindowBottomRight(_window);
            }

            _isTimerActive = true;
            _timer?.Start();

            UpdateWeather();
            return base.OnActivatedAsync(cancellationToken);
        }

        private void PositionWindowBottomRight(Window window)
        {
            var workingArea = SystemParameters.WorkArea;
            window.Left = workingArea.Right - window.Width;
            window.Top = workingArea.Bottom - window.Height;
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _isTimerActive = false;
            _timer?.Stop();

            if (close)
            {
                if (_timer != null)
                {
                    _timer.Elapsed -= Timer_Elapsed;
                    _timer.Dispose();
                    _timer = null;
                }
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        public void HideWindow()
        {
            if (_window != null)
            {
                IsWindowOpen = false;
                _window.Hide();
            }
        }

        private async void UpdateWeather()
        {
            string apiKey = "S2uNHXu7T64WC-NGO";
            var weatherService = new WeatherService(apiKey);

            var now = await weatherService.GetNowWeatherAsync("xian");
            if (now != null)
            {
                var curTheme = ThemeManager.Instance.CurrentTheme;
                var themeStr = curTheme switch
                {
                    ThemeType.Light => "white",
                    ThemeType.Dark => "black",
                    ThemeType.System => "white",
                    _ => "white"
                };
                WeatherIcon = $"/Themes/Images/Weather/{themeStr}/{now.Now.Code}@1x.png";
                WeatherInfo = $"{now.Now.Text}, {now.Now.Temperature}°C";
            }
            else
            {
                WeatherInfo = "无法获取天气信息";
            }
        }
    }
}
