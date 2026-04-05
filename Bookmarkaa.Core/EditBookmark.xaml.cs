using Bookmarkaa.Helpers;
using Bookmarkaa.Managers;
using Bookmarkaa.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bookmarkaa
{
    public partial class EditBookmark : Window
    {
        private readonly Bookmark _item;
        private readonly ObservableCollection<string> _folders = new();

        protected string IconPath
        {
            get
            {
                var brush = (ImageBrush)Icon.Fill;
                if (brush.ImageSource == null)
                    return string.Empty;
                var uri = new Uri(brush.ImageSource.ToString()!);
                return uri.AbsoluteUri.StartsWith("pack:") ? string.Empty : uri.LocalPath;
            }
        }

        public EditBookmark(Bookmark item)
        {
            _item = item;
            InitializeComponent();

            NameBox.Text = _item.Name;
            Icon.Fill = _item.IconBrush;

            foreach (var folder in _item.Folders)
                _folders.Add(folder);

            FoldersList.ItemsSource = _folders;

            // Synchronizacja na żywo: foldery → _item
            _folders.CollectionChanged += (_, _) =>
                _item.Folders = new List<string>(_folders);

            // Synchronizacja na żywo: nazwa → _item
            NameBox.TextChanged += (_, _) =>
                _item.Name = NameBox.Text;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Dodaj ewentualnie wpisany, niezatwierdzony folder
            TryAddNewFolderFromBox();

            // Zapewnij że DialogResult jest ustawiony (zamknięcie przez X = akceptacja)
            DialogResult ??= true;
        }

        private void BtnAddFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Wybierz folder",
                InitialDirectory = GetInitialDirectory()
            };

            if (dialog.ShowDialog() == true)
            {
                AddFolder(dialog.FolderName);
                NewFolderBox.Clear();
            }
        }

        private void NewFolderBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryAddNewFolderFromBox();
                e.Handled = true;
            }
        }

        private void RemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string folder)
                _folders.Remove(folder);
        }

        private void FoldersList_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void FoldersList_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var path in paths)
            {
                var folder = System.IO.Directory.Exists(path)
                    ? path
                    : System.IO.Path.GetDirectoryName(path) ?? string.Empty;

                AddFolder(folder);
            }
        }

        private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = Constants.IconFileFilter,
                Multiselect = false,
                DefaultDirectory = SettingsManager.Settings.DefaultIconsFolder,
                InitialDirectory = IconPath == string.Empty
                    ? SettingsManager.Settings.DefaultIconsFolder
                    : FileExplorerProcess.GetFolderPath(IconPath)
            };

            if (dialog.ShowDialog() == true)
            {
                Icon.Fill = new ImageBrush(new BitmapImage(new Uri(dialog.FileName)));
                _item.IconPath = dialog.FileName;
            }
        }

        private void AddFolder(string path)
        {
            if (!string.IsNullOrEmpty(path) && !_folders.Contains(path))
                _folders.Add(path);
        }

        private void TryAddNewFolderFromBox()
        {
            var path = FileExplorerProcess.GetFolderPath(NewFolderBox.Text.Trim());
            AddFolder(path);
            if (!string.IsNullOrEmpty(path))
                NewFolderBox.Clear();
        }

        private string GetInitialDirectory()
        {
            if (FoldersList.SelectedItem is string selected)
                return selected;
            if (_folders.Count > 0)
                return _folders[^1];
            return string.Empty;
        }
    }
}
