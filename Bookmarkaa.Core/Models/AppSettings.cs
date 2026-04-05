using Bookmarkaa.Helpers;
using System.Text.Json.Serialization;

namespace Bookmarkaa.Models
{
    public class AppSettings
    {
        public string DefaultIconsFolder { get; set; } = @"C:\Storage\Synology_Drive_RASoft\home\Tools\icons\special";
        public List<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

        // Tray settings
        public bool RunAtStartup { get; set; } = false;

        /// <summary>MOD_ALT=1, MOD_CTRL=2, MOD_SHIFT=4, MOD_WIN=8</summary>
        public int HotKeyModifiers { get; set; } = 0x000C; // MOD_SHIFT | MOD_WIN

        /// <summary>Virtual key code (System.Windows.Forms.Keys)</summary>
        public int HotKeyCode { get; set; } = 0x42; // Keys.B
    }
}
