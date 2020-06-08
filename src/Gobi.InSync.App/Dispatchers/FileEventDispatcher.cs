using System.IO;
using Gobi.InSync.App.Extensions;
using Gobi.InSync.App.Watchers.Models;

namespace Gobi.InSync.App.Dispatchers
{
    public sealed class FileEventDispatcher : IFileEventDispatcher
    {
        public void Dispatch(string sourceFolder, string targetFolder, IFileEvent fileEvent)
        {
            var targetPath = Path.Combine(targetFolder, Path.GetRelativePath(sourceFolder, fileEvent.Path));
            switch (fileEvent)
            {
                case FileChanged _:
                case FileCreated _:
                    ReplaceFile(fileEvent.Path, targetPath);
                    break;
                case FileDeleted _:
                    RemoveFile(targetPath);
                    break;
                case FileRenamed renamed:
                    var targetOldPath = Path.Combine(targetFolder,
                        Path.GetRelativePath(sourceFolder, renamed.OldFileName));
                    RenameFile(targetOldPath, targetPath);
                    break;
            }
        }

        private void ReplaceFile(string sourcePath, string targetPath)
        {
            var sourceInfo = new FileInfo(sourcePath);
            var targetInfo = new FileInfo(targetPath);

            if (!sourceInfo.Exists) return;

            if (targetInfo.Exists && targetInfo.IsFolder() && !sourceInfo.IsFolder())
                Directory.Delete(targetInfo.FullName, true);

            if (!targetInfo.Exists && sourceInfo.IsFolder()) Directory.CreateDirectory(targetInfo.FullName);

            if (!sourceInfo.IsFolder()) File.Copy(sourceInfo.FullName, targetInfo.FullName, true);
        }

        private void RemoveFile(string targetPath)
        {
            var targetInfo = new FileInfo(targetPath);
            if (!targetInfo.Exists) return;

            if (targetInfo.Exists && targetInfo.IsFolder()) Directory.Delete(targetInfo.FullName, true);

            if (targetInfo.Exists && !targetInfo.IsFolder()) File.Delete(targetInfo.FullName);
        }

        private void RenameFile(string oldPath, string newPath)
        {
            var oldTargetInfo = new FileInfo(oldPath);
            var targetInfo = new FileInfo(newPath);
            if (!oldTargetInfo.Exists) return;

            RemoveFile(oldTargetInfo.FullName);

            if (oldTargetInfo.IsFolder()) Directory.Move(oldTargetInfo.FullName, targetInfo.FullName);
        }
    }
}