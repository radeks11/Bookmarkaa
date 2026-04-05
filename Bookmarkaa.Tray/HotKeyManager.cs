using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;

namespace Bookmarkaa.Tray
{
    public class HotKeyManager : IDisposable
    {
        public const int MOD_ALT = 0x0001;
        public const int MOD_CTRL = 0x0002;
        public const int MOD_SHIFT = 0x0004;
        public const int MOD_WIN = 0x0008;

        private const int WM_HOTKEY = 0x0312;
        private static int _idCounter = 9000;

        private readonly Dictionary<int, Action> _handlers = new();

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public HotKeyManager()
        {
            ComponentDispatcher.ThreadPreprocessMessage += OnThreadMessage;
        }

        public void Register(int modifiers, Keys key, Action handler)
        {
            int id = _idCounter++;
            RegisterHotKey(IntPtr.Zero, id, modifiers, (int)key);
            _handlers[id] = handler;
        }

        private void OnThreadMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message == WM_HOTKEY && _handlers.TryGetValue((int)msg.wParam, out var handler))
            {
                handler();
                handled = true;
            }
        }

        public void Dispose()
        {
            ComponentDispatcher.ThreadPreprocessMessage -= OnThreadMessage;
            foreach (int id in _handlers.Keys)
                UnregisterHotKey(IntPtr.Zero, id);
        }
    }
}
