using Gobi.InSync.App.Watchers.Models;

namespace Gobi.InSync.App.Dispatchers
{
    public interface IFileEventDispatcher
    {
        void Dispatch(string sourceFolder, string targetFolder, IFileEvent fileEvent);
    }
}