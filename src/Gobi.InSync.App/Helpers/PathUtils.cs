using System.IO;

namespace Gobi.InSync.App.Helpers
{
    public static class PathUtils
    {
        public static bool IsSubPath(string leftPath, string rightPath)
        {
            var left = new DirectoryInfo(leftPath);
            var right = new DirectoryInfo(rightPath);
            var isParent = false;
            while (right.Parent != null)
                if (right.Parent.FullName == left.FullName)
                    return true;
                else
                    right = right.Parent;

            return false;
        }

        public static bool IsPathExists(string path)
        {
            return Directory.Exists(path) || File.Exists(path);
        }
    }
}