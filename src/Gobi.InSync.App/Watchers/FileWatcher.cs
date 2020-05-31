using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Gobi.InSync.App.Watchers.Models;

namespace Gobi.InSync.App.Watchers
{
    public sealed class FileWatcher : IDisposable
    {
        private readonly IScheduler _scheduler;
        private readonly FileSystemWatcher _watcher;

        public FileWatcher(string path, IScheduler scheduler = null)
        {
            _scheduler = scheduler ?? Scheduler.Default;
            _watcher = new FileSystemWatcher
            {
                Path = Path.GetFullPath(path),
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite
                               | NotifyFilters.LastAccess
                               | NotifyFilters.FileName
                               | NotifyFilters.DirectoryName
            };
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }

        public void Start()
        {
            _watcher.EnableRaisingEvents = true;
        }

        public IObservable<IFileEvent> FileObservable()
        {
            return Observable.Merge<IFileEvent>(
                FileChangedObservable(),
                FileCreatedObservable(),
                FileDeletedObservable(),
                FileRenamedObservable()
            ).ObserveOn(_scheduler);
        }

        private IObservable<FileChanged> FileChangedObservable()
        {
            return Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                handler => _watcher.Changed += handler,
                handler => _watcher.Changed -= handler
            ).Select(x => new FileChanged
            {
                Path = x.EventArgs.FullPath,
                FileName = x.EventArgs.Name
            });
        }

        private IObservable<FileCreated> FileCreatedObservable()
        {
            return Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    handler => _watcher.Created += handler,
                    handler => _watcher.Created -= handler
                )
                .Select(x => new FileCreated
                {
                    Path = x.EventArgs.FullPath,
                    FileName = x.EventArgs.Name
                });
        }

        private IObservable<FileDeleted> FileDeletedObservable()
        {
            return Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                handler => _watcher.Deleted += handler,
                handler => _watcher.Deleted -= handler
            ).Select(x => new FileDeleted
            {
                Path = x.EventArgs.FullPath,
                FileName = x.EventArgs.Name
            });
        }

        private IObservable<FileRenamed> FileRenamedObservable()
        {
            return Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
                handler => _watcher.Renamed += handler,
                handler => _watcher.Renamed -= handler
            ).Select(x => new FileRenamed
            {
                Path = x.EventArgs.FullPath,
                FileName = x.EventArgs.Name,
                OldPath = x.EventArgs.OldFullPath,
                OldFileName = x.EventArgs.OldName
            });
        }
    }
}