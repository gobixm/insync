using System.Reactive.Concurrency;

namespace Gobi.InSync.App.Watchers
{
    public class FileWatcherFactory : IFileWatcherFactory
    {
        public IFileWatcher Create(string path)
        {
            return new FileWatcher(path, Scheduler.Default);
        }
    }
}