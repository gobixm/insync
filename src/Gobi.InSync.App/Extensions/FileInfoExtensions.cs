using System.IO;

namespace Gobi.InSync.App.Extensions
{
    public static class FileInfoExtensions
    {
        public static bool IsFolder(this FileInfo fileInfo)
        {
            return (fileInfo.Attributes & FileAttributes.Directory) != 0;
        }
    }
}