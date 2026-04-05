using Bookmarkaa.Helpers;
using Bookmarkaa.Models;
using System.Windows;
using System.Windows.Input;

namespace Bookmarkaa
{
    public partial class BookmarkItemSmall : System.Windows.Controls.UserControl
    {
        public BookmarkItemSmall()
        {
            InitializeComponent();

            Loaded   += (_, _) => InputManager.Current.PreProcessInput += OnPreProcessInput;
            Unloaded += (_, _) => InputManager.Current.PreProcessInput -= OnPreProcessInput;

            MouseEnter += (_, _) => UpdateDeleteButton();
            MouseLeave += (_, _) => UpdateDeleteButton();
        }

        // Wywoływane dla każdego zdarzenia wejścia w aplikacji – filtrujemy tylko klawisze
        private void OnPreProcessInput(object sender, PreProcessInputEventArgs e)
        {
            if (e.StagingItem.Input is KeyEventArgs)
                UpdateDeleteButton();
        }

        private void UpdateDeleteButton()
        {
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            BtnDelete.Visibility = IsMouseOver && shift
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Bookmark bookmark &&
                Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.DeleteBookmark(bookmark);
            }
        }
    }
}
