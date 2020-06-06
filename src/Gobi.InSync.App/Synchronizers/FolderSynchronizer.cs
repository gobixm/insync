using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Murmur;

namespace Gobi.InSync.App.Synchronizers
{
    public class FolderSynchronizer
    {
        private static EnumerationOptions _searchOptions;

        public async Task SyncFolder(string sourcePath, string targetPath)
        {
            var contexts = new Stack<SyncContext>();
            contexts.Push(new SyncContext
            {
                SourcePath = sourcePath,
                TargetPath = targetPath
            });
            Sync(contexts);
        }

        private void Sync(Stack<SyncContext> syncContexts)
        {
            do
            {
                var currentContext = syncContexts.Pop();
                _searchOptions = new EnumerationOptions
                {
                    IgnoreInaccessible = false,
                    MatchCasing = MatchCasing.PlatformDefault,
                    RecurseSubdirectories = false,
                    ReturnSpecialDirectories = false
                };
                SyncFolder(currentContext.SourcePath, currentContext.TargetPath, syncContexts);
            } while (syncContexts.Count != 0);
        }

        private static void SyncFolder(string sourcePath, string targetPath, Stack<SyncContext> syncContexts)
        {
            var sourceDirectory = new DirectoryInfo(sourcePath);
            var targetDirectory = new DirectoryInfo(targetPath);

            if (!targetDirectory.Exists) targetDirectory.Create();

            if ((File.GetAttributes(targetPath) & FileAttributes.Directory) == 0)
            {
                File.Delete(targetPath);
                targetDirectory.Create();
            }

            SyncFiles(sourceDirectory, targetDirectory);
            SyncSubFolders(sourceDirectory, targetDirectory, syncContexts);
        }

        private static void SyncFiles(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory)
        {
            var sourceFiles = sourceDirectory.GetFiles();
            var targetFiles = targetDirectory.GetFiles();

            var sourceFileNames = sourceFiles.Select(x => x.Name)
                .ToHashSet();
            var targetFileNames = targetFiles.Select(x => x.Name)
                .ToHashSet();

            foreach (var sourceFileName in sourceFileNames)
            {
                var sourceFilePath = Path.Combine(sourceDirectory.FullName, sourceFileName);
                var targetFilePath = Path.Combine(targetDirectory.FullName, sourceFileName);
                if (!targetFileNames.Contains(sourceFileName)
                    || !CalculateHash(sourceFilePath).SequenceEqual(CalculateHash(targetFilePath))
                )
                    File.Copy(
                        sourceFilePath,
                        targetFilePath,
                        true
                    );
            }

            targetFileNames.ExceptWith(sourceFileNames);
            foreach (var targetFileName in targetFileNames)
                File.Delete(Path.Combine(targetDirectory.FullName, targetFileName));
        }

        private static void SyncSubFolders(DirectoryInfo sourceDirectory, DirectoryInfo targetDirectory,
            Stack<SyncContext> syncContexts)
        {
            var sourceDirectories = sourceDirectory.GetDirectories();
            var targetDirectories = targetDirectory.GetDirectories();

            var sourceDirectoryNames = sourceDirectories.Select(x => x.Name)
                .ToHashSet();
            var targetDirectoryNames = targetDirectories.Select(x => x.Name)
                .ToHashSet();

            foreach (var sourceDirectoryName in sourceDirectoryNames)
            {
                var sourceDirectoryPath = Path.Combine(sourceDirectory.FullName, sourceDirectoryName);
                var targetDirectoryPath = Path.Combine(targetDirectory.FullName, sourceDirectoryName);
                if (!targetDirectoryNames.Contains(sourceDirectoryName))
                {
                    targetDirectory.Create();
                    syncContexts.Push(new SyncContext
                    {
                        SourcePath = sourceDirectoryPath,
                        TargetPath = targetDirectoryPath
                    });
                    continue;
                }

                if ((File.GetAttributes(targetDirectoryPath) & FileAttributes.Directory) == 0)
                {
                    File.Delete(targetDirectoryPath);
                    targetDirectory.Create();
                }

                syncContexts.Push(new SyncContext
                {
                    SourcePath = sourceDirectoryPath,
                    TargetPath = targetDirectoryPath
                });
            }

            targetDirectoryNames.ExceptWith(sourceDirectoryNames);
            foreach (var targetDirectoryName in targetDirectoryNames)
                Directory.Delete(Path.Combine(targetDirectory.FullName, targetDirectoryName), true);
        }

        private static byte[] CalculateHash(string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open);
            return MurmurHash.Create128().ComputeHash(fileStream);
        }

        private class SyncContext
        {
            public string SourcePath { get; set; }
            public string TargetPath { get; set; }
        }
    }
}