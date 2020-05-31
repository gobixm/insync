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
    [Collection("Sequential")]
    public class FileWatcherTests
    {
        public FileWatcherTests()
        {
            if (Directory.Exists(RootFolder)) Directory.Delete(RootFolder, true);

            Directory.CreateDirectory(TestFolder);
        }

        private static readonly string RootFolder = $"test/{nameof(FileWatcherTests)}";
        private static readonly string TestFolder = $"{RootFolder}/test_folder";

        private async Task<T> SubscribeFirstAsync<T>(IObservable<T> observable)
        {
            using var cts = new CancellationTokenSource(30000);
            var tcs = new TaskCompletionSource<T>();
            observable
                .FirstAsync()
                .Subscribe(
                    x => tcs.SetResult(x),
                    cts.Token);

            return await tcs.Task;
        }


        private bool IsPathEqual(string a, string b)
        {
            return Path.GetFullPath(a) == Path.GetFullPath(b);
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

            var firstEventTask = SubscribeFirstAsync(watcher.FileObservable());

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

            var firstEventTask = SubscribeFirstAsync(watcher.FileObservable());

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

            var firstEventTask = SubscribeFirstAsync(watcher.FileObservable());

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

            var firstEventTask = SubscribeFirstAsync(watcher.FileObservable());

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