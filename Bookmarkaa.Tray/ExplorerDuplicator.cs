using Bookmarkaa.Helpers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Bookmarkaa.Tray
{
    internal static class ExplorerDuplicator
    {
        /// <summary>
        /// Duplikuje aktywną zakładkę Eksploratora:
        /// na Windows 11 22H2+ otwiera nową zakładkę w tym samym oknie,
        /// na starszych systemach otwiera nowe okno.
        /// </summary>
        public static void DuplicateActiveTab()
        {
            IntPtr fgHwnd = GetForegroundWindow();
            if (fgHwnd == IntPtr.Zero) return;

            var cls = new StringBuilder(256);
            GetClassName(fgHwnd, cls, 256);
            if (cls.ToString() != "CabinetWClass") return;

            string? path = GetExplorerPath(fgHwnd);
            if (string.IsNullOrEmpty(path)) return;

            if (FileExplorerProcess.ExplorerTabsSupported)
                _ = FileExplorerProcess.OpenInNewTabAsync(fgHwnd, path);
            else
                Process.Start("explorer.exe", $"\"{path}\"");
        }

        private static string? GetExplorerPath(IntPtr targetHwnd)
        {
            try
            {
                dynamic shell = Activator.CreateInstance(
                    Type.GetTypeFromProgID("Shell.Application")!)!;

                // Próba 1: bezpośrednie dopasowanie HWND (Win 10 / Win 11 bez zakładek)
                foreach (dynamic window in shell.Windows())
                {
                    try
                    {
                        if (new IntPtr((int)window.HWND) == targetHwnd)
                            return window.Document.Folder.Self.Path;
                    }
                    catch { }
                }

                // Próba 2: zakładki Win 11 – HWND zakładki jest dzieckiem głównego okna
                foreach (dynamic window in shell.Windows())
                {
                    try
                    {
                        IntPtr root = GetAncestor(new IntPtr((int)window.HWND), GA_ROOTOWNER);
                        if (root == targetHwnd)
                            return window.Document.Folder.Self.Path;
                    }
                    catch { }
                }
            }
            catch { }

            return null;
        }

        private const uint GA_ROOTOWNER = 3;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);
    }
}
