using Bookmarkaa.Helpers;
using Bookmarkaa.Managers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Bookmarkaa.Tray
{
    public partial class SettingsWindow : Window
    {
        // ── Skrót główny ─────────────────────────────────────────────────────
        private int _modifiers;
        private int _keyCode;

        // ── Skrót duplikowania ───────────────────────────────────────────────
        private int _dupModifiers;
        private int _dupKeyCode;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            ChkStartup.IsChecked = StartupManager.IsEnabled();

            _modifiers = SettingsManager.Settings.HotKeyModifiers;
            _keyCode   = SettingsManager.Settings.HotKeyCode;
            TxtHotKey.Text = FormatHotKey(_modifiers, _keyCode);

            TxtIconsFolder.Text = SettingsManager.Settings.DefaultIconsFolder;

            ChkDuplicateTab.IsChecked = SettingsManager.Settings.DuplicateTabEnabled;
            _dupModifiers = SettingsManager.Settings.DuplicateTabHotKeyModifiers;
            _dupKeyCode   = SettingsManager.Settings.DuplicateTabHotKeyCode;
            TxtDupHotKey.Text = FormatHotKey(_dupModifiers, _dupKeyCode);
            UpdateDupHotKeyEnabled();

            PanelTabsInfo.Visibility = FileExplorerProcess.ExplorerTabsSupported
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        // ── Skrót główny ─────────────────────────────────────────────────────

        private void TxtHotKey_GotFocus(object sender, RoutedEventArgs e)
            => TxtHotKey.Text = "Naciśnij skrót...";

        private void TxtHotKey_LostFocus(object sender, RoutedEventArgs e)
            => TxtHotKey.Text = FormatHotKey(_modifiers, _keyCode);

        private void TxtHotKey_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (IsModifierKey(key)) return;

            _modifiers = ReadModifiers();
            _keyCode   = KeyInterop.VirtualKeyFromKey(key);
            TxtHotKey.Text = FormatHotKey(_modifiers, _keyCode);
        }

        // ── Skrót duplikowania ───────────────────────────────────────────────

        private void TxtDupHotKey_GotFocus(object sender, RoutedEventArgs e)
            => TxtDupHotKey.Text = "Naciśnij skrót...";

        private void TxtDupHotKey_LostFocus(object sender, RoutedEventArgs e)
            => TxtDupHotKey.Text = FormatHotKey(_dupModifiers, _dupKeyCode);

        private void TxtDupHotKey_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (IsModifierKey(key)) return;

            _dupModifiers = ReadModifiers();
            _dupKeyCode   = KeyInterop.VirtualKeyFromKey(key);
            TxtDupHotKey.Text = FormatHotKey(_dupModifiers, _dupKeyCode);
        }

        private void ChkDuplicateTab_Changed(object sender, RoutedEventArgs e)
            => UpdateDupHotKeyEnabled();

        private void UpdateDupHotKeyEnabled()
        {
            bool enabled = ChkDuplicateTab.IsChecked == true;
            TxtDupHotKey.IsEnabled = enabled;
            LblDupHotKey.IsEnabled = enabled;
        }

        // ── Folder ikon ──────────────────────────────────────────────────────

        private void BtnBrowseIconsFolder_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description  = "Wybierz folder ikon",
                SelectedPath = TxtIconsFolder.Text
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                TxtIconsFolder.Text = dialog.SelectedPath;
        }

        // ── Zapisz / Anuluj ──────────────────────────────────────────────────

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Settings.RunAtStartup         = ChkStartup.IsChecked == true;
            SettingsManager.Settings.HotKeyModifiers      = _modifiers;
            SettingsManager.Settings.HotKeyCode           = _keyCode;
            SettingsManager.Settings.DefaultIconsFolder   = TxtIconsFolder.Text;

            SettingsManager.Settings.DuplicateTabEnabled           = ChkDuplicateTab.IsChecked == true;
            SettingsManager.Settings.DuplicateTabHotKeyModifiers   = _dupModifiers;
            SettingsManager.Settings.DuplicateTabHotKeyCode        = _dupKeyCode;

            SettingsManager.SaveSettings();
            StartupManager.Apply(SettingsManager.Settings.RunAtStartup);

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static bool IsModifierKey(Key key) =>
            key is Key.LeftCtrl or Key.RightCtrl
                or Key.LeftAlt  or Key.RightAlt
                or Key.LeftShift or Key.RightShift
                or Key.LWin or Key.RWin;

        private static int ReadModifiers()
        {
            int mods = 0;
            if (Keyboard.IsKeyDown(Key.LeftCtrl)  || Keyboard.IsKeyDown(Key.RightCtrl))  mods |= HotKeyManager.MOD_CTRL;
            if (Keyboard.IsKeyDown(Key.LeftAlt)   || Keyboard.IsKeyDown(Key.RightAlt))   mods |= HotKeyManager.MOD_ALT;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) mods |= HotKeyManager.MOD_SHIFT;
            if (Keyboard.IsKeyDown(Key.LWin)      || Keyboard.IsKeyDown(Key.RWin))       mods |= HotKeyManager.MOD_WIN;
            return mods;
        }

        private static string FormatHotKey(int modifiers, int keyCode)
        {
            var parts = new List<string>();
            if ((modifiers & HotKeyManager.MOD_WIN)   != 0) parts.Add("Win");
            if ((modifiers & HotKeyManager.MOD_CTRL)  != 0) parts.Add("Ctrl");
            if ((modifiers & HotKeyManager.MOD_ALT)   != 0) parts.Add("Alt");
            if ((modifiers & HotKeyManager.MOD_SHIFT) != 0) parts.Add("Shift");
            parts.Add(((Keys)keyCode).ToString());
            return string.Join("+", parts);
        }
    }
}
