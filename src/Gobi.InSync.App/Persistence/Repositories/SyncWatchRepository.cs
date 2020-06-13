using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gobi.InSync.App.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Gobi.InSync.App.Persistence.Repositories
{
    public class SyncWatchRepository : ISyncWatchRepository
    {
        private readonly InSyncDbContext _context;

        public SyncWatchRepository(InSyncDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SyncWatch watch)
        {
            await _context.AddAsync(watch);
        }

        public async Task<List<SyncWatch>> GetAllAsync()
        {
            return await _context.Watches.ToListAsync();
        }

        public async Task<SyncWatch> GetAsync(string sourcePath, string targetPath)
        {
            return await _context.Watches.FindAsync(sourcePath, targetPath);
        }

        public async Task<List<SyncWatch>> GetBySourceAsync(string sourcePath)
        {
            return await _context.Watches.Where(x => x.SourcePath == sourcePath)
                .ToListAsync();
        }

        public void Delete(IEnumerable<SyncWatch> watches)
        {
            foreach (var syncWatch in watches)
            {
                _context.Remove(syncWatch);
            }
        }
    }
}