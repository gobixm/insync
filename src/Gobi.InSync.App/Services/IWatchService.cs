using System;
using Gobi.InSync.App.Services.Models;

namespace Gobi.InSync.App.Services
{
    public interface IWatchService : IDisposable
    {
        WatchFolder StartWatching(string sourceFolder, string targetFolder);
        void StopWatching(string sourceFolder, string targetFolder);
    }
}