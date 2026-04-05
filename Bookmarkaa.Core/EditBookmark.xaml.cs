using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Bookmarkaa.Helpers;
using Bookmarkaa.Managers;
using Bookmarkaa.Models;

namespace Bookmarkaa
{
    /// <summary>
    /// Interaction logic for EditBookmark.xaml
    /// </summary>
    public partial class EditBookmark : Window
    {
        Bookmark _item = new Bookmark();

        protected string IconPath { 
            get
            {
                ImageBrush brush = (ImageBrush)Icon.Fill;
                Uri uri = new Uri(brush.ImageSource.ToString());
                if (uri.AbsoluteUri.StartsWith("pack:"))
                {
                    return string.Empty;
                }
                else
                {
                    return uri.LocalPath;
                }
            } 
        }

        public EditBookmark(Bookmark item)
        {
            _item = item;
            InitializeComponent();

            Name.Text = _item.Name;
            Folder.Text = _item.Folder;
            Icon.Fill = _item.IconBrush;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _item.Name = Name.Text;
            _item.Folder = Folder.Text;
            _item.IconPath = IconPath;            
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            _item.IsDeleted = true;
            this.DialogResult = true;
        }

        private void Icon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = Constants.IconFileFilter;
            openFileDialog.Multiselect = false;
            openFileDialog.DefaultDirectory = SettingsManager.Settings.DefaultIconsFolder;
            openFileDialog.InitialDirectory = IconPath == string.Empty ? openFileDialog.DefaultDirectory : FileExplorerProcess.GetFolderPath(IconPath);
            bool? status = openFileDialog.ShowDialog();
            if (status != null && status == true)
            {
                // _item.IconPath = openFileDialog.FileName;
                Icon.Fill = new ImageBrush(new BitmapImage(new Uri(openFileDialog.FileName)));
            }
        }

        private void SelectFolder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.FileName = "Wybierz folder";
            openFileDialog.Multiselect = false;
            openFileDialog.CheckFileExists = false;
            // openFileDialog.DefaultDirectory = SettingsManager.Settings.DefaultIconsFolder;
            openFileDialog.InitialDirectory = Folder.Text;
            bool? status = openFileDialog.ShowDialog();
            if (status != null && status == true)
            {
                Folder.Text = FileExplorerProcess.GetFolderPath(openFileDialog.FileName);
                // Folder.Text = Folder.Text;
            }
        }
    }
}
