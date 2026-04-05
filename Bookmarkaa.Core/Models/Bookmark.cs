using Bookmarkaa.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bookmarkaa.Models
{
    public class Bookmark
    {
        private string _iconPath = string.Empty;

        /// <summary>
        /// Name of the group
        /// </summary>
        public string Name { get; set; } = "New Bookmark";

        /// <summary>
        /// Folders list
        /// </summary>
        public List<string> Folders { get; set; } = new List<string>();

        [JsonIgnore]
        public string Folder
        {
            get => Folders.Count > 0 ? Folders[0] : string.Empty;
            set
            {
                Folders.Clear();
                Folders.Add(value);
            }
        }

        [JsonIgnore]
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Path to the icon
        /// </summary>
        public string IconPath {
            get
            {
                if (string.IsNullOrEmpty(_iconPath) || _iconPath.StartsWith("pack:"))
                {
                    return Constants.DefaultIconPath;
                }
                else {
                    return _iconPath;
                }
            }
            set
            {
                _iconPath = value;
            } 
        }

        [JsonIgnore]
        public ImageBrush IconBrush 
        { 
            get 
            {
                return new ImageBrush(new BitmapImage( new Uri(IconPath) ) );
            } 
        }

        /// <summary>
        /// Description of the group
        /// </summary>
        public string Description { 
            get 
            {
                if (Folders.Count == 0)
                {
                    return "No folders";
                }
                else if (Folders.Count == 1)
                {
                    return Folders[0];
                }
                else
                {
                    return Folders[0] + $" (+{Folders.Count - 1} more)";
                }
            }
        }

        public Bookmark()
        {
        }

        public Bookmark(string folder)
        {
            Name = System.IO.Path.GetFileName(folder);
            Folder = folder;
        }

    }
}
