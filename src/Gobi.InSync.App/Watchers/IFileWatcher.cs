using System;
using Gobi.InSync.App.Watchers.Models;

namespace Gobi.InSync.App.Watchers
{
    public interface IFileWatcher : IDisposable
    {
        void Start();
        IObservable<IFileEvent> FileObservable();
    }
}