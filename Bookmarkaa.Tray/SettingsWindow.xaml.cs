using Bookmarkaa.Managers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Bookmarkaa.Tray
{
    public partial class SettingsWindow : Window
    {
        private int _modifiers;
        private int _keyCode;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            ChkStartup.IsChecked = StartupManager.IsEnabled();
            _modifiers = SettingsManager.Settings.HotKeyModifiers;
            _keyCode = SettingsManager.Settings.HotKeyCode;
            TxtHotKey.Text = FormatHotKey(_modifiers, _keyCode);
        }

        private void TxtHotKey_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtHotKey.Text = "Naciśnij skrót...";
        }

        private void TxtHotKey_LostFocus(object sender, RoutedEventArgs e)
        {
            TxtHotKey.Text = FormatHotKey(_modifiers, _keyCode);
        }

        private void TxtHotKey_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;

            // Ignoruj same modyfikatory
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
                or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
                return;

            int mods = 0;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                mods |= HotKeyManager.MOD_CTRL;
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                mods |= HotKeyManager.MOD_ALT;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                mods |= HotKeyManager.MOD_SHIFT;
            if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
                mods |= HotKeyManager.MOD_WIN;

            int vk = KeyInterop.VirtualKeyFromKey(key);

            _modifiers = mods;
            _keyCode = vk;
            TxtHotKey.Text = FormatHotKey(_modifiers, _keyCode);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Settings.RunAtStartup = ChkStartup.IsChecked == true;
            SettingsManager.Settings.HotKeyModifiers = _modifiers;
            SettingsManager.Settings.HotKeyCode = _keyCode;
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

        private static string FormatHotKey(int modifiers, int keyCode)
        {
            var parts = new List<string>();
            if ((modifiers & HotKeyManager.MOD_WIN) != 0)   parts.Add("Win");
            if ((modifiers & HotKeyManager.MOD_CTRL) != 0)  parts.Add("Ctrl");
            if ((modifiers & HotKeyManager.MOD_ALT) != 0)   parts.Add("Alt");
            if ((modifiers & HotKeyManager.MOD_SHIFT) != 0) parts.Add("Shift");

            var key = (Keys)keyCode;
            parts.Add(key.ToString());

            return string.Join("+", parts);
        }
    }
}
