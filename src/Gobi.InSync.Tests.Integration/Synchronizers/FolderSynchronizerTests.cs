using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Gobi.InSync.App.Synchronizers;
using Xunit;

namespace Gobi.InSync.Tests.Integration.Synchronizers
{
    [Collection(nameof(FolderSynchronizer))]
    public class FolderSynchronizerTests
    {
        public FolderSynchronizerTests()
        {
            if (Directory.Exists(RootFolder)) Directory.Delete(RootFolder, true);

            Directory.CreateDirectory(SourceFolder);
            Directory.CreateDirectory(TargetFolder);
        }

        private static readonly string RootFolder = $"test/{nameof(FolderSynchronizerTests)}";
        private static readonly string SourceFolder = $"{RootFolder}/source";
        private static readonly string TargetFolder = $"{RootFolder}/target";

        private static void CreateFiles(IEnumerable<FileEntry> entries)
        {
            var fileEntries = entries as FileEntry[] ?? entries.ToArray();
            foreach (var folderEntry in fileEntries.Where(x => x.IsFolder))
                Directory.CreateDirectory(folderEntry.Path);
            foreach (var fileEntry in fileEntries.Where(x => !x.IsFolder))
            {
                using var file = File.Create(fileEntry.Path);
            }
        }

        private static List<FileEntry> EnumerateFiles(string path)
        {
            var directory = new DirectoryInfo(path);
            return directory.GetFileSystemInfos("*", SearchOption.AllDirectories)
                .Select(x => new FileEntry
                (
                    x.FullName,
                    (x.Attributes & FileAttributes.Directory) != 0
                ))
                .ToList();
        }

        private class FileEntry
        {
            public FileEntry(string path, bool isFolder)
            {
                Path = path;
                IsFolder = isFolder;
            }

            public string Path { get; }
            public bool IsFolder { get; }
        }

        private void AssertIsSync(IEnumerable<FileEntry> source, IEnumerable<FileEntry> target)
        {
            static string NormalizeSubPath(string rootPath, string path)
            {
                return Path.GetFullPath(path).Replace(Path.GetFullPath(rootPath), string.Empty);
            }

            var sourceFiles = source.Select(x => new FileEntry(NormalizeSubPath(SourceFolder, x.Path), x.IsFolder));
            var targetFiles = target.Select(x => new FileEntry(NormalizeSubPath(TargetFolder, x.Path), x.IsFolder));

            sourceFiles.Should().BeEquivalentTo(targetFiles);
        }

        [Fact]
        public void SyncFolder_ChangedFile_Updated()
        {
            // arrange
            var sourceFiles = new[]
            {
                new FileEntry($"{SourceFolder}/level1", true),
                new FileEntry($"{SourceFolder}/level1/file", false)
            };

            var targetFiles = new[]
            {
                new FileEntry($"{TargetFolder}/level1", true),
                new FileEntry($"{TargetFolder}/level1/file", false)
            };

            CreateFiles(sourceFiles);
            CreateFiles(targetFiles);

            File.WriteAllText($"{SourceFolder}/level1/file", "new_content");
            File.WriteAllText($"{TargetFolder}/level1/file", "old_content");

            var sync = new FolderSynchronizer();

            // act
            sync.SyncFolder(SourceFolder, TargetFolder);

            // assert
            File.ReadAllText($"{TargetFolder}/level1/file").Should().Be("new_content");
        }

        [Fact]
        public void SyncFolder_NewFile_Created()
        {
            // arrange
            var sourceFiles = new[]
            {
                new FileEntry($"{SourceFolder}/level1", true),
                new FileEntry($"{SourceFolder}/level1/file1", false)
            };

            CreateFiles(sourceFiles);
            var sync = new FolderSynchronizer();

            // act
            sync.SyncFolder(SourceFolder, TargetFolder);

            // assert
            var foundFiles = EnumerateFiles(TargetFolder);
            AssertIsSync(sourceFiles, foundFiles);
        }

        [Fact]
        public void SyncFolder_NewFolder_Created()
        {
            // arrange
            var sourceFiles = new[]
            {
                new FileEntry($"{SourceFolder}/level1", true),
                new FileEntry($"{SourceFolder}/level1/level2_1", true),
                new FileEntry($"{SourceFolder}/level1/level2_2", true),
                new FileEntry($"{SourceFolder}/level1/level2_1/level3", true)
            };

            CreateFiles(sourceFiles);
            var sync = new FolderSynchronizer();

            // act
            sync.SyncFolder(SourceFolder, TargetFolder);

            // assert
            var targetFiles = EnumerateFiles(TargetFolder);
            AssertIsSync(sourceFiles, targetFiles);
        }

        [Fact]
        public void SyncFolder_ObsoleteFile_Removed()
        {
            // arrange
            var sourceFiles = new[]
            {
                new FileEntry($"{SourceFolder}/level1", true)
            };

            var targetFiles = new[]
            {
                new FileEntry($"{TargetFolder}/level1", true),
                new FileEntry($"{TargetFolder}/level1/obsolete", false)
            };

            CreateFiles(sourceFiles);
            CreateFiles(targetFiles);
            var sync = new FolderSynchronizer();

            // act
            sync.SyncFolder(SourceFolder, TargetFolder);

            // assert
            var foundFiles = EnumerateFiles(TargetFolder);
            AssertIsSync(sourceFiles, foundFiles);
        }

        [Fact]
        public void SyncFolder_RemovedFolder_Deleted()
        {
            // arrange
            var sourceFiles = new[]
            {
                new FileEntry($"{SourceFolder}/level1", true),
                new FileEntry($"{SourceFolder}/level1/level2_1", true),
                new FileEntry($"{SourceFolder}/level1/level2_1/level3", true)
            };

            var targetFiles = new[]
            {
                new FileEntry($"{TargetFolder}/level1", true),
                new FileEntry($"{TargetFolder}/level1/level2_1", true),
                new FileEntry($"{TargetFolder}/level1/removed", true),
                new FileEntry($"{TargetFolder}/level1/level2_1/level3", true)
            };

            CreateFiles(sourceFiles);
            CreateFiles(targetFiles);
            var sync = new FolderSynchronizer();

            // act
            sync.SyncFolder(SourceFolder, TargetFolder);

            // assert
            var foundFiles = EnumerateFiles(TargetFolder);
            AssertIsSync(sourceFiles, foundFiles);
        }

        [Fact]
        public void SyncFolder_TargetIsFile_FolderCreated()
        {
            // arrange
            var sourceFiles = new[]
            {
                new FileEntry($"{SourceFolder}/level1", true)
            };

            var targetFiles = new[]
            {
                new FileEntry($"{TargetFolder}/level1", false)
            };

            CreateFiles(sourceFiles);
            CreateFiles(targetFiles);
            var sync = new FolderSynchronizer();

            // act
            sync.SyncFolder(SourceFolder, TargetFolder);

            // assert
            var foundFiles = EnumerateFiles(TargetFolder);
            AssertIsSync(sourceFiles, foundFiles);
        }
    }
}