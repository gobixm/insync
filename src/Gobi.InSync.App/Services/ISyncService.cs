using System;
using System.Collections.Generic;
using Gobi.InSync.App.Services.Models;

namespace Gobi.InSync.App.Services
{
    public interface ISyncService : IDisposable
    {
        WatchFolder AddSyncFolder(string sourceFolder, string targetFolder);
        void RemoveSyncFolder(string sourceFolder);
        List<WatchFolder> GetSyncFolders(string sourceFolder);
    }
}