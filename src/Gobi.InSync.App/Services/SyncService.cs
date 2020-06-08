using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gobi.InSync.App.Dispatchers;
using Gobi.InSync.App.Services.Models;
using Gobi.InSync.App.Synchronizers;
using Gobi.InSync.App.Watchers;

namespace Gobi.InSync.App.Services
{
    public sealed class SyncService
    {
        private readonly IFileEventDispatcher _fileEventDispatcher;
        private readonly IFileWatcherFactory _fileWatcherFactory;
        private readonly IFolderSynchronizer _folderSynchronizer;
        private readonly ConcurrentDictionary<string, Unwatch> _watchers = new ConcurrentDictionary<string, Unwatch>();

        public SyncService(
            IFolderSynchronizer folderSynchronizer,
            IFileWatcherFactory fileWatcherFactory,
            IFileEventDispatcher fileEventDispatcher
        )
        {
            _folderSynchronizer = folderSynchronizer;
            _fileWatcherFactory = fileWatcherFactory;
            _fileEventDispatcher = fileEventDispatcher;
        }

        public void AddSyncFolder(string sourceFolder, string targetFolder)
        {
            if (string.IsNullOrEmpty(sourceFolder))
                throw new ArgumentException($"{nameof(sourceFolder)} can't be null or empty");

            if (!Directory.Exists(sourceFolder)) throw new DirectoryNotFoundException(sourceFolder);

            var fullSourcePath = Path.GetFullPath(sourceFolder);
            var fullTargetPath = Path.GetFullPath(targetFolder);

            ThrowIfRelative(sourceFolder, targetFolder);

            if (_watchers.Keys.Any(x => Path.GetRelativePath(x, fullSourcePath) != fullSourcePath))
                throw new ArgumentException("Path or it parent already added.");

            RemoveSubWatches(fullSourcePath);

            _watchers[fullSourcePath] = SynchronizeAndWatch(fullSourcePath, fullTargetPath);
        }

        private void RemoveSubWatches(string fullSourcePath)
        {
            foreach (var subPath in _watchers.Keys.Where(x => Path.GetRelativePath(fullSourcePath, x) != x))
                _watchers.TryRemove(subPath, out _);
        }

        private static void ThrowIfRelative(string sourceFolder, string targetFolder)
        {
            if (Path.GetRelativePath(sourceFolder, targetFolder) != targetFolder)
                throw new ArgumentException("Target path is a part of source path.");

            if (Path.GetRelativePath(targetFolder, sourceFolder) != sourceFolder)
                throw new ArgumentException("Source path is a part of target path.");
        }

        public void RemoveSyncFolder(string sourceFolder)
        {
            _watchers.TryRemove(sourceFolder, out _);
        }

        public List<WatchFolder> GetSyncFolders(string sourceFolder)
        {
            return _watchers.Values.Select(x => x.WatchFolder)
                .Where(x => x.Source == sourceFolder)
                .ToList();
        }

        private Unwatch SynchronizeAndWatch(string fullSourcePath, string fullTargetPath)
        {
            var watcher = _fileWatcherFactory.Create(fullSourcePath);
            _folderSynchronizer.SyncFolder(fullSourcePath, fullTargetPath);
            watcher.Start();

            var subscription = watcher.FileObservable()
                .Subscribe(x => _fileEventDispatcher.Dispatch(fullSourcePath, fullTargetPath, x));

            var unwatch = new Unwatch(
                watcher,
                subscription,
                new WatchFolder
                {
                    Source = fullSourcePath,
                    Target = fullTargetPath
                }
            );
            return unwatch;
        }

        private sealed class Unwatch : IDisposable
        {
            private readonly IDisposable _subscription;
            private readonly IFileWatcher _watcher;

            public Unwatch(IFileWatcher watcher, IDisposable subscription, WatchFolder watchFolder)
            {
                _subscription = subscription;
                WatchFolder = watchFolder;
                _watcher = watcher;
            }

            public WatchFolder WatchFolder { get; }

            public void Dispose()
            {
                _subscription?.Dispose();
                _watcher?.Dispose();
            }
        }
    }
}