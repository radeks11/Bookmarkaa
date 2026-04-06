using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Bookmarkaa.Helpers
{
    public class FileExplorerProcess
    {
        /// <summary>
        /// Minimalna wersja kompilacji Windows 11 obsługująca zakładki w Eksploratorze (22H2).
        /// </summary>
        public const int MinBuildForTabs = 22621;

        /// <summary>
        /// Czy bieżący system operacyjny obsługuje otwieranie folderów w zakładkach Eksploratora.
        /// </summary>
        public static bool ExplorerTabsSupported
            => Environment.OSVersion.Version.Build >= MinBuildForTabs;

        public FileExplorerProcess(List<string> folders)
        {
            if (folders == null || folders.Count == 0)
                return;

            var validFolders = folders
                .Select(GetFolderPath)
                .Where(f => !string.IsNullOrEmpty(f))
                .Distinct()
                .ToList();

            if (validFolders.Count == 0)
                return;

            if (validFolders.Count == 1 || !ExplorerTabsSupported)
            {
                Process.Start("explorer.exe", $"\"{validFolders[0]}\"");
                return;
            }

            _ = OpenFoldersInTabsAsync(validFolders);
        }

        private static async Task OpenFoldersInTabsAsync(List<string> folders)
        {
            var existingHwnds = GetExplorerHwnds();

            Process.Start("explorer.exe", $"\"{folders[0]}\"");

            // Czekaj aż pojawi się nowe okno Eksploratora (do 5 s)
            IntPtr hwnd = IntPtr.Zero;
            for (int i = 0; i < 50 && hwnd == IntPtr.Zero; i++)
            {
                await Task.Delay(100);
                hwnd = GetExplorerHwnds().FirstOrDefault(h => !existingHwnds.Contains(h));
            }

            if (hwnd == IntPtr.Zero)
                return;

            bool hadText = System.Windows.Clipboard.ContainsText();
            string savedClipboard = hadText ? System.Windows.Clipboard.GetText() : string.Empty;

            try
            {
                foreach (var folder in folders.Skip(1))
                    await OpenFolderInNewTabAsync(hwnd, folder);
            }
            finally
            {
                if (hadText)
                    System.Windows.Clipboard.SetText(savedClipboard);
                else
                    System.Windows.Clipboard.Clear();
            }
        }

        /// <summary>
        /// Otwiera podaną ścieżkę jako nową zakładkę w istniejącym oknie Eksploratora.
        /// Wymaga Windows 11 22H2 lub nowszego.
        /// </summary>
        public static async Task OpenInNewTabAsync(IntPtr explorerHwnd, string path)
            => await OpenFolderInNewTabAsync(explorerHwnd, path);

        private static async Task OpenFolderInNewTabAsync(IntPtr hwnd, string folderPath)
        {
            SetForegroundWindow(hwnd);
            await Task.Delay(150);

            SendKeyCombo(VK_LCONTROL, VK_T);    // Ctrl+T – nowa zakładka
            await Task.Delay(400);

            SendKeyCombo(VK_LCONTROL, VK_L);    // Ctrl+L – pasek adresu
            await Task.Delay(200);

            System.Windows.Clipboard.SetText(folderPath);
            SendKeyCombo(VK_LCONTROL, VK_V);    // Ctrl+V – wklej ścieżkę
            await Task.Delay(100);

            SendKey(VK_RETURN);                 // Enter – nawiguj
            await Task.Delay(500);
        }

        private static List<IntPtr> GetExplorerHwnds()
        {
            var result = new List<IntPtr>();
            EnumWindows((hwnd, _) =>
            {
                if (IsWindowVisible(hwnd))
                {
                    var cls = new StringBuilder(256);
                    GetClassName(hwnd, cls, 256);
                    if (cls.ToString() == "CabinetWClass")
                        result.Add(hwnd);
                }
                return true;
            }, IntPtr.Zero);
            return result;
        }

        public static string GetFolderPath(string folder)
        {
            if (System.IO.Directory.Exists(folder))
                return folder;

            folder = System.IO.Path.GetDirectoryName(folder) ?? string.Empty;
            return System.IO.Directory.Exists(folder) ? folder : string.Empty;
        }

        // ── P/Invoke ─────────────────────────────────────────────────────────

        private const ushort VK_LCONTROL     = 0xA2;
        private const ushort VK_T            = 0x54;
        private const ushort VK_L            = 0x4C;
        private const ushort VK_V            = 0x56;
        private const ushort VK_RETURN       = 0x0D;
        private const uint   KEYEVENTF_KEYUP = 0x0002;
        private const uint   INPUT_KEYBOARD  = 1;

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint Type;
            public INPUTUNION Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)] public MOUSEINPUT Mouse;
            [FieldOffset(0)] public KEYBDINPUT Keyboard;
        }

        // MOUSEINPUT musi być w unii, żeby rozmiar był poprawny na x64
        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx, dy;
            public uint mouseData, dwFlags, time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk, wScan;
            public uint dwFlags, time;
            public IntPtr dwExtraInfo;
        }

        private static void SendKeyCombo(ushort modifier, ushort key)
        {
            var inputs = new[]
            {
                KeyInput(modifier, 0),
                KeyInput(key,      0),
                KeyInput(key,      KEYEVENTF_KEYUP),
                KeyInput(modifier, KEYEVENTF_KEYUP),
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        }

        private static void SendKey(ushort key)
        {
            var inputs = new[]
            {
                KeyInput(key, 0),
                KeyInput(key, KEYEVENTF_KEYUP),
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        }

        private static INPUT KeyInput(ushort vk, uint flags) => new INPUT
        {
            Type = INPUT_KEYBOARD,
            Data = new INPUTUNION { Keyboard = new KEYBDINPUT { wVk = vk, dwFlags = flags } }
        };

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    }
}
