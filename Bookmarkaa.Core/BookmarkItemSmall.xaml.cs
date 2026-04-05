using Bookmarkaa.Helpers;
using Bookmarkaa.Models;

namespace Bookmarkaa
{
    /// <summary>
    /// Interaction logic for BookmarkItem.xaml
    /// </summary>
    public partial class BookmarkItemSmall : System.Windows.Controls.UserControl
    {
        public Bookmark DataItem { get; set; } = new();

        public BookmarkItemSmall()
        {
            InitializeComponent();
        }
    }
}
