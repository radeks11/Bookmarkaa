using Bookmarkaa.Helpers;
using Bookmarkaa.Managers;
using Bookmarkaa.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bookmarkaa
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Models.Bookmark> Items { get; } = new();

        private ICollectionView _collectionView = null!;
        private bool _dirty = false;

        public bool Dirty {  
            get 
            {
                return _dirty;
            }
            set
            {
                _dirty = value;
                StatusSave.Foreground = _dirty ? Brushes.Red : Brushes.White;
            }
        } 

        public MainWindow()
        {
            InitializeComponent();
            Items = new ObservableCollection<Bookmark>(SettingsManager.Settings.Bookmarks);
            DataContext = this;
            BookmarksList.SelectionChanged += BookmarksList_SelectionChanged;
            Items.CollectionChanged += Items_CollectionChanged;

            _collectionView = CollectionViewSource.GetDefaultView(Items);
            _collectionView.Filter = FilterBookmark;

            Loaded += (_, _) => SearchBox.Focus();
        }

        private bool FilterBookmark(object obj)
        {
            if (obj is not Bookmark bookmark)
                return false;
            string filter = SearchBox.Text;
            if (string.IsNullOrWhiteSpace(filter))
                return true;
            return bookmark.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _collectionView.Refresh();
            if (BookmarksList.SelectedIndex < 0 && BookmarksList.Items.Count > 0)
                BookmarksList.SelectedIndex = 0;
        }

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            int count = BookmarksList.Items.Count;
            if (count == 0)
                return;

            if (e.Key == Key.Down)
            {
                BookmarksList.SelectedIndex = Math.Min(BookmarksList.SelectedIndex + 1, count - 1);
                BookmarksList.ScrollIntoView(BookmarksList.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                BookmarksList.SelectedIndex = Math.Max(BookmarksList.SelectedIndex - 1, 0);
                BookmarksList.ScrollIntoView(BookmarksList.SelectedItem);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && BookmarksList.SelectedItem is Bookmark selected)
            {
                bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                ExecuteBookmarkAction(selected, shift);
                e.Handled = true;
            }
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Dirty = true;
        }

        public void DeleteBookmark(Bookmark bookmark)
        {
            Items.Remove(bookmark); // Items_CollectionChanged ustawi Dirty = true
        }

        private void BookmarksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Zaznaczenie śledzone tylko przez UI — akcja wywoływana z kliknięcia lub Enter
        }

        private void BookmarksList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (BookmarksList.IsSortingEnabled)
                return;

            // Sprawdź czy kliknięto na element listy (nie na puste miejsce)
            DependencyObject? element = e.OriginalSource as DependencyObject;
            while (element != null && element is not ListViewItem)
                element = VisualTreeHelper.GetParent(element);
            if (element == null)
                return;

            if (BookmarksList.SelectedItem is not Bookmark item)
                return;

            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            ExecuteBookmarkAction(item, shift);
        }

        private void ExecuteBookmarkAction(Bookmark item, bool edit)
        {
            if (edit)
            {
                EditBookmark editBookmark = new EditBookmark(item);
                editBookmark.ShowDialog();
                // Brak przycisku Anuluj – każde zamknięcie okna traktujemy jako zatwierdzenie
                Dirty = true;
                SettingsManager.Settings.Bookmarks = new List<Bookmark>(Items);
                BookmarksList.Items.Refresh();
                BookmarksList.SelectedItem = null;
            }
            else
            {
                new Helpers.FileExplorerProcess(item.Folders);
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NativeMethods.POINT cursorPos;
            NativeMethods.GetCursorPos(out cursorPos);

            IntPtr monitor = NativeMethods.MonitorFromPoint(cursorPos, NativeMethods.MONITOR_DEFAULTTONEAREST);

            var info = new NativeMethods.MONITORINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            NativeMethods.GetMonitorInfo(monitor, ref info);

            var area = info.rcWork;
            double dpi = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;

            Left = area.left / dpi + (area.Width / dpi - ActualWidth) / 2;
            Top = area.top / dpi + (area.Height / dpi - ActualHeight) / 2;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        protected override void OnKeyDown(KeyEventArgs args)
        {
            base.OnKeyDown(args);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                switch (args.Key)
                {
                    case Key.S:
                        {
                            Save();
                            break;
                        }
                    case Key.Q:
                        {
                            BookmarksList.IsSortingEnabled = !BookmarksList.IsSortingEnabled;
                            BookmarksList.AllowDrop = BookmarksList.IsSortingEnabled;
                            BookmarksList.SelectedIndex = -1;
                            break;
                        }
                    case Key.V:
                        {
                            CreateBookmarkFromClipboard();
                            break;
                        }
                    
                }

                StatusSort.Foreground = BookmarksList.IsSortingEnabled ? Brushes.Lime : Brushes.White;
            }
        }

        protected void Save()
        {
            SettingsManager.Settings.Bookmarks = new List<Bookmark>(Items);
            SettingsManager.SaveSettings();
            Dirty = false;
        }

        private void StatusSort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BookmarksList.IsSortingEnabled = !BookmarksList.IsSortingEnabled;
            BookmarksList.AllowDrop = BookmarksList.IsSortingEnabled;
            StatusSort.Foreground = BookmarksList.IsSortingEnabled ? Brushes.Lime : Brushes.White;
        }

        private void StatusSave_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Save();
        }

        private void CreateBookmarkFromClipboard()
        {
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                string[] lines = clipboardText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                    return;
                
                string folder = FileExplorerProcess.GetFolderPath(lines[0]);
                if (folder != string.Empty)
                {
                    Bookmark newBookmark = new Bookmark(folder);
                    Items.Add(newBookmark);
                    Dirty = true;
                    return;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Dirty)
            {
                MessageBoxResult result = MessageBox.Show("You have unsaved changes. Do you want to save before exiting?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    Save();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
            
        }
    }
}