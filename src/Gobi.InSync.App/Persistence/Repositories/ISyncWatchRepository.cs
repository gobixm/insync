using System.Collections.Generic;
using System.Threading.Tasks;
using Gobi.InSync.App.Persistence.Models;

namespace Gobi.InSync.App.Persistence.Repositories
{
    public interface ISyncWatchRepository
    {
        Task AddAsync(SyncWatch watch);
        Task<List<SyncWatch>> GetAllAsync();
        Task<SyncWatch> GetAsync(string sourcePath, string targetPath);
        Task<List<SyncWatch>> GetBySourceAsync(string sourcePath);
        void Delete(IEnumerable<SyncWatch> watches);
    }
}