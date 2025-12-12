using Caliburn.Micro;
using System;
using System.Text.Json;
using System.Windows;
using WPFCalendarTool;
using WPFCalendarTool.Services;
using WPFCalendarTool.Utils;

namespace WPFCalendarTool.ViewModels
{
    public class CalendarViewModel : Caliburn.Micro.Screen
    {
        private Window? _window;
        private readonly IWindowManager _windowManager;

        private System.Timers.Timer _timer;

        #region Bindings
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

        public string DateNow => DateTime.Now.ToString("yyyy年MM月dd日");

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
        #endregion

        private bool _isTimerActive;

        public CalendarViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;

            TimeNow = DateTime.Now.ToString("HH:mm:ss");
            _timer = new System.Timers.Timer(1000);
            _isTimerActive = true;
            _timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_isTimerActive) return;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                TimeNow = DateTime.Now.ToString("HH:mm:ss");
            });
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
            base.OnViewLoaded(view);
            if (view is Window win)
            {
                win.Activate();
            }
        }

        // 主题切换
        public void ToggleTheme(object sender, RoutedEventArgs e)
        {
            ThemeManager.Instance.ToggleTheme();

            // 更新按钮内容和提示（可通过事件或数据绑定实现，推荐用绑定）
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

        // 拖动窗口
        public void DragWindow(Window window, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                window.DragMove();
            }
        }

        // 失去焦点时隐藏窗口
        public void OnWindowDeactivated(Window window)
        {
            window.Hide();
        }

        protected override Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            if (_window != null)
            {
                PositionWindowBottomRight(_window);
            }
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
            if (_timer != null)
            {
                _timer.Elapsed -= Timer_Elapsed;
                _timer.Stop();
                _timer.Dispose();
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #region 获取天气
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

        private async void UpdateWeather()
        {
            string apiKey = "S2uNHXu7T64WC-NGO";  // 替换为你的心知天气 Key

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
                };
                WeatherIcon = $"/Themes/Images/Weather/{themeStr}/{now.Now.Code}@1x.png";
                WeatherInfo = $"{now.Now.Text}, {now.Now.Temperature}°C";
            }
            else
            {
                WeatherInfo = "无法获取天气信息";
            }
        }
        #endregion
    }
}
