using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using WPFCalendarTool.ViewModels;

namespace WPFCalendarTool
{
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();

            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.Singleton<CalendarViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            // 初始化主题
            ThemeManager.Instance.ApplyTheme(ThemeType.System);

            // 托盘菜单
            _contextMenu = new ContextMenuStrip();
            var openItem = new ToolStripMenuItem("打开日历", null, (s, args) => ShowCalendar());
            var exitItem = new ToolStripMenuItem("退出", null, (s, args) => System.Windows.Application.Current.Shutdown());
            _contextMenu.Items.Add(openItem);
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add(exitItem);

            // 托盘图标
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "天气.ico");
            System.Drawing.Icon trayIcon = File.Exists(iconPath)
                ? new System.Drawing.Icon(iconPath)
                : System.Drawing.SystemIcons.Information;

            _notifyIcon = new NotifyIcon
            {
                Icon = trayIcon,
                Visible = true,
                Text = "日历工具",
                ContextMenuStrip = _contextMenu
            };
            _notifyIcon.MouseDown += NotifyIcon_MouseDown;

            await DisplayRootViewForAsync<CalendarViewModel>();
        }

        private void NotifyIcon_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var vm = _container.GetInstance<CalendarViewModel>();

                // 查找窗口
                var existWindow = System.Windows.Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.DataContext == vm);

                if (existWindow != null && existWindow.IsVisible)
                {
                    // 窗口可见则隐藏
                    vm.HideWindow();
                }
                else
                {
                    // 窗口不可见或不存在则显示
                    ShowCalendar();
                }
            }
        }

        private void ShowCalendar()
        {
            var vm = _container.GetInstance<CalendarViewModel>();

            // 查找已存在的窗口
            var existWindow = System.Windows.Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == vm);

            if (existWindow != null)
            {
                // 窗口已存在，显示并激活
                existWindow.Show();
                if (existWindow.WindowState == WindowState.Minimized)
                    existWindow.WindowState = WindowState.Normal;
                existWindow.Activate();
                existWindow.Topmost = true;
                existWindow.Topmost = false;
                vm.IsWindowOpen = true;
            }
            else
            {
                // 不存在则新建
                var windowManager = _container.GetInstance<IWindowManager>();
                windowManager.ShowWindowAsync(vm);
            }
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            ThemeManager.Instance.Dispose();
            _notifyIcon?.Dispose();
            _contextMenu?.Dispose();
            base.OnExit(sender, e);
        }
    }
}
