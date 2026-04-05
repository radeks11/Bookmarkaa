using Bookmarkaa.Models;

namespace Bookmarkaa
{
    /// <summary>
    /// Interaction logic for BookmarkItem.xaml
    /// </summary>
    public partial class BookmarkItem : System.Windows.Controls.UserControl
    {
        public Bookmark DataItem { get; set; } = new();

        public BookmarkItem()
        {
            InitializeComponent();
        }
    }
}
