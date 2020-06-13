using System.Collections.Generic;
using System.Threading.Tasks;
using Gobi.InSync.App.Persistence;
using Gobi.InSync.App.Persistence.Models;

namespace Gobi.InSync.App.Services
{
    public interface ISyncService
    {
        Task StartAsync(IUnitOfWork unitOfWork);
        Task AddWatchAsync(IUnitOfWork unitOfWork, SyncWatch syncWatch);
        Task<List<SyncWatch>> DeleteWatchesAsync(IUnitOfWork unitOfWork, string sourceFolder, string targetFolder);
        Task<List<SyncWatch>> GetWatchesAsync(IUnitOfWork unitOfWork);
    }
}