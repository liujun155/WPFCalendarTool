using System;
using System.Windows;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace WPFCalendarTool
{
    public enum ThemeType
    {
        Light,
        Dark,
        System // 跟随系统
    }

    public class ThemeManager
    {
        private static ThemeManager _instance;
        private static readonly object _lock = new object();
        private ThemeType _currentTheme;
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string RegistryValueName = "AppsUseLightTheme";

        public static ThemeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ThemeManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public ThemeType CurrentTheme
        {
            get => _currentTheme;
            private set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ThemeChanged?.Invoke(this, value);
                }
            }
        }

        public event EventHandler<ThemeType> ThemeChanged;

        private ThemeManager()
        {
            _currentTheme = ThemeType.System;
            // 监听系统主题变化
            SystemEvents.UserPreferenceChanged += OnSystemThemeChanged;
        }

        /// <summary>
        /// 检测操作系统当前是否使用暗色主题
        /// </summary>
        /// <returns>true 表示暗色主题，false 表示亮色主题</returns>
        public bool IsSystemDarkTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
                var value = key?.GetValue(RegistryValueName);

                if (value is int intValue)
                {
                    // 0 = 暗色, 1 = 亮色
                    return intValue == 0;
                }
            }
            catch
            {
                // 如果无法读取注册表，默认返回亮色主题
            }

            return false;
        }

        /// <summary>
        /// 获取系统主题类型
        /// </summary>
        public ThemeType GetSystemThemeType()
        {
            return IsSystemDarkTheme() ? ThemeType.Dark : ThemeType.Light;
        }

        /// <summary>
        /// 应用指定主题
        /// </summary>
        public void ApplyTheme(ThemeType theme)
        {
            // 如果选择跟随系统，则获取系统实际主题
            var actualTheme = theme == ThemeType.System ? GetSystemThemeType() : theme;

            var themeUri = actualTheme switch
            {
                ThemeType.Light => new Uri("Themes/LightTheme.xaml", UriKind.Relative),
                ThemeType.Dark => new Uri("Themes/DarkTheme.xaml", UriKind.Relative),
                _ => throw new ArgumentOutOfRangeException(nameof(actualTheme))
            };

            var resources = Application.Current.Resources;

            // 移除旧主题
            var oldTheme = resources.MergedDictionaries.FirstOrDefault(
                d => d.Source?.OriginalString.Contains("Theme.xaml") == true);
            if (oldTheme != null)
            {
                resources.MergedDictionaries.Remove(oldTheme);
            }

            // 加载新主题
            var newTheme = new ResourceDictionary { Source = themeUri };
            resources.MergedDictionaries.Add(newTheme);

            CurrentTheme = theme;
        }

        /// <summary>
        /// 切换主题（亮色 -> 暗色 -> 跟随系统 -> 亮色）
        /// </summary>
        public void ToggleTheme()
        {
            var newTheme = CurrentTheme switch
            {
                ThemeType.Light => ThemeType.Dark,
                ThemeType.Dark => ThemeType.System,
                ThemeType.System => ThemeType.Light,
                _ => ThemeType.Light
            };
            ApplyTheme(newTheme);
        }

        /// <summary>
        /// 系统主题变化事件处理
        /// </summary>
        private void OnSystemThemeChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // 只有当当前设置为跟随系统时才响应系统主题变化
            if (CurrentTheme == ThemeType.System && e.Category == UserPreferenceCategory.General)
            {
                ApplyTheme(ThemeType.System);
            }
        }

        /// <summary>
        /// 获取当前实际应用的主题（解析 System 为具体主题）
        /// </summary>
        public ThemeType GetActualTheme()
        {
            return CurrentTheme == ThemeType.System ? GetSystemThemeType() : CurrentTheme;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            SystemEvents.UserPreferenceChanged -= OnSystemThemeChanged;
        }
    }
}