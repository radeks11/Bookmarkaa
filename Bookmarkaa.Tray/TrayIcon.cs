using Bookmarkaa.Managers;

namespace Bookmarkaa.Tray
{
    public class TrayIcon
    {
        private NotifyIcon _notifyIcon = null!;
        private HotKeyManager _hotKeyManager = null!;
        private MainWindow? _mainWindow;
        private SettingsWindow? _settingsWindow;

        public void Initialize(bool showMainWindow = false)
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = LoadIcon(),
                Text = "Bookmarkaa",
                Visible = true
            };

            _notifyIcon.Click += (_, e) =>
            {
                if (e is MouseEventArgs me && me.Button == MouseButtons.Left)
                    ShowMainWindow();
            };

            _notifyIcon.ContextMenuStrip = BuildMenu();

            _hotKeyManager = new HotKeyManager();
            RegisterHotKey();

            if (showMainWindow)
                ShowMainWindow();
        }

        private ContextMenuStrip BuildMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Otwórz", null, (_, _) => ShowMainWindow());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Ustawienia", null, (_, _) => ShowSettings());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Zamknij", null, (_, _) => Shutdown());
            return menu;
        }

        private void RegisterHotKey()
        {
            _hotKeyManager.Register(
                SettingsManager.Settings.HotKeyModifiers,
                (Keys)SettingsManager.Settings.HotKeyCode,
                ShowMainWindow);
        }

        public void ShowMainWindow()
        {
            if (_mainWindow != null && _mainWindow.IsVisible)
            {
                _mainWindow.Activate();
                return;
            }

            _mainWindow = new MainWindow();
            _mainWindow.Closed += (_, _) => _mainWindow = null;
            _mainWindow.Show();
        }

        private void ShowSettings()
        {
            if (_settingsWindow != null)
            {
                _settingsWindow.Activate();
                return;
            }

            _settingsWindow = new SettingsWindow();
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;

            if (_settingsWindow.ShowDialog() == true)
            {
                _hotKeyManager.Dispose();
                _hotKeyManager = new HotKeyManager();
                RegisterHotKey();
            }
        }

        private void Shutdown()
        {
            _hotKeyManager?.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        private static Icon LoadIcon()
        {
            var uri = new Uri(Helpers.Constants.DefaultIconPath);
            var stream = System.Windows.Application.GetResourceStream(uri)?.Stream;
            return stream != null ? new Icon(stream) : SystemIcons.Application;
        }
    }
}
