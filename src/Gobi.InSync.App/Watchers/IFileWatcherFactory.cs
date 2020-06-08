namespace Gobi.InSync.App.Watchers
{
    public interface IFileWatcherFactory
    {
        IFileWatcher Create(string path);
    }
}