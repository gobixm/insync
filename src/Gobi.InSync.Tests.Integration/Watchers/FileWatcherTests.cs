using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Gobi.InSync.App.Watchers;
using Gobi.InSync.App.Watchers.Models;
using Xunit;

namespace Gobi.InSync.Tests.Integration.Watchers
{
    [Collection(nameof(FileWatcher))]
    public class FileWatcherTests
    {
        public FileWatcherTests()
        {
            if (Directory.Exists(RootFolder)) Directory.Delete(RootFolder, true);

            Directory.CreateDirectory(TestFolder);
        }

        private static readonly string RootFolder = $"test/{nameof(FileWatcherTests)}";
        private static readonly string TestFolder = $"{RootFolder}/test_folder";

        private async Task<T> SubscribeFirstAsync<T, TEvent>(IObservable<T> observable)
        {
            var millisecondsDelay = 10000;
            using var cts = new CancellationTokenSource(millisecondsDelay);
            var tcs = new TaskCompletionSource<T>();
            observable
                .FirstAsync(x => x is TEvent)
                .Subscribe(
                    x => tcs.SetResult(x),
                    cts.Token);

            await Task.WhenAny(Task.Delay(millisecondsDelay, cts.Token), tcs.Task);
            if (!tcs.Task.IsCompleted)
            {
                tcs.SetCanceled();
            }
            return await tcs.Task;
        }


        private bool IsPathEqual(string a, string b)
        {
            return Path.GetFullPath(a) == Path.GetFullPath(b);
        }

        [Fact]
        public async Task Start_Directory_CreateCaptured()
        {
            // arrange
            var dirName = "dir";
            var dirPath = $"{TestFolder}/{dirName}";
            using var watcher = new FileWatcher($"{TestFolder}");

            var firstEventTask = SubscribeFirstAsync<IFileEvent, FileCreated>(watcher.FileObservable());

            // act
            watcher.Start();
            Directory.CreateDirectory(dirPath);

            // assert
            var ev = await firstEventTask;
            ev.Should().BeOfType<FileCreated>();
            ev.As<FileCreated>().FileName.Should().Be(dirName);
            IsPathEqual(ev.As<FileCreated>().Path, dirPath).Should().BeTrue();
        }

        [Fact]
        public async Task Start_Directory_DeleteCaptured()
        {
            // arrange
            var dirName = "dir";
            var dirPath = $"{TestFolder}/{dirName}";
            using var watcher = new FileWatcher($"{TestFolder}");
            Directory.CreateDirectory(dirPath);

            var firstEventTask = SubscribeFirstAsync<IFileEvent, FileDeleted>(watcher.FileObservable());

            // act
            watcher.Start();
            Directory.Delete(dirPath);

            // assert
            var ev = await firstEventTask;

            ev.Should().BeOfType<FileDeleted>();
            ev.As<FileDeleted>().FileName.Should().Be(dirName);
            IsPathEqual(ev.As<FileDeleted>().Path, dirPath).Should().BeTrue();
        }

        [Fact]
        public async Task Start_Directory_RenameCaptured()
        {
            // arrange
            var dirName = "dir";
            var newName = "dir_new";
            var dirPath = $"{TestFolder}/{dirName}";
            var newPath = $"{TestFolder}/{newName}";
            using var watcher = new FileWatcher($"{TestFolder}");
            Directory.CreateDirectory(dirPath);

            var firstEventTask = SubscribeFirstAsync<IFileEvent, FileRenamed>(watcher.FileObservable());

            // act
            watcher.Start();
            Directory.Move(dirPath, newPath);

            // assert
            var ev = await firstEventTask;
            ev.Should().BeOfType<FileRenamed>();
            ev.As<FileRenamed>().FileName.Should().Be(newName);
            ev.As<FileRenamed>().OldFileName.Should().Be(dirName);
            IsPathEqual(ev.As<FileRenamed>().Path, newPath).Should().BeTrue();
            IsPathEqual(ev.As<FileRenamed>().OldPath, dirPath).Should().BeTrue();
        }

        [Fact]
        public async Task Start_SingleFile_ChangeCaptured()
        {
            // arrange
            var fileName = "file.test";
            var filePath = $"{TestFolder}/{fileName}";
            using var watcher = new FileWatcher($"{TestFolder}");
            await using var file = File.Create(filePath);
            await file.DisposeAsync();

            var firstEventTask = SubscribeFirstAsync<IFileEvent, FileChanged>(watcher.FileObservable());

            // act
            watcher.Start();
            await File.WriteAllTextAsync(filePath, "test");

            // assert
            var ev = await firstEventTask;
            ev.Should().BeOfType<FileChanged>();
            ev.As<FileChanged>().FileName.Should().Be(fileName);
            IsPathEqual(ev.As<FileChanged>().Path, filePath).Should().BeTrue();
        }

        [Fact]
        public async Task Start_SingleFile_CreateCaptured()
        {
            // arrange
            var fileName = "file.test";
            var filePath = $"{TestFolder}/{fileName}";
            using var watcher = new FileWatcher($"{TestFolder}");

            var firstEventTask = SubscribeFirstAsync<IFileEvent, FileCreated>(watcher.FileObservable());

            // act
            watcher.Start();
            await using var file = File.Create(filePath);

            // assert
            var ev = await firstEventTask;
            ev.Should().BeOfType<FileCreated>();
            ev.As<FileCreated>().FileName.Should().Be(fileName);
            IsPathEqual(ev.As<FileCreated>().Path, filePath).Should().BeTrue();
        }

        [Fact]
        public async Task Start_SingleFile_DeleteCaptured()
        {
            // arrange
            var fileName = "file.test";
            var filePath = $"{TestFolder}/{fileName}";
            using var watcher = new FileWatcher($"{TestFolder}");
            await using var file = File.Create(filePath);
            await file.DisposeAsync();

            var firstEventTask = SubscribeFirstAsync<IFileEvent, FileDeleted>(watcher.FileObservable());

            // act
            watcher.Start();
            File.Delete(filePath);

            // assert
            var ev = await firstEventTask;
            ev.Should().BeOfType<FileDeleted>();
            ev.As<FileDeleted>().FileName.Should().Be(fileName);
            IsPathEqual(ev.As<FileDeleted>().Path, filePath).Should().BeTrue();
        }

        [Fact]
        public async Task Start_SingleFile_RenameCaptured()
        {
            // arrange
            var fileName = "file.test";
            var newName = "file_new.test";
            var filePath = $"{TestFolder}/{fileName}";
            var newPath = $"{TestFolder}/{newName}";
            using var watcher = new FileWatcher($"{TestFolder}");
            await using var file = File.Create(filePath);
            await file.DisposeAsync();

            var firstEventTask = SubscribeFirstAsync<IFileEvent, FileRenamed>(watcher.FileObservable());

            // act
            watcher.Start();
            File.Move(filePath, newPath);

            // assert
            var ev = await firstEventTask;
            ev.Should().BeOfType<FileRenamed>();
            ev.As<FileRenamed>().FileName.Should().Be(newName);
            ev.As<FileRenamed>().OldFileName.Should().Be(fileName);
            IsPathEqual(ev.As<FileRenamed>().Path, newPath).Should().BeTrue();
            IsPathEqual(ev.As<FileRenamed>().OldPath, filePath).Should().BeTrue();
        }
    }
}