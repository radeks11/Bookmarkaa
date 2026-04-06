namespace Bookmarkaa.Models
{
    public class AppSettings
    {
        public string DefaultIconsFolder { get; set; } = string.Empty;
        public List<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

        // Tray settings
        public bool RunAtStartup { get; set; } = false;

        /// <summary>MOD_ALT=1, MOD_CTRL=2, MOD_SHIFT=4, MOD_WIN=8</summary>
        public int HotKeyModifiers { get; set; } = 0x000C; // MOD_SHIFT | MOD_WIN

        /// <summary>Virtual key code (System.Windows.Forms.Keys)</summary>
        public int HotKeyCode { get; set; } = 0x42; // Keys.B

        // ── Duplikowanie aktywnej zakładki Eksploratora ──────────────────────

        public bool DuplicateTabEnabled { get; set; } = false;

        /// <summary>MOD_ALT=1, MOD_CTRL=2, MOD_SHIFT=4, MOD_WIN=8</summary>
        public int DuplicateTabHotKeyModifiers { get; set; } = 0x0002; // MOD_CTRL

        /// <summary>Virtual key code (System.Windows.Forms.Keys)</summary>
        public int DuplicateTabHotKeyCode { get; set; } = 0x44; // Keys.D
    }
}
