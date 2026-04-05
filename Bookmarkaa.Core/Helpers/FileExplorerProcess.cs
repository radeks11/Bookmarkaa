using Bookmarkaa.Models;
using System.Diagnostics;

namespace Bookmarkaa.Helpers
{
    public class FileExplorerProcess
    {
        Process _process = new Process();

        public FileExplorerProcess(List<string> folders)
        {
            if (folders == null || folders.Count == 0)
                return;

            // Uruchamiamy tylko pierwszy folder
            _process = Process.Start("explorer.exe", folders[0]);

        }
        public static string GetFolderPath(string folder)
        {
            if (System.IO.Directory.Exists(folder))
            {
                return folder;
            }
            else 
            {
                folder = System.IO.Path.GetDirectoryName(folder) ?? string.Empty;
                if (System.IO.Directory.Exists(folder))
                {
                    return folder;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

    }

}
