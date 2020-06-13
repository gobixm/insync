using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Gobi.InSync.App.Dispatchers;
using Gobi.InSync.App.Helpers;
using Gobi.InSync.App.Services.Models;
using Gobi.InSync.App.Synchronizers;
using Gobi.InSync.App.Watchers;

namespace Gobi.InSync.App.Services
{
    public sealed class WatchService : IWatchService
    {
        private readonly IFileEventDispatcher _fileEventDispatcher;
        private readonly IFileWatcherFactory _fileWatcherFactory;
        private readonly IFolderSynchronizer _folderSynchronizer;
        private readonly ConcurrentDictionary<string, Unwatch> _watchers = new ConcurrentDictionary<string, Unwatch>();

        public WatchService(
            IFolderSynchronizer folderSynchronizer,
            IFileWatcherFactory fileWatcherFactory,
            IFileEventDispatcher fileEventDispatcher
        )
        {
            _folderSynchronizer = folderSynchronizer;
            _fileWatcherFactory = fileWatcherFactory;
            _fileEventDispatcher = fileEventDispatcher;
        }

        public void Dispose()
        {
            _watchers?
                .Values
                .ToList()
                .ForEach(x => x.Dispose());
        }

        public WatchFolder StartWatching(string sourceFolder, string targetFolder)
        {
            if (string.IsNullOrEmpty(sourceFolder))
                throw new ArgumentException($"{nameof(sourceFolder)} can't be null or empty");

            if (!Directory.Exists(sourceFolder)) throw new DirectoryNotFoundException(sourceFolder);

            var fullSourcePath = Path.GetFullPath(sourceFolder);
            var fullTargetPath = Path.GetFullPath(targetFolder);

            ThrowIfRelative(sourceFolder, targetFolder);

            if (_watchers.Keys.Any(x => PathUtils.IsSubPath(x, fullSourcePath)))
                throw new ArgumentException("Path or it parent already added.");

            RemoveSubWatches(fullSourcePath);

            var unwatch = SynchronizeAndWatch(fullSourcePath, fullTargetPath);
            _watchers[BuildKey(sourceFolder, targetFolder)] = unwatch;
            return unwatch.WatchFolder;
        }

        public void StopWatching(string sourceFolder, string targetFolder)
        {
            _watchers.TryRemove(BuildKey(sourceFolder, targetFolder), out var removed);
            removed?.Dispose();
        }

        private string BuildKey(string sourceFolder, string targetFolder)
        {
            return $"{sourceFolder}:{targetFolder}";
        }

        private void RemoveSubWatches(string fullSourcePath)
        {
            var keysToUnwatch = _watchers
                .Where(x => PathUtils.IsSubPath(fullSourcePath, x.Value.WatchFolder.Source))
                .Select(x => x.Key);
            foreach (var key in keysToUnwatch)
            {
                _watchers.TryRemove(key, out var removed);
                removed?.Dispose();
            }
        }

        private static void ThrowIfRelative(string sourceFolder, string targetFolder)
        {
            if (PathUtils.IsSubPath(sourceFolder, targetFolder))
                throw new ArgumentException("Target path is a part of source path.");

            if (PathUtils.IsSubPath(targetFolder, sourceFolder))
                throw new ArgumentException("Source path is a part of target path.");
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